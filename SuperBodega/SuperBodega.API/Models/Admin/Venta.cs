using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SuperBodega.API.Models.Admin
{
    public class Venta
    {
        [Key]
        public int Id { get; set; }

        [MaxLength(50)]
        public string NumeroDeFactura { get; set; }

        // Foreign Key for Cliente
        public int IdCliente { get; set; }
        [ForeignKey("IdCliente")]
        public virtual Cliente Cliente { get; set; }

        [Column(TypeName = "decimal(10, 2)")]
        public decimal MontoDePago { get; set; }

        [Column(TypeName = "decimal(10, 2)")]
        public decimal MontoDeCambio { get; set; }

        [Column(TypeName = "decimal(10, 2)")]
        public decimal MontoTotal { get; set; }

        // Foreign Key for EstadoDeLaVenta
        public int IdEstadoDeLaVenta { get; set; }
        [ForeignKey("IdEstadoDeLaVenta")]
        public virtual EstadoDeLaVenta EstadoDeLaVenta { get; set; }

        public DateTime FechaDeRegistro { get; set; } = DateTime.UtcNow;

        // Navigation property for details
        public virtual ICollection<DetalleDeLaVenta> DetallesDeLaVenta { get; set; } = new List<DetalleDeLaVenta>();
    }
}