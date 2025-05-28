using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SuperBodega.API.Models.Admin
{
    /// <summary>
    /// Clase que representa un detalle de la venta de productos.
    /// </summary>
    public class DetalleDeLaVenta
    {
        /// <summary>
        /// Identificador único del detalle de la venta.
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        /// <summary>
        /// ID de la venta asociada al detalle.
        /// </summary>
        [Required (ErrorMessage = "El ID de la venta es obligatorio.")]
        public int IdVenta { get; set; }
        
        /// <summary>
        /// Venta asociada al detalle.
        /// </summary>
        [ForeignKey("IdVenta")]
        public virtual Venta Venta { get; set; }

        /// <summary>
        /// ID del producto asociado al detalle.
        /// </summary>
        [Required (ErrorMessage = "El ID del producto es obligatorio.")]
        public int IdProducto { get; set; }
        
        /// <summary>
        /// Producto asociado al detalle.
        /// </summary>
        [ForeignKey("IdProducto")]
        public virtual Producto Producto { get; set; }

        /// <summary>
        /// ID del proveedor asociado al detalle.
        /// </summary>
        [Required (ErrorMessage = "El ID del proveedor es obligatorio.")]
        public int IdProveedor { get; set; }
        
        /// <summary>
        /// Proveedor asociado al detalle.
        /// </summary>
        [ForeignKey("IdProveedor")]
        public virtual Proveedor Proveedor { get; set; }

        /// <summary>
        /// Nombre del proveedor asociado al detalle.
        /// </summary>
        [Required(ErrorMessage = "El nombre del proveedor es obligatorio.")]
        [StringLength(100, ErrorMessage = "El nombre del proveedor no puede exceder los 100 caracteres.")]
        [RegularExpression(@"^[a-zA-ZáéíóúÁÉÍÓÚüÜñÑ\s]+$", ErrorMessage = "El nombre solo puede contener letras, tildes y espacios")]
        public string NombreDelProveedor { get; set; }

        /// <summary>
        /// Código del producto asociado al detalle.
        /// </summary>
        [Required (ErrorMessage = "El código del producto es obligatorio.")]
        [StringLength(5, ErrorMessage = "El código del producto no puede exceder los 5 caracteres.")]
        public string CodigoDelProducto { get; set; }

        /// <summary>
        /// Nombre del producto asociado al detalle.
        /// </summary>
        [Required (ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(100, ErrorMessage = "El nombre no puede exceder los 100 caracteres.")]
        [RegularExpression(@"^[a-zA-ZáéíóúÁÉÍÓÚüÜñÑ0-9\s,.-]+$", ErrorMessage = "El nombre solo puede contener letras, números, espacios y algunos caracteres especiales (, . -)")]
        public string NombreDelProducto { get; set; }

        /// <summary>
        /// Imagen del producto asociado al detalle.
        /// </summary>
        public string ImagenDelProducto { get; set; }

        /// <summary>
        /// Precio de venta del producto asociado al detalle.
        /// </summary>
        [Required (ErrorMessage = "El precio de venta es obligatorio.")]
        [Column(TypeName = "decimal(10, 2)")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El precio de venta debe ser mayor que cero")]
        public decimal PrecioDeVenta { get; set; }

        /// <summary>
        /// Cantidad del producto asociado al detalle.
        /// </summary>
        [Required (ErrorMessage = "La cantidad del producto es obligatoria.")]
        [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser mayor que cero")]
        public int Cantidad { get; set; }

        /// <summary>
        /// Monto total del detalle de la venta.
        /// </summary>
        [Required (ErrorMessage = "El monto total es obligatorio.")]
        [Column(TypeName = "decimal(10, 2)")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El monto total debe ser mayor que cero")]
        public decimal MontoTotal { get; set; }

        /// <summary>
        /// Fecha de registro del detalle de la venta.
        /// </summary>
        [Required (ErrorMessage = "La fecha de registro es obligatoria.")]
        [DataType(DataType.DateTime)]
        public DateTime FechaDeRegistro { get; set; } = DateTime.UtcNow;
    }
}