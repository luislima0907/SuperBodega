namespace SuperBodega.API.DTOs.Admin;

/// <summary>
/// DTO para visualizar datos completos de un producto
/// </summary>
public class ProductoDTO
{
    /// <summary>
    /// Identificador único del producto
    /// </summary>
    public int Id { get; set; }
    
    /// <summary>
    /// Código del producto
    /// </summary>
    public string Codigo { get; set; }
    
    /// <summary>
    /// Nombre del producto
    /// </summary>
    public string Nombre { get; set; }
    
    /// <summary>
    /// Descripción del producto
    /// </summary>
    public string Descripcion { get; set; }
    
    /// <summary>
    /// Identificador de la categoría asociada al producto
    /// </summary>
    public int CategoriaId { get; set; }
    
    /// <summary>
    /// Nombre de la categoría asociada al producto
    /// </summary>
    public string CategoriaNombre { get; set; }
    
    /// <summary>
    /// Indica si la categoría asociada al producto está activa o inactiva
    /// </summary>
    public bool? CategoriaActiva { get; set; } 
    
    /// <summary>
    /// Cantidad de producto disponible en stock
    /// </summary>
    public int Stock { get; set; }
    
    /// <summary>
    /// Precio de compra del producto
    /// </summary>
    public decimal? PrecioDeCompra { get; set; }
    
    /// <summary>
    /// Precio de venta del producto
    /// </summary>
    public decimal? PrecioDeVenta { get; set; }
    
    /// <summary>
    /// Indica si el producto está activo o inactivo
    /// </summary>
    public bool Estado { get; set; }
    
    /// <summary>
    /// URL de la imagen del producto
    /// </summary>
    public string? ImagenUrl { get; set; }
    
    /// <summary>
    /// Fecha en que se registró el producto en el sistema
    /// </summary>
    public DateTime FechaDeRegistro { get; set; }
}

/// <summary>
/// DTO para la creación de un nuevo producto
/// </summary>
public class CreateProductoDTO
{
    /// <summary>
    /// Código del producto
    /// </summary>
    public string Codigo { get; set; }
    
    /// <summary>
    /// Nombre del producto
    /// </summary>
    public string Nombre { get; set; }
    
    /// <summary>
    /// Descripción del producto
    /// </summary>
    public string Descripcion { get; set; }
    
    /// <summary>
    /// Identificador de la categoría asociada al producto
    /// </summary>
    public int CategoriaId { get; set; }
    
    /// <summary>
    /// Cantidad de producto disponible en stock
    /// </summary>
    public int Stock { get; set; }

    /// <summary>
    /// Precio de compra del producto
    /// </summary>
    public decimal? PrecioDeCompra { get; set; } = 0;

    /// <summary>
    /// Precio de venta del producto
    /// </summary>
    public decimal? PrecioDeVenta { get; set; } = 0;
    
    /// <summary>
    /// Indica si el producto está activo o inactivo
    /// </summary>
    public bool Estado { get; set; }
    
    /// <summary>
    /// URL de la imagen del producto
    /// </summary>
    public string? ImagenUrl { get; set; }
    
    /// <summary>
    /// Fecha en que se registró el producto en el sistema
    /// </summary>
    public DateTime FechaDeRegistro { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// DTO para la actualización de un producto
/// </summary>
public class UpdateProductoDTO
{
    /// <summary>
    /// Código del producto
    /// </summary>
    public string Codigo { get; set; }
    
    /// <summary>
    /// Nombre del producto
    /// </summary>
    public string Nombre { get; set; }
    
    /// <summary>
    /// Descripción del producto
    /// </summary>
    public string Descripcion { get; set; }
    
    /// <summary>
    /// Identificador de la categoría asociada al producto
    /// </summary>
    public int CategoriaId { get; set; }
       
    /// <summary>
    /// Precio de compra del producto
    /// </summary>
    public decimal? PrecioDeCompra { get; set; }
        
    /// <summary>
    /// Precio de venta del producto
    /// </summary>
    public decimal? PrecioDeVenta { get; set; }
        
    /// <summary>
    /// Indica si el producto está activo o inactivo
    /// </summary>
    public bool Estado { get; set; }
      
    /// <summary>
    /// URL de la imagen del producto
    /// </summary>
    public string? ImagenUrl { get; set; }
}