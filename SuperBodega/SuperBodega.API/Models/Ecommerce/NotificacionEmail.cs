namespace SuperBodega.API.Models.Ecommerce
{
    public class NotificacionEmail
    {
        public string IdNotificacion { get; set; } = Guid.NewGuid().ToString();
        public string Para { get; set; }
        public string Asunto { get; set; }
        public string Contenido { get; set; }
        public int IdVenta { get; set; }
        public string NumeroDeFactura { get; set; }
        public string EstadoDeLaVenta { get; set; }
        public DateTime FechaDeRegistro { get; set; }
        public string NombreCompletoDelCliente { get; set; }
        public string EmailDelCliente { get; set; }
        public decimal MontoTotal { get; set; }
        public List<DetalleNotificacionEmail> Productos { get; set; } = new List<DetalleNotificacionEmail>();
    }
}