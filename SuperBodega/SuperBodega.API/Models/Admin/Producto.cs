using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SuperBodega.API.Models.Admin;

/// <summary>
/// Clase que representa un producto.
/// </summary>
public class Producto
{
    /// <summary>
    /// Identificador único del producto.
    /// </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    
    /// <summary>
    /// Código único del producto.
    /// </summary>
    [Required (ErrorMessage = "El código es obligatorio.")]
    [StringLength(5, ErrorMessage = "El código no puede exceder los 5 caracteres.")]
    public string Codigo { get; set; }
    
    /// <summary>
    /// Nombre del producto.
    /// </summary>
    [Required (ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(100, ErrorMessage = "El nombre no puede exceder los 100 caracteres.")]
    [RegularExpression(@"^[a-zA-ZáéíóúÁÉÍÓÚüÜñÑ0-9\s,.-]+$", ErrorMessage = "El nombre solo puede contener letras, números, espacios y algunos caracteres especiales (, . -)")]
    public string Nombre { get; set; }
    
    /// <summary>
    /// Descripción del producto.
    /// </summary>
    [Required (ErrorMessage = "La descripción es obligatoria.")]
    [StringLength(200, ErrorMessage = "La descripción no puede exceder los 200 caracteres.")]
    public string Descripcion { get; set; }
    
    /// <summary>
    /// ID de la categoría a la que pertenece el producto.
    /// </summary>
    [Required (ErrorMessage = "El ID de la categoría es obligatorio.")]
    public int CategoriaId { get; set; }
    
    /// <summary>
    /// Categoría a la que pertenece el producto.
    /// </summary>
    [ForeignKey("CategoriaId")]
    public Categoria Categoria { get; set; }

    /// <summary>
    /// Stock del producto.
    /// </summary>
    [Required (ErrorMessage = "El stock del producto es obligatorio.")]
    public int Stock { get; set; } = 0;

    /// <summary>
    /// Precio de compra del producto.
    /// </summary>
    [Required (ErrorMessage = "El precio de compra es obligatorio.")]
    [Column(TypeName = "decimal(10,2)")] 
    [Range(0, 999999.99, ErrorMessage = "El precio de compra debe estar entre 0 y 999999.99")]
    public decimal? PrecioDeCompra { get; set; } = 0;

    /// <summary>
    /// Precio de venta del producto.
    /// </summary>
    [Required (ErrorMessage = "El precio de venta es obligatorio.")]
    [Column(TypeName = "decimal(10,2)")] 
    [Range(0, 999999.99, ErrorMessage = "El precio de venta debe estar entre 0 y 999999.99")]
    public decimal? PrecioDeVenta { get; set; } = 0;
    
    /// <summary>
    /// Estado del producto (activo/inactivo).
    /// </summary>
    [Required (ErrorMessage = "El estado es obligatorio.")]
    [RegularExpression(@"^(true|false)$", ErrorMessage = "El estado debe ser verdadero o falso")]
    public bool Estado { get; set; }
    
    /// <summary>
    /// Imagen del producto.
    /// </summary>
    public string? ImagenUrl { get; set; }
    
    /// <summary>
    /// Fecha de registro del producto.
    /// </summary>
    [Required (ErrorMessage = "La fecha de registro es obligatoria.")]
    [DataType(DataType.DateTime)]
    public DateTime FechaDeRegistro { get; set; } = DateTime.UtcNow;
}