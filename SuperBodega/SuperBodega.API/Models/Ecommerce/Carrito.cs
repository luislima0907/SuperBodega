using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SuperBodega.API.Models.Admin;

namespace SuperBodega.API.Models.Ecommerce
{
    /// <summary>
    /// Clase que representa un carrito de compras.
    /// </summary>
    public class Carrito
    {
        /// <summary>
        /// Identificador único del carrito.
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        
        /// <summary>
        /// ID del cliente asociado al carrito.
        /// </summary>
        [Required (ErrorMessage = "El ID del cliente es obligatorio.")]
        public int ClienteId { get; set; }
        
        /// <summary>
        /// Fecha de creacion del carrito.
        /// </summary>
        [Required (ErrorMessage = "La fehca de creacion es obligatoria.")]
        [DataType(DataType.DateTime)]
        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
        
        /// <summary>
        /// Cliente asociado al carrito.
        /// </summary>
        // Relación con Cliente
        [ForeignKey("ClienteId")]
        public virtual Cliente Cliente { get; set; }
        
        /// <summary>
        /// Lista de elementos en el carrito.
        /// </summary>
        // Relación con elementos del carrito
        [Required (ErrorMessage = "Los elementos del carrito son obligatorios.")]
        public virtual ICollection<ElementoCarrito> Elementos { get; set; } = new List<ElementoCarrito>();
        
        /// <summary>
        /// Propiedad calculada que representa el total del carrito.
        /// </summary>
        // Propiedad calculada que no se guarda en la base de datos
        [NotMapped]
        public decimal? Total => Elementos?.Sum(e => e.Subtotal) ?? 0;
    }
}