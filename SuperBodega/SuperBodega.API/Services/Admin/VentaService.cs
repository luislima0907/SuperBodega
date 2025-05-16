using Microsoft.EntityFrameworkCore;
using SuperBodega.API.Data;
using SuperBodega.API.DTOs.Admin;
using SuperBodega.API.Models.Admin;
using SuperBodega.API.Repositories.Interfaces.Admin;
using SuperBodega.API.Services.Ecommerce;

namespace SuperBodega.API.Services.Admin
{
    public class VentaService
    {
        private readonly IVentaRepository _ventaRepository;
        private readonly IDetalleDeLaVentaRepository _detalleDeLaVentaRepository;
        private readonly IEstadoDeLaVentaRepository _estadoDeLaVentaRepository;
        private readonly SuperBodegaContext _context;
        private readonly NotificacionService _notificacionService;

        public VentaService
        (IVentaRepository ventaRepository, 
        IDetalleDeLaVentaRepository detalleDeLaVentaRepository, 
        IEstadoDeLaVentaRepository estadoDeLaVentaRepository,
        NotificacionService notificacionService,
        SuperBodegaContext context)
        {
            _ventaRepository = ventaRepository;
            _detalleDeLaVentaRepository = detalleDeLaVentaRepository;
            _estadoDeLaVentaRepository = estadoDeLaVentaRepository;
            _notificacionService = notificacionService;
            _context = context;
        }

        public async Task<VentaDTO> GetVentaByIdWithDetailsAsync(int id)
        {
            var venta = await _ventaRepository.GetWithDetailsAsync(id);
            if (venta == null) return null;
            
            // Asegurar que se cargue la información completa incluyendo el cliente
            if (venta.Cliente == null)
            {
                // Cargar explícitamente el cliente si no se cargó
                await _context.Entry(venta).Reference(v => v.Cliente).LoadAsync();
                await _context.Entry(venta).Reference(v => v.EstadoDeLaVenta).LoadAsync();
            }
        
            // Cargar categorías para cada producto
            foreach (var detalle in venta.DetallesDeLaVenta)
            {
                if (detalle.Producto != null && detalle.Producto.Categoria == null)
                {
                    await _context.Entry(detalle.Producto).Reference(p => p.Categoria).LoadAsync();
                }
            }
            
            return new VentaDTO
            {
                Id = venta.Id,
                NumeroDeFactura = venta.NumeroDeFactura,
                IdCliente = venta.IdCliente,
                NombreCompletoCliente = venta.Cliente != null ? $"{venta.Cliente.Nombre} {venta.Cliente.Apellido}" : "Cliente no encontrado",
                EmailCliente = venta.Cliente?.Email ?? "Email no encontrado",
                IdEstadoDeLaVenta = venta.IdEstadoDeLaVenta,
                NombreEstadoDeLaVenta = venta.EstadoDeLaVenta?.Nombre ?? "Estado no encontrado",
                FechaDeRegistro = venta.FechaDeRegistro,
                MontoTotal = venta.MontoTotal,
                MontoDePago = venta.MontoDePago,
                MontoDeCambio = venta.MontoDeCambio,
                DetallesDeLaVenta = venta.DetallesDeLaVenta?.Select(d => new DetalleDeLaVentaDTO
                {
                    Id = d.Id,
                    IdVenta = d.IdVenta,
                    IdProducto = d.IdProducto,
                    NombreDelProducto = d.NombreDelProducto ?? d.Producto?.Nombre ?? "Producto no encontrado",
                    CodigoDelProducto = d.CodigoDelProducto ?? d.Producto?.Codigo ?? "N/A",
                    ImagenDelProducto = d.ImagenDelProducto ?? d.Producto?.ImagenUrl ?? "/images/productos/default.png",
                    NombreCategoria = d.Producto?.Categoria?.Nombre ?? "Categoría no encontrada",
                    PrecioDeVenta = d.PrecioDeVenta,
                    Cantidad = d.Cantidad,
                    MontoTotal = d.MontoTotal,
                    FechaDeRegistro = d.FechaDeRegistro
                }).ToList() ?? new List<DetalleDeLaVentaDTO>()
            };
        }

