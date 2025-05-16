using System.ComponentModel.DataAnnotations;

namespace SuperBodega.API.DTOs.Admin
{
    public class DetalleDeLaVentaDTO
    {
        public int Id { get; set; }
        public int IdVenta { get; set; }
        public int IdProducto { get; set; }
        public string CodigoDelProducto { get; set; }
        public string NombreDelProducto { get; set; }
        public string ImagenDelProducto { get; set; }
        public string NombreCategoria { get; set; } // Included as per model
        public decimal PrecioDeVenta { get; set; }
        public int Cantidad { get; set; }
        public decimal MontoTotal { get; set; }
        public int IdProveedor { get; set; } // Included as per model
        public string NombreDelProveedor { get; set; } // Included as per model
        public DateTime FechaDeRegistro { get; set; } = DateTime.UtcNow;
    }

    public class CreateDetalleDeLaVentaDTO
    {
        [Required]
        public int IdProducto { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser al menos 1.")]
        public int Cantidad { get; set; }

        [Required]
        public int IdProveedor { get; set; }

        public string NombreDelProveedor { get; set; }

        // PrecioDeVenta might be fetched server-side based on IdProducto to prevent manipulation,
        // or passed from client if pricing can vary. Assuming passed from client for now.
        [Required]
        [Range(0.01, (double)decimal.MaxValue)]
        public decimal PrecioDeVenta { get; set; }

        public DateTime FechaDeRegistro { get; set; } = DateTime.UtcNow;
    }
}