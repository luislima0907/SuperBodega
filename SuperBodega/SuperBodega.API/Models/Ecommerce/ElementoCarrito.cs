using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SuperBodega.API.Models.Admin;

namespace SuperBodega.API.Models.Ecommerce
{
    public class ElementoCarrito
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int CarritoId { get; set; }

        [Required]
        public int ProductoId { get; set; }

        [Required]
        public int Cantidad { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal? PrecioUnitario { get; set; }

        // Relación con Carrito
        [ForeignKey("CarritoId")]
        public virtual Carrito Carrito { get; set; }

        // Relación con Producto
        [ForeignKey("ProductoId")]
        public virtual Producto Producto { get; set; }

        // Propiedad calculada que no se guarda en la base de datos
        [NotMapped]
        public decimal? Subtotal => Cantidad * PrecioUnitario;
    }
}