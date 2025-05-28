using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SuperBodega.API.Data;
using SuperBodega.API.ViewModels;

namespace SuperBodega.API.Controllers.Admin
{
    /// <summary>
    /// Controlador para gestionar los dashboards de la aplicación
    /// </summary>
    /// <remarks>
    /// Este controlador proporciona acceso a las vistas de los dashboards
    /// para los administradores y clientes.
    /// </remarks>
    /// <response code="200">Retorna la vista del dashboard correspondiente</response>
    /// <response code="500">Error interno del servidor</response>
    /// <response code="404">Vista no encontrada</response>
    [ApiExplorerSettings(GroupName = "Dashboard")]
    [Route("Dashboard")]
    public class DashboardController : Controller
    {
        private readonly ILogger<DashboardController> _logger;
        private readonly SuperBodegaContext _context;

        public DashboardController(ILogger<DashboardController> logger, SuperBodegaContext context)
        {
            _logger = logger;
            _context = context;
        }
        
        /// <summary>
        /// Muestra el dashboard de administrador con datos reales
        /// </summary>
        [HttpGet("AdminDashboard")]
        public async Task<IActionResult> AdminDashboard()
        {
            _logger.LogInformation("Accediendo al dashboard de administrador");
            
            var viewModel = new AdminDashboardViewModel
            {
                // Datos para las tarjetas de estadísticas
                TotalProductos = await _context.Productos.CountAsync(),
                TotalClientes = await _context.Clientes.CountAsync(),
                TotalProveedores = await _context.Proveedores.CountAsync(),
                
                // Ventas del mes actual
                VentasDelMes = await _context.Ventas
                    .Where(v => v.FechaDeRegistro.Month == DateTime.Now.Month && 
                                v.FechaDeRegistro.Year == DateTime.Now.Year)
                    .SumAsync(v => v.MontoTotal),
                
                // Ventas recientes (las últimas 5)
                VentasRecientes = await _context.Ventas
                    .Include(v => v.Cliente)
                    .Include(v => v.EstadoDeLaVenta)
                    .OrderByDescending(v => v.FechaDeRegistro)
                    .Take(5)
                    .Select(v => new VentaRecienteViewModel
                    {
                        Id = v.Id,
                        NumeroFactura = v.NumeroDeFactura,
                        NombreCliente = $"{v.Cliente.Nombre} {v.Cliente.Apellido}",
                        FechaRegistro = v.FechaDeRegistro,
                        MontoTotal = v.MontoTotal,
                        Estado = v.EstadoDeLaVenta.Nombre
                    })
                    .ToListAsync(),
                
                // Productos con stock bajo (menos o igual a 5 unidades)
                ProductosStockBajo = await _context.Productos
                    .Include(p => p.Categoria)
                    .Where(p => p.Stock <= 5 && p.Estado)
                    .OrderBy(p => p.Stock)
                    .Take(5)
                    .Select(p => new ProductoStockBajoViewModel
                    {
                        Id = p.Id,
                        Nombre = p.Nombre,
                        Categoria = p.Categoria.Nombre,
                        StockActual = p.Stock,
                        StockMinimo = 5 // Valor fijo para ejemplo, podrías tener un campo en tu DB
                    })
                    .ToListAsync(),
                
                // Productos más vendidos
                ProductosMasVendidos = await _context.DetallesDeLaVenta
                    .GroupBy(d => d.IdProducto)
                    .Select(g => new 
                    { 
                        ProductoId = g.Key,
                        CantidadVendida = g.Sum(d => d.Cantidad),
                        NombreProducto = g.First().NombreDelProducto
                    })
                    .OrderByDescending(x => x.CantidadVendida)
                    .Take(5)
                    .Select(x => new ProductoMasVendidoViewModel
                    {
                        Nombre = x.NombreProducto,
                        CantidadVendida = x.CantidadVendida,
                        Porcentaje = 0 // Se calculará después
                    })
                    .ToListAsync(),
                
                // Actividades recientes (ventas, nuevos productos, etc.)
                UltimasActividades = await GetUltimasActividadesAsync()
            };
            
            // Calcular porcentajes para productos más vendidos
            if (viewModel.ProductosMasVendidos.Any())
            {
                int totalVendido = viewModel.ProductosMasVendidos.Sum(p => p.CantidadVendida);
                foreach (var producto in viewModel.ProductosMasVendidos)
                {
                    producto.Porcentaje = (int)Math.Round((double)producto.CantidadVendida / totalVendido * 100);
                }
            }
            
            return View(viewModel);
        }
        
