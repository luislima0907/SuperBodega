using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SuperBodega.API.Models.Admin
{
    /// <summary>
    /// Clase que representa un estado de la venta de productos.
    /// </summary>
    public class EstadoDeLaVenta
    {
        /// <summary>
        /// Identificador único del estado de la venta.
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        /// <summary>
        /// Nombre del estado de la venta.
        /// </summary>
        [Required (ErrorMessage = "El nombre del estado es obligatorio.")]
        [MaxLength(50, ErrorMessage = "El nombre del estado no puede exceder los 50 caracteres.")]
        [RegularExpression(@"^[a-zA-ZáéíóúÁÉÍÓÚüÜñÑ\s]+$", ErrorMessage = "El nombre solo puede contener letras, tildes y espacios")]
        public string Nombre { get; set; } // "Recibida", "Despachada", "Entregada", "Solicitud de Devolucion", "Devolucion Completada"

        /// <summary>
        /// Venta asociada al estado.
        /// </summary>
        [Required (ErrorMessage = "La venta es obligatoria.")]
        [MinLength(1, ErrorMessage = "Se requiere al menos una venta")]
        public virtual ICollection<Venta> Ventas { get; set; }
        
        /// <summary>
        /// Detalles de la venta asociados al estado.
        /// </summary>
        [Required(ErrorMessage = "Los detalles de la venta son obligatorios.")]
        [MinLength(1, ErrorMessage = "Se requiere al menos un detalle")]
        public virtual ICollection<DetalleDeLaVenta> DetallesDeLaVenta { get; set; }
    }
}