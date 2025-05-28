namespace SuperBodega.API.DTOs.Admin
{
    /// <summary>
    /// DTO para visualizar datos completos de un estado de la venta
    /// </summary>
    public class EstadoDeLaVentaDTO
    {
        /// <summary>
        /// Identificador único del estado de la venta
        /// </summary>
        public int Id { get; set; }
        
        /// <summary>
        /// Nombre del estado de la venta
        /// </summary>
        public string Nombre { get; set; }
    }

    /// <summary>
    /// DTO para actualizar el estado de la venta
    /// </summary>
    public class UpdateEstadoDeLaVentaDTO
    {
        /// <summary>
        /// Identificador único del estado de la venta
        /// </summary>
        public int IdEstadoDeLaVenta { get; set; }
        
        /// <summary>
        /// Modo de la notificacion para la venta
        /// </summary>
        public bool UsarNotificacionSincronica { get; set; } = false;
    }
}