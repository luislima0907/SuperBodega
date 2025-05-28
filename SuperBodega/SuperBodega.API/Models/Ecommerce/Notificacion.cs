using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SuperBodega.API.Models.Ecommerce
{
    /// <summary>
    /// Clase que representa una notificación de venta.
    /// </summary>
    public class Notificacion
    {
        /// <summary>
        /// Identificador único de la notificación.
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        
        /// <summary>
        /// ID del cliente asociado a la notificación.
        /// </summary>
        [Required(ErrorMessage = "El ID del cliente es obligatorio.")]
        public int IdCliente { get; set; }
        
        /// <summary>
        /// ID de la venta asociada a la notificación.
        /// </summary>
        [Required(ErrorMessage = "El ID de la venta es obligatorio.")]
        public int IdVenta { get; set; }
        
        /// <summary>
        /// Titulo de la notificación.
        /// </summary>
        [Required(ErrorMessage = "El titulo de la notificacion es obligatorio.")]
        public string Titulo { get; set; }
        
        /// <summary>
        /// Mensaje de la notificación.
        /// </summary>
        [Required(ErrorMessage = "El mensaje de la notificacion es obligatorio.")]
        public string Mensaje { get; set; }
        
        /// <summary>
        /// Fecha y hora de la notificación.
        /// </summary>
        [Required(ErrorMessage = "La fecha es obligatoria.")]
        [DataType(DataType.DateTime)]
        public DateTime Fecha { get; set; } = DateTime.UtcNow;
        
        /// <summary>
        /// Indica si la notificación ha sido leída.
        /// </summary>
        public bool Leida { get; set; }
        
        /// <summary>
        /// Estado de la venta (Ej: "Recibida", "Despachada", etc.).
        /// </summary>
        [Required (ErrorMessage = "El estado de la venta es obligatorio.")]
        public string EstadoDeLaVenta { get; set; }
        
        /// <summary>
        /// Número de factura asociado a la venta.
        /// </summary>
        [Required (ErrorMessage = "El numero de factura es obligatorio.")]
        [MaxLength(5, ErrorMessage = "El número de factura no puede exceder los 5 caracteres.")]
        public string NumeroDeFactura { get; set; }
    }
}