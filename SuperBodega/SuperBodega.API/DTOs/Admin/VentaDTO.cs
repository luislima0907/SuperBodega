using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SuperBodega.API.DTOs.Admin
{
    public class VentaDTO
    {
        public int Id { get; set; }
        public string NumeroDeFactura { get; set; }
        public int IdCliente { get; set; }
        public string NombreCompletoCliente { get; set; }
        public string EmailCliente { get; set; }
        public int IdEstadoDeLaVenta { get; set; }
        public string NombreEstadoDeLaVenta { get; set; }
        public decimal MontoDePago { get; set; }
        public decimal MontoDeCambio { get; set; }
        public decimal MontoTotal { get; set; }
        public DateTime FechaDeRegistro { get; set; } = DateTime.UtcNow;
        public List<DetalleDeLaVentaDTO> DetallesDeLaVenta { get; set; }
    }

    public class CreateVentaDTO
    {
        [Required]
        public int IdCliente { get; set; }

        [Required]
        [Range(0.01, (double)decimal.MaxValue)]
        public decimal MontoDePago { get; set; }

        public DateTime FechaDeRegistro { get; set; } = DateTime.UtcNow; 

        [Required]
        [MinLength(1, ErrorMessage = "La venta debe tener al menos un producto.")]
        public List<CreateDetalleDeLaVentaDTO> Detalles { get; set; }
        public bool UsarNotificacionSincronica { get; set; } = false;
    }

}