        public async Task<IEnumerable<VentaDTO>> GetVentasByClienteIdAsync(int clienteId)
        {
            var ventas = await _ventaRepository.GetByClienteIdAsync(clienteId);
            return ventas.Select(v => new VentaDTO
            {
                Id = v.Id,
                NumeroDeFactura = v.NumeroDeFactura,
                IdCliente = v.IdCliente,
                NombreCompletoCliente = v.Cliente != null ? $"{v.Cliente.Nombre} {v.Cliente.Apellido}" : "Cliente no encontrado",
                EmailCliente = v.Cliente?.Email ?? "Email no encontrado",
                IdEstadoDeLaVenta = v.IdEstadoDeLaVenta,
                NombreEstadoDeLaVenta = v.EstadoDeLaVenta?.Nombre ?? "Estado no encontrado",
                MontoTotal = v.MontoTotal,
                MontoDePago = v.MontoDePago,
                MontoDeCambio = v.MontoDeCambio,
                FechaDeRegistro = v.FechaDeRegistro,
                // Añadir mapeo de detalles
                DetallesDeLaVenta = v.DetallesDeLaVenta?.Select(d => new DetalleDeLaVentaDTO
                {
                    Id = d.Id,
                    IdVenta = d.IdVenta,
                    IdProducto = d.IdProducto,
                    CodigoDelProducto = d.CodigoDelProducto,
                    NombreDelProducto = d.NombreDelProducto,
                    ImagenDelProducto = d.ImagenDelProducto ?? d.Producto.ImagenUrl ?? "/images/productos/default.png",
                    NombreCategoria = d.Producto.Categoria?.Nombre ?? "Categoría no encontrada",
                    PrecioDeVenta = d.PrecioDeVenta,
                    Cantidad = d.Cantidad,
                    MontoTotal = d.MontoTotal,
                    FechaDeRegistro = d.FechaDeRegistro
                }).ToList() ?? new List<DetalleDeLaVentaDTO>()
            });
        }

        public async Task<IEnumerable<VentaDTO>> GetVentasByEstadoIdAsync(int estadoId)
        {
            var ventas = await _ventaRepository.GetByEstadoIdAsync(estadoId);
            return ventas.Select(v => new VentaDTO
            {
                Id = v.Id,
                NumeroDeFactura = v.NumeroDeFactura,
                IdCliente = v.IdCliente,
                NombreCompletoCliente = v.Cliente != null ? $"{v.Cliente.Nombre} {v.Cliente.Apellido}" : "Cliente no encontrado",
                EmailCliente = v.Cliente?.Email ?? "Email no encontrado",
                IdEstadoDeLaVenta = v.IdEstadoDeLaVenta,
                NombreEstadoDeLaVenta = v.EstadoDeLaVenta?.Nombre ?? "Estado no encontrado",
                MontoTotal = v.MontoTotal,
                MontoDePago = v.MontoDePago,
                MontoDeCambio = v.MontoDeCambio,
                FechaDeRegistro = v.FechaDeRegistro
            });
        }

        public async Task<IEnumerable<VentaDTO>> GetAllVentasWithDetailsAsync()
        {
            var ventas = await _ventaRepository.GetAllWithDetailsAsync();
            return ventas.Select(v => new VentaDTO
            {
                Id = v.Id,
                NumeroDeFactura = v.NumeroDeFactura,
                IdCliente = v.IdCliente,
                NombreCompletoCliente = v.Cliente != null ? $"{v.Cliente.Nombre} {v.Cliente.Apellido}" : "Cliente no encontrado",
                EmailCliente = v.Cliente?.Email ?? "Email no encontrado",
                IdEstadoDeLaVenta = v.IdEstadoDeLaVenta,
                NombreEstadoDeLaVenta = v.EstadoDeLaVenta?.Nombre ?? "Estado no encontrado",
                MontoTotal = v.MontoTotal,
                MontoDePago = v.MontoDePago,
                MontoDeCambio = v.MontoDeCambio,
                FechaDeRegistro = v.FechaDeRegistro,
                DetallesDeLaVenta = v.DetallesDeLaVenta?.Select(d => new DetalleDeLaVentaDTO
                {
                    Id = d.Id,
                    IdVenta = d.IdVenta,
                    IdProducto = d.IdProducto,
                    CodigoDelProducto = d.CodigoDelProducto,
                    NombreDelProducto = d.NombreDelProducto,
                    ImagenDelProducto = d.ImagenDelProducto ?? d.Producto?.ImagenUrl ?? "/images/productos/default.png",
                    NombreCategoria = d.Producto?.Categoria?.Nombre ?? "Categoría no encontrada",
                    PrecioDeVenta = d.PrecioDeVenta,
                    Cantidad = d.Cantidad,
                    MontoTotal = d.MontoTotal,
                    FechaDeRegistro = d.FechaDeRegistro
                }).ToList() ?? new List<DetalleDeLaVentaDTO>()
            });
        }

