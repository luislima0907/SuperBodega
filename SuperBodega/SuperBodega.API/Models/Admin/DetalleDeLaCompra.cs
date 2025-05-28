using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SuperBodega.API.Models.Admin;

/// <summary>
/// Clase que representa un detalle de la compra de productos.
/// </summary>
public class DetalleDeLaCompra
{
    /// <summary>
    /// Identificador único del detalle de la compra.
    /// </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    
    /// <summary>
    /// ID de la compra asociada al detalle.
    /// </summary>
    [Required (ErrorMessage = "El ID de la compra es obligatorio.")]
    public int IdCompra { get; set; }
    
    /// <summary>
    /// Compra asociada al detalle.
    /// </summary>
    [ForeignKey("IdCompra")]
    public Compra Compra { get; set; }
    
    /// <summary>
    /// ID del producto asociado al detalle.
    /// </summary>
    [Required (ErrorMessage = "El ID del producto es obligatorio.")]
    public int IdProducto { get; set; }
    
    /// <summary>
    /// Producto asociado al detalle.
    /// </summary>
    [ForeignKey("IdProducto")]
    public Producto Producto { get; set; }
    
    /// <summary>
    /// Precio de compra del producto.
    /// </summary>
    [Required (ErrorMessage = "El precio de compra es obligatorio.")]
    [Column(TypeName = "decimal(10,2)")]
    [Range(0.01, double.MaxValue, ErrorMessage = "El precio de compra debe ser mayor que cero")]
    public decimal PrecioDeCompra { get; set; }
    
    /// <summary>
    /// Precio de venta del producto.
    /// </summary>
    [Required (ErrorMessage = "El precio de venta es obligatorio.")]
    [Column(TypeName = "decimal(10,2)")]
    [Range(0.01, double.MaxValue, ErrorMessage = "El precio de venta debe ser mayor que cero")]
    public decimal PrecioDeVenta { get; set; }
    
    /// <summary>
    /// Cantidad del producto.
    /// </summary>
    [Required (ErrorMessage = "La cantidad del producto es obligatoria.")]
    [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser mayor que cero")]
    public int Cantidad { get; set; }
    
    /// <summary>
    /// Monto total del detalle de la compra.
    /// </summary>
    [Required (ErrorMessage = "El monto total es obligatorio.")]
    [Column(TypeName = "decimal(10,2)")]
    [Range(0.01, double.MaxValue, ErrorMessage = "El monto total debe ser mayor que cero")]
    public decimal Montototal { get; set; }
    
    /// <summary>
    /// Fecha de registro del detalle de la compra.
    /// </summary>
    [Required (ErrorMessage = "La fecha de registro es obligatoria.")]
    [DataType(DataType.DateTime)]
    public DateTime FechaDeRegistro { get; set; } = DateTime.UtcNow;
}