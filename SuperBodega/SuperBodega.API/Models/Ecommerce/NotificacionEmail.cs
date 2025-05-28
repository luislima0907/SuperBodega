namespace SuperBodega.API.Models.Ecommerce
{
    /// <summary>
    /// Modelo para representar una notificación por correo electrónico
    /// </summary>
    public class NotificacionEmail
    {
        /// <summary>
        /// Identificador único de la notificación
        /// </summary>
        public string IdNotificacion { get; set; } = Guid.NewGuid().ToString();
        
        /// <summary>
        /// Dirección de correo electrónico del destinatario
        /// </summary>
        public string Para { get; set; }
        
        /// <summary>
        /// Asunto de la notificacion al correo electrónico
        /// </summary>
        public string Asunto { get; set; }
        
        /// <summary>
        /// Contenido de la notificacion por correo electrónico
        /// </summary>
        public string Contenido { get; set; }
        
        /// <summary>
        /// Identificador de la venta asociada a la notificación
        /// </summary>
        public int IdVenta { get; set; }
        
        /// <summary>
        /// Número de factura de la venta
        /// </summary>
        public string NumeroDeFactura { get; set; }
        
        /// <summary>
        /// Estado de la venta
        /// </summary>
        public string EstadoDeLaVenta { get; set; }
        
        /// <summary>
        /// Fecha y hora en que se registró la notificación
        /// </summary>
        public DateTime FechaDeRegistro { get; set; }
        
        /// <summary>
        /// Nombre completo del cliente asociado a la venta
        /// </summary>
        public string NombreCompletoDelCliente { get; set; }
        
        /// <summary>
        /// Correo electrónico del cliente asociado a la venta
        /// </summary>
        public string EmailDelCliente { get; set; }
        
        /// <summary>
        /// Monto total de la venta
        /// </summary>
        public decimal MontoTotal { get; set; }
        
        /// <summary>
        /// Detalles de los productos asociados a la venta en la notificacion.
        /// </summary>
        public List<DetalleNotificacionEmail> Productos { get; set; } = new List<DetalleNotificacionEmail>();
    }
}