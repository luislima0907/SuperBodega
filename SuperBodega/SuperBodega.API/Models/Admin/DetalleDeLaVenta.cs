using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SuperBodega.API.Models.Admin
{
    public class DetalleDeLaVenta
    {
        [Key]
        public int Id { get; set; }

        // Foreign Key for Venta
        public int IdVenta { get; set; }
        [ForeignKey("IdVenta")]
        public virtual Venta Venta { get; set; }

        // Foreign Key for Producto
        public int IdProducto { get; set; }
        [ForeignKey("IdProducto")]
        public virtual Producto Producto { get; set; }

        // Foreign Key for Proveedor (As per README, though unusual for a sales detail)
        public int IdProveedor { get; set; }
        [ForeignKey("IdProveedor")]
        public virtual Proveedor Proveedor { get; set; }

        // Denormalized fields (as per README)
        [MaxLength(100)]
        public string NombreDelProveedor { get; set; }

        [MaxLength(50)]
        public string CodigoDelProducto { get; set; }

        [MaxLength(100)] // Increased length based on Venta table example
        public string NombreDelProducto { get; set; }

        public string ImagenDelProducto { get; set; }

        [Column(TypeName = "decimal(10, 2)")]
        public decimal PrecioDeVenta { get; set; }

        public int Cantidad { get; set; }

        [Column(TypeName = "decimal(10, 2)")]
        public decimal MontoTotal { get; set; }

        public DateTime FechaDeRegistro { get; set; } = DateTime.UtcNow;
    }
}