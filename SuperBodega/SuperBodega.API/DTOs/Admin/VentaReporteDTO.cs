namespace SuperBodega.API.DTOs.Admin;

public class VentaReporteDTO
{
    public int Id { get; set; }
    public string NumeroDeFactura { get; set; }
    public DateTime FechaDeRegistro { get; set; }
    public int IdCliente { get; set; }
    public ClienteReporteDTO Cliente { get; set; }
    public decimal MontoTotal { get; set; }
    public decimal MontoDePago { get; set; }
    public decimal MontoDeCambio { get; set; }
    public string NombreEstadoDeLaVenta { get; set; }
    public ProveedorReporteDTO Proveedor { get; set; }
    public List<DetalleVentaReporteDTO> Detalles { get; set; }
}

public class DetalleVentaReporteDTO
{
    public int IdProducto { get; set; }
    public string NombreDelProducto { get; set; }
    public string CodigoDelProducto { get; set; }
    public string ImagenDelProducto { get; set; }
    public decimal PrecioDeVenta { get; set; }
    public int Cantidad { get; set; }
    public decimal Subtotal { get; set; }
    public string NombreCategoria { get; set; }
    public ProveedorReporteDTO Proveedor { get; set; }
}

public class ClienteReporteDTO
{
    public int Id { get; set; }
    public string NombreCompleto { get; set; }
    public string Email { get; set; }
    public string Telefono { get; set; }
    public string Direccion { get; set; }
}

public class ProveedorReporteDTO
{
    public int Id { get; set; }
    public string Nombre { get; set; }
    public string Telefono { get; set; }
    public string Direccion { get; set; }
    public string Email { get; set; }
}