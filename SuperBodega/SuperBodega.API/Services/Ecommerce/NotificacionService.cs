using Microsoft.EntityFrameworkCore;
using SuperBodega.API.Data;
using SuperBodega.API.Models.Ecommerce;
using SuperBodega.API.Repositories.Interfaces.Ecommerce;
using SuperBodega.API.DTOs.Ecommerce;

namespace SuperBodega.API.Services.Ecommerce;

public class NotificacionService
{
    private readonly SuperBodegaContext _context;
    private readonly IRabbitMQService _rabbitMqService;
    private readonly IEmailService _emailService;
    private readonly ILogger<NotificacionService> _logger;
    
    public NotificacionService(
        SuperBodegaContext context, 
        IRabbitMQService rabbitMqService,
        IEmailService emailService,
        ILogger<NotificacionService> logger)
    {
        _context = context;
        _rabbitMqService = rabbitMqService;
        _emailService = emailService;
        _logger = logger;
    }

    // Método para obtener notificaciones por cliente
    public async Task<List<NotificacionDTO>> GetNotificacionesPorClienteAsync(int clienteId)
    {
        _logger.LogInformation("Buscando notificaciones para cliente {ClienteId}", clienteId);
        
        // Añadir logging detallado
        var notificaciones = await _context.Notificaciones
            .Where(n => n.IdCliente == clienteId)
            .OrderByDescending(n => n.Fecha)
            .ToListAsync();
        
        // Agregar log para cada notificación encontrada
        foreach(var notif in notificaciones)
        {
            _logger.LogInformation("Notificación encontrada: ID={Id}, Cliente={ClienteId}, Estado={Estado}, Título={Titulo}",
                notif.Id, notif.IdCliente, notif.EstadoDeLaVenta, notif.Titulo);
        }
        
        _logger.LogInformation("Se encontraron {Count} notificaciones para cliente {ClienteId}", 
                             notificaciones.Count, clienteId);
        
        return notificaciones.Select(n => new NotificacionDTO
        {
            Id = n.Id,
            Titulo = n.Titulo,
            Mensaje = n.Mensaje,
            Fecha = n.Fecha,
            EstadoDeLaVenta = n.EstadoDeLaVenta,
            NumeroDeFactura = n.NumeroDeFactura,
            IdVenta = n.IdVenta,
            Leida = n.Leida
        }).ToList();
    }

