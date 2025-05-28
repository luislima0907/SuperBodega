namespace SuperBodega.API.ViewModels
{
    public class AdminDashboardViewModel
    {
        public int TotalProductos { get; set; }
        public int TotalClientes { get; set; }
        public int TotalProveedores { get; set; }
        public decimal VentasDelMes { get; set; }
        public List<VentaRecienteViewModel> VentasRecientes { get; set; } = new();
        public List<ProductoStockBajoViewModel> ProductosStockBajo { get; set; } = new();
        public List<ProductoMasVendidoViewModel> ProductosMasVendidos { get; set; } = new();
        public List<ActividadRecienteViewModel> UltimasActividades { get; set; } = new();
    }

    public class ClienteDashboardViewModel
    {
        public bool NoHayCliente { get; set; }
        public int ClienteId { get; set; }
        public string ClienteNombre { get; set; }
        public int TotalPedidos { get; set; }
        public int PedidosEntregados { get; set; }
        public int PedidosEnTransito { get; set; }
        public int ArticulosEnCarrito { get; set; }
        public List<PedidoRecienteViewModel> PedidosRecientes { get; set; } = new();
        public List<NotificacionClienteViewModel> Notificaciones { get; set; } = new();
        public List<ElementoCarritoViewModel> ElementosCarrito { get; set; } = new();
        public decimal? TotalCarrito { get; set; }
        public List<ProductoRecomendadoViewModel> ProductosRecomendados { get; set; } = new();
    }

    public class VentaRecienteViewModel
    {
        public int Id { get; set; }
        public string NumeroFactura { get; set; }
        public string NombreCliente { get; set; }
        public DateTime FechaRegistro { get; set; }
        public decimal? MontoTotal { get; set; }
        public string Estado { get; set; }
    }

    public class PedidoRecienteViewModel
    {
        public int Id { get; set; }
        public string NumeroFactura { get; set; }
        public DateTime FechaRegistro { get; set; }
        public decimal? MontoTotal { get; set; }
        public string Estado { get; set; }
    }

    public class ProductoStockBajoViewModel
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Categoria { get; set; }
        public int StockActual { get; set; }
        public int StockMinimo { get; set; }
        public string GetEstadoStock() => StockActual <= 3 ? "Crítico" : "Bajo";
        public string GetBadgeClass() => StockActual <= 3 ? "bg-danger" : "bg-warning";
    }

    public class ProductoMasVendidoViewModel
    {
        public string Nombre { get; set; }
        public int CantidadVendida { get; set; }
        public int Porcentaje { get; set; }
        public string GetBarColor()
        {
            if (Porcentaje >= 75) return "bg-primary";
            if (Porcentaje >= 50) return "bg-success";
            if (Porcentaje >= 25) return "bg-warning";
            return "bg-danger";
        }
    }

    public class ActividadRecienteViewModel
    {
        public string Titulo { get; set; }
        public string Descripcion { get; set; }
        public DateTime Fecha { get; set; }
        public string TipoIcono { get; set; }
        public string TipoClase { get; set; }
    }

    public class NotificacionClienteViewModel
    {
        public int Id { get; set; }
        public string Titulo { get; set; }
        public string Mensaje { get; set; }
        public DateTime Fecha { get; set; }
        public string TipoIcono { get; set; }
        public TimeSpan TiempoTranscurrido => DateTime.Now - Fecha;
        public string TiempoFormateado
        {
            get
            {
                if (TiempoTranscurrido.TotalMinutes < 60)
                    return $"Hace {(int)TiempoTranscurrido.TotalMinutes} minutos";
                if (TiempoTranscurrido.TotalHours < 24)
                    return $"Hace {(int)TiempoTranscurrido.TotalHours} horas";
                if (TiempoTranscurrido.TotalDays < 7)
                    return $"Hace {(int)TiempoTranscurrido.TotalDays} días";
                return Fecha.ToShortDateString();
            }
        }
        public string GetIconClass()
        {
            return TipoIcono switch
            {
                "truck" => "bg-primary",
                "check-circle" => "bg-success",
                "tag" => "bg-success",
                "arrow-return-right" => "bg-warning",
                "arrow-repeat" => "bg-info",
                _ => "bg-info"
            };
        }
    }

    public class ElementoCarritoViewModel
    {
        public int Id { get; set; }
        public string NombreProducto { get; set; }
        public int Cantidad { get; set; }
        public decimal? PrecioUnitario { get; set; }
        public string ImagenUrl { get; set; }
        public decimal? Subtotal => Cantidad * PrecioUnitario;
    }

    public class ProductoRecomendadoViewModel
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public decimal? Precio { get; set; }
        public string Categoria { get; set; }
        public string ImagenUrl { get; set; }
        public int? Stock { get; set; }
    }
}