using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SuperBodega.API.Models.Admin;

public class DetalleDeLaCompra
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    public int IdCompra { get; set; }
    
    [ForeignKey("IdCompra")]
    public Compra Compra { get; set; }
    
    [Required]
    public int IdProducto { get; set; }
    
    [ForeignKey("IdProducto")]
    public Producto Producto { get; set; }
    
    [Column(TypeName = "decimal(10,2)")]
    public decimal PrecioDeCompra { get; set; }
    
    [Column(TypeName = "decimal(10,2)")]
    public decimal PrecioDeVenta { get; set; }
    
    [Required]
    public int Cantidad { get; set; }
    
    [Column(TypeName = "decimal(10,2)")]
    public decimal Montototal { get; set; }
    
    public DateTime FechaDeRegistro { get; set; } = DateTime.UtcNow;
}