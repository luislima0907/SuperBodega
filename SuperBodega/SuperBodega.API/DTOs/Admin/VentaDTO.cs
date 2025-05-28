namespace SuperBodega.API.DTOs.Admin
{
    /// <summary>
    /// DTO para visualizar datos completos de una venta
    /// </summary>
    public class VentaDTO
    {
        /// <summary>
        /// Identificador único de la venta
        /// </summary>
        public int Id { get; set; }
        
        /// <summary>
        /// Número de factura de la venta
        /// </summary>
        public string NumeroDeFactura { get; set; }
        
        /// <summary>
        /// Identificador del cliente asociado a la venta
        /// </summary>
        public int IdCliente { get; set; }
        
        /// <summary>
        /// Nombre del cliente asociado a la venta
        /// </summary>
        public string NombreCompletoCliente { get; set; }
        
        /// <summary>
        /// Correo electrónico del cliente asociado a la venta
        /// </summary>
        public string EmailCliente { get; set; }
        
        /// <summary>
        /// Identificador del estado asociado a la venta
        /// </summary>
        public int IdEstadoDeLaVenta { get; set; }
        
        /// <summary>
        /// Nombre del estado asociado a la venta
        /// </summary>
        public string NombreEstadoDeLaVenta { get; set; }
        
        /// <summary>
        /// Monto de pago de la venta
        /// </summary>
        public decimal MontoDePago { get; set; }
        
        /// <summary>
        /// Monto de cambio de la venta
        /// </summary>
        public decimal MontoDeCambio { get; set; }
        
        /// <summary>
        /// Monto total de la venta
        /// </summary>
        public decimal MontoTotal { get; set; }
        
        /// <summary>
        /// Fecha y hora en que se registró la venta
        /// </summary>
        public DateTime FechaDeRegistro { get; set; } = DateTime.UtcNow;
        
        /// <summary>
        /// Detalles de la venta
        /// </summary>
        public List<DetalleDeLaVentaDTO> DetallesDeLaVenta { get; set; }
    }

    /// <summary>
    /// DTO para crear una nueva venta
    /// </summary>
    public class CreateVentaDTO
    {
        /// <summary>
        /// Identificador del cliente asociado a la venta
        /// </summary>
        public int IdCliente { get; set; }

        /// <summary>
        /// Monto de pago de la venta
        /// </summary>
        public decimal MontoDePago { get; set; }

        /// <summary>
        /// Fecha y hora en que se registró la venta
        /// </summary>
        public DateTime FechaDeRegistro { get; set; } = DateTime.UtcNow; 

        /// <summary>
        /// Detalles de la venta
        /// </summary>
        public List<CreateDetalleDeLaVentaDTO> Detalles { get; set; }
        
        /// <summary>
        /// Modo de la notificacion para la venta
        /// </summary>
        public bool UsarNotificacionSincronica { get; set; } = false;
    }
}