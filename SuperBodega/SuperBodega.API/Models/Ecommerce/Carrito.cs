using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using SuperBodega.API.Models.Admin;

namespace SuperBodega.API.Models.Ecommerce
{
    public class Carrito
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ClienteId { get; set; }

        [Required]
        public DateTime FechaCreacion { get; set; }

        // Relación con Cliente
        [ForeignKey("ClienteId")]
        public virtual Cliente Cliente { get; set; }

        // Relación con elementos del carrito
        public virtual ICollection<ElementoCarrito> Elementos { get; set; } = new List<ElementoCarrito>();

        // Propiedad calculada que no se guarda en la base de datos
        [NotMapped]
        public decimal? Total => Elementos?.Sum(e => e.Subtotal) ?? 0;
    }
}
