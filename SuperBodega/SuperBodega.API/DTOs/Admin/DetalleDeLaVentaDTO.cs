namespace SuperBodega.API.DTOs.Admin
{
    /// <summary>
    /// DTO para visualizar datos completos de un detalle de la venta
    /// </summary>
    public class DetalleDeLaVentaDTO
    {
        /// <summary>
        /// Identificador único del detalle de la venta
        /// </summary>
        public int Id { get; set; }
        
        /// <summary>
        /// Identificador de la venta asociada al detalle
        /// </summary>
        public int IdVenta { get; set; }
        
        /// <summary>
        /// Identificador del producto asociado al detalle
        /// </summary>
        public int IdProducto { get; set; }
        
        /// <summary>
        /// Código del producto asociado al detalle
        /// </summary>
        public string CodigoDelProducto { get; set; }
        
        /// <summary>
        /// Nombre del producto asociado al detalle
        /// </summary>
        public string NombreDelProducto { get; set; }
        
        /// <summary>
        /// Imagen del producto asociado al detalle
        /// </summary>
        public string ImagenDelProducto { get; set; }
        
        /// <summary>
        /// Categoría del producto asociado al detalle
        /// </summary>
        public string NombreCategoria { get; set; }
        
        /// <summary>
        /// Precio de venta del producto asociado al detalle
        /// </summary>
        public decimal PrecioDeVenta { get; set; }
        
        /// <summary>
        /// Cantidad del producto asociado al detalle
        /// </summary>
        public int Cantidad { get; set; }
        
        /// <summary>
        /// Monto total del detalle de la venta
        /// </summary>
        public decimal MontoTotal { get; set; }
        
        /// <summary>
        /// Fecha de registro del detalle de la venta
        /// </summary>
        public DateTime FechaDeRegistro { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// DTO para la creación de un nuevo detalle de la venta
    /// </summary>
    public class CreateDetalleDeLaVentaDTO
    {
        /// <summary>
        /// Identificador del producto asociado al detalle
        /// </summary>
        public int IdProducto { get; set; }

        /// <summary>
        /// Cantidad del producto asociado al detalle
        /// </summary>
        public int Cantidad { get; set; }

        /// <summary>
        /// Identificador del proveedor asociado al detalle
        /// </summary>
        public int IdProveedor { get; set; }

        /// <summary>
        /// Nombre del proveedor asociado al detalle
        /// </summary>
        public string NombreDelProveedor { get; set; }

        /// <summary>
        /// Precio de venta del producto asociado al detalle
        /// </summary>
        public decimal PrecioDeVenta { get; set; }

        /// <summary>
        /// Fecha de registro del detalle de la venta
        /// </summary>
        public DateTime FechaDeRegistro { get; set; } = DateTime.UtcNow;
    }
}