        public async Task<VentaDTO> CreateVentaAsync(CreateVentaDTO createDTO)
        {
            // Generar número de factura único con el formato especificado
            string numeroDeFactura = await GenerarNumeroFacturaUnico();
            
            // ¡IMPORTANTE! Usar la estrategia de ejecución en lugar de transacciones manuales
            return await _context.Database.CreateExecutionStrategy().ExecuteAsync(async () => 
            {
                using var transaction = await _context.Database.BeginTransactionAsync();
                
                try
                {
                    var venta = new Venta
                    {
                        IdCliente = createDTO.IdCliente,
                        NumeroDeFactura = numeroDeFactura,
                        MontoDePago = createDTO.MontoDePago,
                        IdEstadoDeLaVenta = 1, // "Recibida" por defecto
                        FechaDeRegistro = createDTO.FechaDeRegistro,
                        MontoTotal = 0 // Se calculará con los detalles
                    };
        
                    decimal montoTotal = 0;
                    var detalles = new List<DetalleDeLaVenta>();
                    
                    foreach (var detalle in createDTO.Detalles)
                    {
                        var producto = await _context.Productos.FindAsync(detalle.IdProducto);
                        if (producto == null)
                        {
                            throw new InvalidOperationException($"Producto con ID {detalle.IdProducto} no encontrado");
                        }
                        
                        // Verificar stock
                        if (producto.Stock < detalle.Cantidad)
                        {
                            throw new InvalidOperationException($"Stock insuficiente para el producto {producto.Nombre}");
                        }
                        
                        var detalleDeLaVenta = new DetalleDeLaVenta
                        {
                            IdProducto = detalle.IdProducto,
                            CodigoDelProducto = producto.Codigo,
                            NombreDelProducto = producto.Nombre,
                            ImagenDelProducto = producto.ImagenUrl ?? "/images/productos/default.png",
                            IdProveedor = detalle.IdProveedor,
                            NombreDelProveedor = detalle.NombreDelProveedor ?? "Proveedor no encontrado",
                            PrecioDeVenta = detalle.PrecioDeVenta,
                            Cantidad = detalle.Cantidad,
                            MontoTotal = detalle.PrecioDeVenta * detalle.Cantidad,
                            FechaDeRegistro = detalle.FechaDeRegistro,
                        };
                        
                        // Reducir stock
                        producto.Stock -= detalle.Cantidad;
                        _context.Productos.Update(producto);
                        
                        montoTotal += detalleDeLaVenta.MontoTotal;
                        detalles.Add(detalleDeLaVenta);
                    }
                    
                    // Asignar la lista de detalles y el monto total
                    venta.MontoTotal = montoTotal;
                    venta.MontoDeCambio = createDTO.MontoDePago - montoTotal;
                    venta.DetallesDeLaVenta = detalles;
                    
                    // Guardar la venta
                    _context.Ventas.Add(venta);
                    await _context.SaveChangesAsync();
                    
                    // Confirmar la transacción
                    await transaction.CommitAsync();

                    // await _notificacionService.NotificarCambioDelEstadoDeLaVentaRabbitMQ(venta.Id, venta.IdEstadoDeLaVenta);
                    
                    // await _notificacionService.EnviarNotificacionCambioEstadoAsync(venta.Id);

                    // Cargar datos relacionados para el DTO
                    var cliente = await _context.Clientes.FindAsync(venta.IdCliente);
                    var estado = await _context.EstadosDeLaVenta.FindAsync(venta.IdEstadoDeLaVenta);
                    
                    return new VentaDTO
                    {
                        Id = venta.Id,
                        NumeroDeFactura = venta.NumeroDeFactura,
                        IdCliente = venta.IdCliente,
                        NombreCompletoCliente = venta.Cliente != null ? $"{venta.Cliente.Nombre} {venta.Cliente.Apellido}" : "Cliente no encontrado",
                        EmailCliente = cliente?.Email ?? "Email no encontrado",
                        IdEstadoDeLaVenta = venta.IdEstadoDeLaVenta,
                        NombreEstadoDeLaVenta = estado?.Nombre ?? "Estado no encontrado",
                        MontoTotal = venta.MontoTotal,
                        MontoDePago = venta.MontoDePago,
                        MontoDeCambio = venta.MontoDeCambio,
                        FechaDeRegistro = venta.FechaDeRegistro,
                        DetallesDeLaVenta = detalles.Select(d => new DetalleDeLaVentaDTO
                        {
                            Id = d.Id,
                            IdVenta = d.IdVenta,
                            IdProducto = d.IdProducto,
                            PrecioDeVenta = d.PrecioDeVenta,
                            Cantidad = d.Cantidad,
                            MontoTotal = d.MontoTotal,
                            FechaDeRegistro = d.FechaDeRegistro
                        }).ToList()
                    };
                }
                catch (Exception ex)
                {
                    // Si hay un error, hacer rollback
                    await transaction.RollbackAsync();
                    throw new Exception($"Error al crear la venta: {ex.Message}", ex);
                }
            });
        }
        
