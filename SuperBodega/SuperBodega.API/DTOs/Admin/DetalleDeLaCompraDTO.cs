using System.ComponentModel.DataAnnotations;

namespace SuperBodega.API.DTOs.Admin;

public class DetalleDeLaCompraDTO
{
    public int Id { get; set; }
    public int IdCompra { get; set; }
    public int IdProducto { get; set; }
    public string NombreDelProducto { get; set; }
    public string CodigoDelProducto { get; set; }
    public string ImagenDelProducto { get; set; }
    public string CategoriaDelProducto { get; set; }
    public decimal PrecioDeCompra { get; set; }
    public decimal PrecioDeVenta { get; set; }
    public int Cantidad { get; set; }
    public decimal Montototal { get; set; }
    public DateTime FechaDeRegistro { get; set; }
}

public class CreateDetalleDeLaCompraDTO
{
    [Required(ErrorMessage = "El Id del producto es obligatorio")]
    public int IdProducto { get; set; }
    
    [Required(ErrorMessage = "El precio de compra es obligatorio")]
    [Range(0.01, double.MaxValue, ErrorMessage = "El precio de compra debe ser mayor que cero")]
    public decimal PrecioDeCompra { get; set; }
    
    [Required(ErrorMessage = "El precio de venta es obligatorio")]
    [Range(0.01, double.MaxValue, ErrorMessage = "El precio de venta debe ser mayor que cero")]
    public decimal PrecioDeVenta { get; set; }
    
    [Required(ErrorMessage = "La cantidad es obligatoria")]
    [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser mayor que cero")]
    public int Cantidad { get; set; }

    public DateTime FechaDeRegistro { get; set; } = DateTime.UtcNow;
}

public class UpdateDetalleDeLaCompraDTO
{
    public int Id { get; set; } // ID existente si lo hay, 0 para nuevos
    public int IdProducto { get; set; }
    public decimal PrecioDeCompra { get; set; }
    public decimal PrecioDeVenta { get; set; }
    public int Cantidad { get; set; }
}