    // Método para crear notificaciones internas
    public async Task EnviarNotificacionAsync(int ventaId)
    {
        try 
        {
            var venta = await _context.Ventas
                .Include(v => v.Cliente)
                .Include(v => v.EstadoDeLaVenta)
                .FirstOrDefaultAsync(v => v.Id == ventaId);
                    
            if (venta == null)
            {
                throw new Exception($"No se encontró la venta con ID {ventaId}");
            }

            TimeZoneInfo guatemalaZone;
                try {
                    // Intentar formato Windows
                    guatemalaZone = TimeZoneInfo.FindSystemTimeZoneById("Central America Standard Time");
                } 
                catch {
                    try {
                        // Intentar formato Linux
                        guatemalaZone = TimeZoneInfo.FindSystemTimeZoneById("America/Guatemala");
                    }
                    catch {
                        // Crear zona manualmente con UTC-6 para Guatemala
                        guatemalaZone = TimeZoneInfo.CreateCustomTimeZone("Guatemala", new TimeSpan(-6, 0, 0), "Guatemala", "GT");
                    }
                }
                
                // Fecha y hora del reporte
                DateTime localDateTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, guatemalaZone);
            
            // Crear la notificación interna
            var notificacion = new Notificacion
            {
                IdCliente = venta.IdCliente,
                IdVenta = venta.Id,
                Titulo = $"Actualización de pedido #{venta.NumeroDeFactura}",
                Mensaje = $"Tu pedido ha cambiado a estado: {venta.EstadoDeLaVenta.Nombre}",
                Fecha = localDateTime,
                Leida = false,
                EstadoDeLaVenta = venta.EstadoDeLaVenta.Nombre,
                NumeroDeFactura = venta.NumeroDeFactura
            };
            
            // Personalizar el mensaje según el estado
            switch (venta.IdEstadoDeLaVenta)
            {
                case 1: // Recibida
                    notificacion.Titulo = $"Pedido #{venta.NumeroDeFactura} recibido";
                    notificacion.Mensaje = $"¡Gracias por tu compra! Hemos recibido tu pedido y lo estamos preparando.";
                    break;
                case 2: // Despachada
                    notificacion.Titulo = $"Pedido #{venta.NumeroDeFactura} despachado";
                    notificacion.Mensaje = $"Tu pedido ha sido despachado y está en camino a tu dirección.";
                    break;
                case 3: // Entregada
                    notificacion.Titulo = $"Pedido #{venta.NumeroDeFactura} entregado";
                    notificacion.Mensaje = $"¡Tu pedido ha sido entregado! Esperamos que disfrutes de tus productos.";
                    break;
                case 4: // Devolución solicitada
                    notificacion.Titulo = $"Devolución solicitada para pedido #{venta.NumeroDeFactura}";
                    notificacion.Mensaje = $"Hemos recibido tu solicitud de devolución. Procesaremos tu solicitud lo antes posible.";
                    break;
                case 5: // Devolución completada
                    notificacion.Titulo = $"Devolución del pedido #{venta.NumeroDeFactura} completada";
                    notificacion.Mensaje = $"La devolución de tu pedido ha sido procesada exitosamente.";
                    break;
                default:
                    break;
            }
            
            // Guardar la notificación en la base de datos
            await _context.Notificaciones.AddAsync(notificacion);
            await _context.SaveChangesAsync();
            
            _logger.LogInformation("Notificación interna guardada para venta {ventaId}", ventaId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error al crear notificación interna para venta {ventaId}");
            throw;
        }
    }

    // Método para marcar una notificación como leída
    public async Task MarcarComoLeidaAsync(int notificacionId)
    {
        var notificacion = await _context.Notificaciones.FindAsync(notificacionId);
        if (notificacion != null)
        {
            notificacion.Leida = true;
            await _context.SaveChangesAsync();
        }
    }