        /// <summary>
        /// Muestra el dashboard de cliente con datos reales
        /// </summary>
        [HttpGet("ClienteDashboard")]
        public async Task<IActionResult> ClienteDashboard()
        {
            _logger.LogInformation("Accediendo al dashboard de cliente");
            
            // NOTA: En un sistema real, obtendrías el ID del cliente de la sesión o token
            // Para este ejemplo, usaremos el primer cliente activo
            var cliente = await _context.Clientes.FirstOrDefaultAsync(c => c.Estado);
            
            if (cliente == null)
            {
                return View(new ClienteDashboardViewModel
                {
                    NoHayCliente = true
                });
            }
            
            var viewModel = new ClienteDashboardViewModel
            {
                ClienteId = cliente.Id,
                ClienteNombre = $"{cliente.Nombre} {cliente.Apellido}",
                
                // Estadísticas de pedidos
                TotalPedidos = await _context.Ventas
                    .Where(v => v.IdCliente == cliente.Id)
                    .CountAsync(),
                
                PedidosEntregados = await _context.Ventas
                    .Where(v => v.IdCliente == cliente.Id && v.IdEstadoDeLaVenta == 3) // Entregados
                    .CountAsync(),
                
                PedidosEnTransito = await _context.Ventas
                    .Where(v => v.IdCliente == cliente.Id && v.IdEstadoDeLaVenta == 2) // Despachados/En tránsito
                    .CountAsync(),
                
                // Artículos en carrito
                ArticulosEnCarrito = await _context.Carritos
                    .Where(c => c.ClienteId == cliente.Id)
                    .SelectMany(c => c.Elementos)
                    .SumAsync(e => e.Cantidad),
                
                // Pedidos recientes
                PedidosRecientes = await _context.Ventas
                    .Include(v => v.EstadoDeLaVenta)
                    .Where(v => v.IdCliente == cliente.Id)
                    .OrderByDescending(v => v.FechaDeRegistro)
                    .Take(4)
                    .Select(v => new PedidoRecienteViewModel
                    {
                        Id = v.Id,
                        NumeroFactura = v.NumeroDeFactura,
                        FechaRegistro = v.FechaDeRegistro,
                        MontoTotal = v.MontoTotal,
                        Estado = v.EstadoDeLaVenta.Nombre
                    })
                    .ToListAsync(),
                
                // Notificaciones del cliente - CORREGIDO para usar el modelo correcto
                Notificaciones = await _context.Notificaciones
                    .Where(n => n.IdCliente == cliente.Id)
                    .OrderByDescending(n => n.Fecha)
                    .Take(3)
                    .Select(n => new NotificacionClienteViewModel
                    {
                        Id = n.Id,
                        Titulo = n.Titulo,
                        Mensaje = n.Mensaje,
                        Fecha = n.Fecha,
                        TipoIcono = ObtenerTipoIconoNotificacion(n.EstadoDeLaVenta)
                    })
                    .ToListAsync(),
                
                // Elementos en carrito
                ElementosCarrito = await _context.Carritos
                    .Where(c => c.ClienteId == cliente.Id)
                    .SelectMany(c => c.Elementos)
                    .Include(e => e.Producto)
                    .Take(3)
                    .Select(e => new ElementoCarritoViewModel
                    {
                        Id = e.Id,
                        NombreProducto = e.Producto.Nombre,
                        Cantidad = e.Cantidad,
                        PrecioUnitario = e.PrecioUnitario,
                        ImagenUrl = e.Producto.ImagenUrl ?? "/images/productos/default.png"
                    })
                    .ToListAsync(),
                
                // Total del carrito
                TotalCarrito = await _context.Carritos
                    .Where(c => c.ClienteId == cliente.Id)
                    .SelectMany(c => c.Elementos)
                    .SumAsync(e => e.PrecioUnitario * e.Cantidad),
                
                // Productos recomendados
                ProductosRecomendados = await _context.Productos
                    .Include(p => p.Categoria)
                    .Where(p => p.Estado)
                    .OrderByDescending(p => p.FechaDeRegistro)
                    .Take(4)
                    .Select(p => new ProductoRecomendadoViewModel
                    {
                        Id = p.Id,
                        Nombre = p.Nombre,
                        Precio = p.PrecioDeVenta,
                        Categoria = p.Categoria.Nombre,
                        ImagenUrl = p.ImagenUrl ?? "/images/productos/default.png",
                        Stock = p.Stock
                    })
                    .ToListAsync()
            };
            
            return View(viewModel);
        }
        
