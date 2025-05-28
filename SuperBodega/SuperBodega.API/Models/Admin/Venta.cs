using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SuperBodega.API.Models.Admin
{
    /// <summary>
    /// Clase que representa una venta de productos.
    /// </summary>
    public class Venta
    {
        /// <summary>
        /// Identificador único de la venta.
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        /// <summary>
        /// Número de factura de la venta.
        /// </summary>
        [Required (ErrorMessage = "El número de factura es obligatorio.")]
        [StringLength(5, ErrorMessage = "El número de factura no puede exceder los 5 caracteres.")]
        public string NumeroDeFactura { get; set; }
        
        /// <summary>
        /// ID del cliente asociado a la venta.
        /// </summary>
        [Required(ErrorMessage = "El ID del cliente es obligatorio.")]
        public int IdCliente { get; set; }
        
        /// <summary>
        /// Cliente asociado a la venta.
        /// </summary>
        [ForeignKey("IdCliente")]
        public virtual Cliente Cliente { get; set; }

        /// <summary>
        /// Monto de pago realizado por el cliente.
        /// </summary>
        [Required(ErrorMessage = "El monto de pago es obligatorio.")]
        [Column(TypeName = "decimal(10, 2)")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El monto de pago debe ser mayor que cero")]
        public decimal MontoDePago { get; set; }

        /// <summary>
        /// Monto de cambio que se le debe dar al cliente.
        /// </summary>
        [Required(ErrorMessage = "El monto de cambio es obligatorio.")]
        [Column(TypeName = "decimal(10, 2)")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El monto de cambio debe ser mayor que cero")]
        public decimal MontoDeCambio { get; set; }

        /// <summary>
        /// Monto total de la venta.
        /// </summary>
        [Required(ErrorMessage = "El monto total es obligatorio.")]
        [Column(TypeName = "decimal(10, 2)")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El monto total debe ser mayor que cero")]
        public decimal MontoTotal { get; set; }

        /// <summary>
        /// ID del estado asociado a la venta.
        /// </summary>
        [Required(ErrorMessage = "El ID del estado de la venta es obligatorio.")]
        public int IdEstadoDeLaVenta { get; set; }
        
        /// <summary>
        /// Estado asociado a la venta.
        /// </summary>
        [ForeignKey("IdEstadoDeLaVenta")]
        public virtual EstadoDeLaVenta EstadoDeLaVenta { get; set; }

        /// <summary>
        /// Fecha y hora en que se registró la venta.
        /// </summary>
        [Required(ErrorMessage = "La fecha de registro es obligatoria.")]
        [DataType(DataType.DateTime)]
        public DateTime FechaDeRegistro { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Detalles de la venta.
        /// </summary>
        [Required(ErrorMessage = "Los detalles de la venta son obligatorios.")]
        [MinLength(1, ErrorMessage = "Se requiere al menos un detalle")]
        public virtual ICollection<DetalleDeLaVenta> DetallesDeLaVenta { get; set; } = new List<DetalleDeLaVenta>();
    }
}