    public async Task EnviarNotificacionCambioEstadoAsync(int ventaId)
    {
        try
        {
            _logger.LogInformation("Iniciando proceso de notificación para venta {ventaId}", ventaId);
    
            // Primero guardar la notificación interna
            await EnviarNotificacionAsync(ventaId);
            
            // Luego enviar el correo
            await NotificarCambioDelEstadoDeLaVentaRabbitMQ(ventaId);
    
            _logger.LogInformation("Notificación enviada correctamente para venta {ventaId}", ventaId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al enviar notificaciones para venta {ventaId}", ventaId);
            throw;
        }
    }

    // Método principal para enviar notificaciones - ahora con opción sincrónica/asincrónica
    public async Task EnviarNotificacionCambioEstadoAsync(int ventaId, bool usarModoSincrono = false)
    {
        try
        {
            _logger.LogInformation("Iniciando proceso de notificación para venta {ventaId}, modo sincrónico: {modeSincrono}", 
                ventaId, usarModoSincrono);

            // Primero guardar la notificación interna (siempre se hace de forma sincrónica)
            await EnviarNotificacionAsync(ventaId);
                
            if (usarModoSincrono)
            {
                // Modo sincrónico: enviar email directamente
                await EnviarEmailDirectoAsync(ventaId);
                _logger.LogInformation("Notificación por email enviada directamente (modo sincrónico) para venta {ventaId}", ventaId);
            }
            else
            {
                // Modo asincrónico: usar RabbitMQ (comportamiento actual)
                await NotificarCambioDelEstadoDeLaVentaRabbitMQ(ventaId);
                _logger.LogInformation("Notificación por email encolada en RabbitMQ (modo asincrónico) para venta {ventaId}", ventaId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al enviar notificaciones para venta {ventaId}", ventaId);
            throw;
        }
    }

    // Método para enviar emails directamente sin pasar por la cola
    public async Task<bool> EnviarEmailDirectoAsync(int ventaId)
    {
        try
        {
            _logger.LogInformation("Preparando email directo (sincrónico) para venta {ventaId}", ventaId);
            
            var venta = await _context.Ventas
                .Include(v => v.Cliente)
                .Include(v => v.EstadoDeLaVenta)
                .Include(v => v.DetallesDeLaVenta)
                    .ThenInclude(d => d.Producto)
                        .ThenInclude(p => p.Categoria)
                .FirstOrDefaultAsync(v => v.Id == ventaId);

            if (venta == null)
            {
                _logger.LogWarning("No se pudo encontrar la venta {ventaId} para enviar email", ventaId);
                return false;
            }

            // Verificar que el cliente tenga email
            if (string.IsNullOrEmpty(venta.Cliente?.Email))
            {
                _logger.LogWarning("Cliente de la venta {ventaId} no tiene email configurado", ventaId);
                return false;
            }

            // Crear notificación de email
            var notificacion = new NotificacionEmail
            {
                Para = venta.Cliente.Email,
                IdVenta = venta.Id,
                NumeroDeFactura = venta.NumeroDeFactura,
                EstadoDeLaVenta = venta.EstadoDeLaVenta.Nombre,
                FechaDeRegistro = venta.FechaDeRegistro,
                NombreCompletoDelCliente = $"{venta.Cliente.Nombre} {venta.Cliente.Apellido}".Trim(),
                EmailDelCliente = venta.Cliente.Email,
                MontoTotal = venta.MontoTotal,
                Productos = new List<DetalleNotificacionEmail>()
            };

            // Añadir detalles de productos
            foreach (var detalle in venta.DetallesDeLaVenta)
            {
                notificacion.Productos.Add(new DetalleNotificacionEmail
                {
                    NombreDelProducto = detalle.NombreDelProducto,
                    CodigoDelProducto = detalle.CodigoDelProducto,
                    ImagenUrlDelProducto = detalle.ImagenDelProducto,
                    CategoriaDelProducto = detalle.Producto?.Categoria?.Nombre ?? "Sin categoría",
                    Cantidad = detalle.Cantidad,
                    PrecioUnitario = detalle.PrecioDeVenta,
                    MontoDePago = venta.MontoDePago,
                    MontoDeCambio = venta.MontoDeCambio,
                    SubTotal = detalle.MontoTotal
                });
            }

            // Configurar asunto y contenido según estado
            ConfigurarContenidoSegunEstado(notificacion, venta.IdEstadoDeLaVenta);
                
            // Enviar email directamente usando el servicio de email
            _logger.LogInformation("Enviando email directo a {email}", notificacion.Para);
            return await _emailService.EnviarEmailAsync(notificacion);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al enviar email directo para venta {ventaId}", ventaId);
            return false;
        }
    }

    public async Task NotificarCambioDelEstadoDeLaVentaRabbitMQ(int ventaId, int? nuevoEstadoId = null)
    {
        try
        {
            _logger.LogInformation("Preparando email de notificación para venta {ventaId}", ventaId);
            
            var venta = await _context.Ventas
                .Include(v => v.Cliente)
                .Include(v => v.EstadoDeLaVenta)
                .Include(v => v.DetallesDeLaVenta)
                    .ThenInclude(d => d.Producto)
                        .ThenInclude(p => p.Categoria)
                .FirstOrDefaultAsync(v => v.Id == ventaId);
    
            if (venta == null)
            {
                _logger.LogWarning("No se pudo encontrar la venta {ventaId} para enviar notificación", ventaId);
                return;
            }
    
            // Revisar si el cliente tiene email
            if (string.IsNullOrEmpty(venta.Cliente?.Email))
            {
                _logger.LogWarning("La venta {ventaId} tiene un cliente sin email configurado", ventaId);
                return;
            }
    
            var notificacion = new NotificacionEmail
            {
                Para = venta.Cliente.Email,
                IdVenta = venta.Id,
                NumeroDeFactura = venta.NumeroDeFactura,
                EstadoDeLaVenta = venta.EstadoDeLaVenta.Nombre,
                FechaDeRegistro = venta.FechaDeRegistro,
                NombreCompletoDelCliente = $"{venta.Cliente.Nombre} {venta.Cliente.Apellido}".Trim(),
                EmailDelCliente = venta.Cliente.Email,
                MontoTotal = venta.MontoTotal,
                Productos = new List<DetalleNotificacionEmail>()
            };
    
            foreach (var detalle in venta.DetallesDeLaVenta)
            {
                notificacion.Productos.Add(new DetalleNotificacionEmail
                {
                    NombreDelProducto = detalle.NombreDelProducto,
                    CodigoDelProducto = detalle.CodigoDelProducto,
                    ImagenUrlDelProducto = detalle.ImagenDelProducto,
                    CategoriaDelProducto = detalle.Producto?.Categoria?.Nombre ?? "Sin categoría",
                    Cantidad = detalle.Cantidad,
                    PrecioUnitario = detalle.PrecioDeVenta,
                    MontoDePago = detalle.Venta.MontoDePago,
                    MontoDeCambio = detalle.Venta.MontoDeCambio,
                    SubTotal = detalle.MontoTotal
                });
            }
    
            // Usar el estado pasado como parámetro o el de la venta
            int estadoId = nuevoEstadoId ?? venta.IdEstadoDeLaVenta;
            ConfigurarContenidoSegunEstado(notificacion, estadoId);
            
            _logger.LogInformation("Enviando email de notificación para venta {ventaId} a {email}", 
                    ventaId, venta.Cliente.Email);
            
            // Usar RabbitMQ para envío asíncrono
            _rabbitMqService.EnviarNotificacionEmail(notificacion);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al preparar la notificación por email para venta {ventaId}", ventaId);
            // No relanzar la excepción para evitar que la notificación principal falle
        }
    }

    private void ConfigurarContenidoSegunEstado(NotificacionEmail notificacion, int estadoId)
    {
        switch (estadoId)
        {
            case 1: // recibida
                notificacion.Asunto = $"Pedido #{notificacion.NumeroDeFactura} - Recibido";
                notificacion.Contenido = $"¡Gracias por tu compra! Hemos recibido tu pedido #{notificacion.NumeroDeFactura}. " +
                                         $"El estado de tu pedido es: {notificacion.EstadoDeLaVenta}.";
                break;
            case 2: // Despachada
                notificacion.Asunto = $"Pedido #{notificacion.NumeroDeFactura} - Despachado";
                notificacion.Contenido = $"¡Tu pedido #{notificacion.NumeroDeFactura} ha sido despachado! " +
                                         $"El paquete ha sido despachado y está en camino a tu dirección";
                break;
            case 3: // Entregado
                notificacion.Asunto = $"Pedido #{notificacion.NumeroDeFactura} - Entregado";
                notificacion.Contenido = $"¡Tu pedido #{notificacion.NumeroDeFactura} ha sido entregado! " +
                                         $"Esperamos que disfrutes de tu compra. ¡Gracias por confiar en SuperBodega!";
                break;
            case 4: // Devolucion Solicitada
                notificacion.Asunto = $"Pedido #{notificacion.NumeroDeFactura} - Devolución Solicitada";
                notificacion.Contenido = $"Hemos recibido tu solicitud de devolución para el pedido #{notificacion.NumeroDeFactura}. " +
                                         $"Estamos procesando tu solicitud y te mantendremos informado.";
                break;
            case 5: // Devolucion Completada
                notificacion.Asunto = $"Pedido #{notificacion.NumeroDeFactura} - Devolución completada";
                notificacion.Contenido = $"La devolución de tu pedido #{notificacion.NumeroDeFactura} ha sido procesada exitosamente. " +
                                         $"Los productos han sido retornados a nuestro inventario. Gracias por elegirnos.";
                break; 
            default:
                notificacion.Asunto = $"Actualización de pedido #{notificacion.NumeroDeFactura}";
                notificacion.Contenido = $"Tu pedido #{notificacion.NumeroDeFactura} ha sido actualizado. " +
                                         $"El nuevo estado es: {notificacion.EstadoDeLaVenta}.";
                break;
        }
    }
}