using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SuperBodega.API.Models.Admin
{
    public class EstadoDeLaVenta
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Nombre { get; set; } // e.g., "Recibida", "Despachada", "Entregada"

        // Navigation property
        public virtual ICollection<Venta> Ventas { get; set; }
        public virtual ICollection<DetalleDeLaVenta> DetallesDeLaVenta { get; set; } // Potentially redundant if Venta has state
    }
}