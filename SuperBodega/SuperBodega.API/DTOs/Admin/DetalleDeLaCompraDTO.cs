namespace SuperBodega.API.DTOs.Admin;

/// <summary>
/// DTO para visualizar datos completos de una compra
/// </summary>
public class DetalleDeLaCompraDTO
{
    /// <summary>
    /// Identificador único del detalle de la compra
    /// </summary>
    public int Id { get; set; }
    
    /// <summary>
    /// Identificador de la compra asociada al detalle
    /// </summary>
    public int IdCompra { get; set; }
    
    /// <summary>
    /// Identificador del producto asociado al detalle
    /// </summary>
    public int IdProducto { get; set; }
    
    /// <summary>
    /// Nombre del producto asociado al detalle
    /// </summary>
    public string NombreDelProducto { get; set; }
    
    /// <summary>
    /// Código del producto asociado al detalle
    /// </summary>
    public string CodigoDelProducto { get; set; }
    
    /// <summary>
    /// Imagen del producto asociado al detalle
    /// </summary>
    public string ImagenDelProducto { get; set; }
    
    /// <summary>
    /// Categoría del producto asociado al detalle
    /// </summary>
    public string CategoriaDelProducto { get; set; }
    
    /// <summary>
    /// Precio de compra del producto asociado al detalle
    /// </summary>
    public decimal PrecioDeCompra { get; set; }
    
    /// <summary>
    /// Precio de venta del producto asociado al detalle
    /// </summary>
    public decimal PrecioDeVenta { get; set; }
    
    /// <summary>
    /// Cantidad del producto asociado al detalle
    /// </summary>
    public int Cantidad { get; set; }
    
    /// <summary>
    /// Monto total del detalle de la compra
    /// </summary>
    public decimal Montototal { get; set; }
    
    /// <summary>
    /// Fecha de registro del detalle de la compra
    /// </summary>
    public DateTime FechaDeRegistro { get; set; }
}

/// <summary>
/// DTO para crear un nuevo detalle de la compra
/// </summary>
public class CreateDetalleDeLaCompraDTO
{
    /// <summary>
    /// Identificador del producto asociado al detalle
    /// </summary>
    public int IdProducto { get; set; }
    
    /// <summary>
    /// Precio de compra del producto asociado al detalle
    /// </summary>
    public decimal PrecioDeCompra { get; set; }
    
    /// <summary>
    /// Precio de venta del producto asociado al detalle
    /// </summary>
    public decimal PrecioDeVenta { get; set; }
    
    /// <summary>
    /// Cantidad del producto asociado al detalle
    /// </summary>
    public int Cantidad { get; set; }

    /// <summary>
    /// Fecha de registro del detalle de la compra
    /// </summary>
    public DateTime FechaDeRegistro { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// DTO para actualizar un detalle de la compra
/// </summary>
public class UpdateDetalleDeLaCompraDTO
{
    /// <summary>
    /// Identificador único del detalle de la compra
    /// </summary>
    public int Id { get; set; } // ID existente si lo hay, 0 para nuevos
    
    /// <summary>
    /// Identificador del producto asociado al detalle
    /// </summary>
    public int IdProducto { get; set; }
    
    /// <summary>
    /// Precio de compra del producto asociado al detalle
    /// </summary>
    public decimal PrecioDeCompra { get; set; }
    
    /// <summary>
    /// Precio de venta del producto asociado al detalle
    /// </summary>
    public decimal PrecioDeVenta { get; set; }
    
    /// <summary>
    /// Cantidad del producto asociado al detalle
    /// </summary>
    public int Cantidad { get; set; }
}