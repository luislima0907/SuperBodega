using System.ComponentModel.DataAnnotations;

namespace SuperBodega.API.DTOs.Admin;

public class CompraDTO
{
    public int Id { get; set; }
    public string NumeroDeFactura { get; set; }
    public int IdProveedor { get; set; }
    public string NombreDelProveedor { get; set; }
    public DateTime FechaDeRegistro { get; set; } = DateTime.UtcNow;
    public decimal MontoTotal { get; set; }
    public List<DetalleDeLaCompraDTO> DetallesDeLaCompra { get; set; } = new List<DetalleDeLaCompraDTO>();
}

public class CreateCompraDTO
{
    [Required(ErrorMessage = "El numero de factura es obligatorio")]
    [StringLength(50, ErrorMessage = "El numero de factura no puede exceder los 50 caracteres")]
    public string NumeroDeFactura { get; set; }
    
    [Required(ErrorMessage = "El id del proveedor es obligatorio")]
    public int IdProveedor { get; set; }
    
    [Required(ErrorMessage = "Se requiere al menos un detalle de compra")]
    [MinLength(1, ErrorMessage = "Se requiere al menos un detalle de compra")]
    public List<CreateDetalleDeLaCompraDTO> DetallesDeLaCompra { get; set; } = new List<CreateDetalleDeLaCompraDTO>();

    public DateTime FechaDeRegistro { get; set; } = DateTime.UtcNow;

}

public class UpdateCompraDTO
{
    [Required(ErrorMessage = "El numero de factura es obligatorio")]
    [StringLength(50, ErrorMessage = "El numero de factura no puede exceder los 50 caracteres")]
    public string NumeroDeFactura { get; set; }
    
    [Required(ErrorMessage = "El id del proveedor es obligatorio")]
    public int IdProveedor { get; set; }

    public List<UpdateDetalleDeLaCompraDTO> DetallesDeLaCompra { get; set; } = new List<UpdateDetalleDeLaCompraDTO>();

}