        private async Task<string> GenerarNumeroFacturaUnico()
        {
            Random random = new Random();
            string numeroFactura;
            bool facturaExiste;
            
            do {
                char letra = (char)random.Next('A', 'Z' + 1);
                int numeros = random.Next(100, 1000); // Genera un número entre 100-999 (3 dígitos)
                numeroFactura = $"{letra}-{numeros}";
                
                // Verificar que el número no exista
                facturaExiste = await _context.Ventas.AnyAsync(v => v.NumeroDeFactura == numeroFactura);
            } while (facturaExiste);
            
            return numeroFactura;
        }

        public async Task<bool> ProcessReturnAsync(int ventaId, bool usarModoSincrono = false)
        {
            // Usar la estrategia de ejecución de EF Core en lugar de una transacción manual
            return await _context.Database.CreateExecutionStrategy().ExecuteAsync(async () => 
            {
                using var transaction = await _context.Database.BeginTransactionAsync();
                
                try
                {
                    var venta = await _context.Ventas
                        .Include(v => v.DetallesDeLaVenta)
                            .ThenInclude(d => d.Producto)
                        .FirstOrDefaultAsync(v => v.Id == ventaId);
                        
                    if (venta == null)
                        return false;
                        
                    // Validar que el estado sea "Devolución solicitada"
                    if (venta.IdEstadoDeLaVenta != 4) // "Devolución Solicitada"
                        return false;
                        
                    // Procesar la devolución - actualizar el stock
                    foreach (var detalle in venta.DetallesDeLaVenta)
                    {
                        if (detalle.Producto != null)
                        {
                            // Devolver productos al inventario
                            detalle.Producto.Stock += detalle.Cantidad;
                            _context.Productos.Update(detalle.Producto);
                        }
                    }
                    
                    // Intentar encontrar el estado "Devolución Completada" o crearlo si no existe
                    var estadoDevolucionCompletada = await _context.EstadosDeLaVenta
                        .FirstOrDefaultAsync(e => e.Nombre == "Devolución Completada");
                    
                    if (estadoDevolucionCompletada == null)
                    {
                        // Crear nuevo estado de devolución completada
                        estadoDevolucionCompletada = new EstadoDeLaVenta
                        {
                            Nombre = "Devolución Completada"
                        };
                        _context.EstadosDeLaVenta.Add(estadoDevolucionCompletada);
                        await _context.SaveChangesAsync(); // Guardar para obtener el ID asignado
                    }
                    
                    // Cambiar estado al de devolución completada
                    venta.IdEstadoDeLaVenta = estadoDevolucionCompletada.Id;
                    _context.Ventas.Update(venta);
                    
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    // Enviar notificación con el modo elegido
                    await _notificacionService.EnviarNotificacionCambioEstadoAsync(ventaId, usarModoSincrono);
                    
                    return true;
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    // Registrar el error para diagnóstico
                    Console.WriteLine($"Error en ProcessReturnAsync: {ex.Message}");
                    Console.WriteLine($"StackTrace: {ex.StackTrace}");
                    if (ex.InnerException != null)
                    {
                        Console.WriteLine($"InnerException: {ex.InnerException.Message}");
                        Console.WriteLine($"InnerException StackTrace: {ex.InnerException.StackTrace}");
                    }
                    throw; // Re-lanzar la excepción para que se maneje en el controlador
                }
            });
        }

