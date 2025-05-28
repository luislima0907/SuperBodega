using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SuperBodega.API.Models.Admin;

namespace SuperBodega.API.Models.Ecommerce
{
    /// <summary>
    /// Clase que representa un elemento en el carrito de compras.
    /// </summary>
    public class ElementoCarrito
    {
        /// <summary>
        /// Identificador único del elemento del carrito.
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        
        /// <summary>
        /// ID del carrito al que pertenece el elemento.
        /// </summary>
        [Required (ErrorMessage = "El ID del carrito es obligatorio.")]
        public int CarritoId { get; set; }
        
        /// <summary>
        /// ID del producto asociado al elemento del carrito.
        /// </summary>
        [Required (ErrorMessage = "El ID del producto es obligatorio.")]
        public int ProductoId { get; set; }
        
        /// <summary>
        /// Cantidad del producto en el carrito.
        /// </summary>
        [Required (ErrorMessage = "La cantidad es obligatoria.")]
        public int Cantidad { get; set; }
        
        /// <summary>
        /// Precio unitario del producto en el carrito.
        /// </summary>
        [Required (ErrorMessage = "El precio unitario es obligatorio.")]
        [Column(TypeName = "decimal(10,2)")]
        public decimal? PrecioUnitario { get; set; }
        
        /// <summary>
        /// Carrito asociado al producto.
        /// </summary>
        // Relación con Carrito
        [ForeignKey("CarritoId")]
        public virtual Carrito Carrito { get; set; }
        
        /// <summary>
        /// Producto asociado al elemento del carrito.
        /// </summary>
        // Relación con Producto
        [ForeignKey("ProductoId")]
        public virtual Producto Producto { get; set; }
        
        /// <summary>
        /// Subtotal del elemento del carrito.
        /// </summary>
        // Propiedad calculada que no se guarda en la base de datos
        [NotMapped]
        public decimal? Subtotal => Cantidad * PrecioUnitario;
    }
}