        // Método auxiliar para obtener las últimas actividades
        private async Task<List<ActividadRecienteViewModel>> GetUltimasActividadesAsync()
        {
            var actividades = new List<ActividadRecienteViewModel>();
            
            // Últimas ventas
            var ventas = await _context.Ventas
                .Include(v => v.Cliente)
                .OrderByDescending(v => v.FechaDeRegistro)
                .Take(2)
                .Select(v => new ActividadRecienteViewModel
                {
                    Titulo = "Nueva venta",
                    Descripcion = $"Cliente: {v.Cliente.Nombre} {v.Cliente.Apellido} - Q{v.MontoTotal:F2}",
                    Fecha = v.FechaDeRegistro,
                    TipoIcono = "cash-coin",
                    TipoClase = "success"
                })
                .ToListAsync();
            
            // Últimos productos actualizados
            var productos = await _context.Productos
                .OrderByDescending(p => p.FechaDeRegistro)
                .Take(1)
                .Select(p => new ActividadRecienteViewModel
                {
                    Titulo = "Producto actualizado",
                    Descripcion = $"{p.Nombre} - Stock: {p.Stock}",
                    Fecha = p.FechaDeRegistro,
                    TipoIcono = "box",
                    TipoClase = "primary"
                })
                .ToListAsync();
            
            // Últimos proveedores añadidos
            var proveedores = await _context.Proveedores
                .OrderByDescending(p => p.FechaDeRegistro)
                .Take(1)
                .Select(p => new ActividadRecienteViewModel
                {
                    Titulo = "Nuevo proveedor añadido",
                    Descripcion = p.Nombre,
                    Fecha = p.FechaDeRegistro,
                    TipoIcono = "truck",
                    TipoClase = "warning"
                })
                .ToListAsync();
            
            // Últimos clientes registrados
            var clientes = await _context.Clientes
                .OrderByDescending(c => c.FechaDeRegistro)
                .Take(1)
                .Select(c => new ActividadRecienteViewModel
                {
                    Titulo = "Nuevo cliente registrado",
                    Descripcion = $"{c.Nombre} {c.Apellido}",
                    Fecha = c.FechaDeRegistro,
                    TipoIcono = "people",
                    TipoClase = "info"
                })
                .ToListAsync();
            
            // Combinamos y ordenamos todas las actividades por fecha
            actividades.AddRange(ventas);
            actividades.AddRange(productos);
            actividades.AddRange(proveedores);
            actividades.AddRange(clientes);
            
            return actividades.OrderByDescending(a => a.Fecha).Take(5).ToList();
        }
        
        // Métodos auxiliares para notificaciones
        private static string ObtenerTipoIconoNotificacion(string estado)
        {
            return estado?.ToLower() switch
            {
                "despachada" => "truck",
                "entregada" => "check-circle",
                "devolución solicitada" => "arrow-return-right",
                "devolucion solicitada" => "arrow-return-right",
                "devolución completada" => "arrow-repeat",
                "devolucion completada" => "arrow-repeat",
                _ => "info-circle"
            };
        }

        // También cambia los otros métodos relacionados a static
        private static string ObtenerTituloNotificacion(string estado)
        {
            return estado?.ToLower() switch
            {
                "despachada" => "Pedido en camino",
                "entregada" => "Pedido entregado",
                "devolución solicitada" => "Devolución en proceso",
                "devolución completada" => "Devolución completada",
                _ => "Actualización de pedido"
            };
        }

        private static string ObtenerMensajeNotificacion(string estado)
        {
            return estado?.ToLower() switch
            {
                "despachada" => "está en camino",
                "entregada" => "ha sido entregado",
                "devolución solicitada" => "está en proceso de devolución",
                "devolución completada" => "ha completado el proceso de devolución",
                _ => "ha sido actualizado"
            };
        }
    }
}