        public async Task<bool> ChangeVentaStateAsync(int id, int IdEstadoDeLaVenta, bool usarModoSincrono = false)
        {
            return await _context.Database.CreateExecutionStrategy().ExecuteAsync(async () => 
            {
                try 
                {
                    var venta = await _context.Ventas
                        .Include(v => v.DetallesDeLaVenta)
                        .FirstOrDefaultAsync(v => v.Id == id);
                        
                    if (venta == null)
                        return false;
                    
                    // Validamos el nuevo estado
                    var estadoExiste = await _context.EstadosDeLaVenta.AnyAsync(e => e.Id == IdEstadoDeLaVenta);
                    if (!estadoExiste)
                        return false;
                    
                    // No permitir cambiar el estado si ya es "Devolución Solicitada" o "Devolución Completada"
                    if (venta.IdEstadoDeLaVenta == 4 || venta.IdEstadoDeLaVenta == 5)
                        return false;
                    
                    // **CASO ESPECIAL: Permitir cambiar a "Devolución Solicitada" desde Recibida o Entregada**
                    if (IdEstadoDeLaVenta == 4)
                    {
                        // Solo permitir solicitar devolución desde estado Recibida (1) o Entregada (3)
                        if (venta.IdEstadoDeLaVenta != 1 && venta.IdEstadoDeLaVenta != 3)
                        {
                            return false; // No se permite cambiar a devolución solicitada desde otros estados
                        }
                        
                        venta.IdEstadoDeLaVenta = IdEstadoDeLaVenta;
                        await _context.SaveChangesAsync();
                        await _notificacionService.EnviarNotificacionCambioEstadoAsync(id, usarModoSincrono);
                        return true;
                    }

                    // Si está en estado Devolución Solicitada (4), solo permitir cambio a Devolución Completada (5)
                    if (venta.IdEstadoDeLaVenta == 4 && IdEstadoDeLaVenta != 5)
                    {
                        return false;
                    }
                        
                    // **PARA OTROS CASOS NORMALES (NO DEVOLUCIÓN)**
                    // Seguir el flujo normal: Recibida -> Despachada -> Entregada
                        
                    // De Recibida solo se puede pasar a Despachada
                    if (venta.IdEstadoDeLaVenta == 1 && IdEstadoDeLaVenta != 2)
                        return false;
                        
                    // De Despachada solo se puede pasar a Entregada
                    if (venta.IdEstadoDeLaVenta == 2 && IdEstadoDeLaVenta != 3)
                        return false;
                        
                    // Cuando ya está Entregada, no se puede cambiar a otro estado excepto a Devolución Solicitada
                    // (Este caso ya está cubierto arriba, por lo que solo bloqueamos otros cambios)
                    if (venta.IdEstadoDeLaVenta == 3 && IdEstadoDeLaVenta != 4)
                        return false;
                        
                    // No permitir cambiar directamente a "Devolución Completada" desde esta API
                    if (IdEstadoDeLaVenta == 5)
                        return false;
                    
                    venta.IdEstadoDeLaVenta = IdEstadoDeLaVenta;
                    await _context.SaveChangesAsync();
                    await _notificacionService.EnviarNotificacionCambioEstadoAsync(id, usarModoSincrono);
                    return true;
                }
                catch (Exception ex)
                {
                    // Registrar el error para diagnóstico
                    Console.WriteLine($"Error en ChangeVentaStateAsync: {ex.Message}");
                    throw new Exception($"Error al cambiar el estado de la venta: {ex.Message}", ex);
                }
            });
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var venta = await _ventaRepository.GetByIdAsync(id);
            if (venta == null) return false;
        
            var result = await _ventaRepository.DeleteAsync(id);
            return result;
        }
    }
}