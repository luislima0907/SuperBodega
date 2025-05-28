using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SuperBodega.API.Models.Admin;

/// <summary>
/// Clase que representa una compra de productos para aumentar stock.
/// </summary>
public class Compra
{
    /// <summary>
    /// Identificador único de la compra.
    /// </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    
    /// <summary>
    /// Número de factura de la compra.
    /// </summary>
    [Required (ErrorMessage = "El número de factura es obligatorio.")]
    [StringLength(5, ErrorMessage = "El número de factura no puede exceder los 5 caracteres.")]
    public string NumeroDeFactura { get; set; }
    
    /// <summary>
    /// ID del proveedor asociado a la compra.
    /// </summary>
    [Required (ErrorMessage = "El ID del proveedor es obligatorio.")]
    public int IdProveedor { get; set; }
    
    /// <summary>
    /// Proveedor asociado a la compra.
    /// </summary>
    [ForeignKey("IdProveedor")]
    public virtual Proveedor Proveedor { get; set; }
    
    /// <summary>
    /// Fecha de registro de la compra.
    /// </summary>
    [Required (ErrorMessage = "La fecha de registro es obligatoria.")]
    [DataType(DataType.DateTime)]
    public DateTime FechaDeRegistro { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Monto total de la compra.
    /// </summary>
    [Required (ErrorMessage = "El monto total es obligatorio.")]
    [Column(TypeName = "decimal(10,2)")]
    [Range(0.01, double.MaxValue, ErrorMessage = "El monto total debe ser mayor que cero")]
    public decimal MontoTotal { get; set; }

    /// <summary>
    /// Detalles de la compra.
    /// </summary>
    [Required (ErrorMessage = "Los detalles de la compra son obligatorios.")]
    [MinLength(1, ErrorMessage = "Se requiere al menos un detalle de compra")]
    public virtual ICollection<DetalleDeLaCompra> DetallesDeLaCompra { get; set; } = new List<DetalleDeLaCompra>();
}