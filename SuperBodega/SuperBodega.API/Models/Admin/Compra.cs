using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SuperBodega.API.Models.Admin;

public class Compra
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    [StringLength(50)]
    public string NumeroDeFactura { get; set; }
    
    [Required]
    public int IdProveedor { get; set; }
    
    [ForeignKey("IdProveedor")]
    public virtual Proveedor Proveedor { get; set; }
    
    public DateTime FechaDeRegistro { get; set; } = DateTime.UtcNow;
    
    [Column(TypeName = "decimal(10,2)")]
    public decimal MontoTotal { get; set; }

    public virtual ICollection<DetalleDeLaCompra> DetallesDeLaCompra { get; set; } = new List<DetalleDeLaCompra>();
}