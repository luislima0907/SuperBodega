namespace SuperBodega.API.DTOs.Admin;

/// <summary>
/// DTO para visualizar datos completos de un reporte de una venta
/// </summary>
public class VentaReporteDTO
{
    /// <summary>
    /// Identificador único de la venta
    /// </summary>
    public int Id { get; set; }
    
    /// <summary>
    /// Número de factura de la venta
    /// </summary>
    public string NumeroDeFactura { get; set; }
    
    /// <summary>
    /// Fecha de registro de la venta
    /// </summary>
    public DateTime FechaDeRegistro { get; set; }
    
    /// <summary>
    /// Identificador del cliente asociado a la venta
    /// </summary>
    public int IdCliente { get; set; }
    
    /// <summary>
    /// Informacion completa del cliente asociado a la venta
    /// </summary>
    public ClienteReporteDTO Cliente { get; set; }
    
    /// <summary>
    /// Monto total de la venta
    /// </summary>
    public decimal MontoTotal { get; set; }
    
    /// <summary>
    /// Monto de pago de la venta
    /// </summary>
    public decimal MontoDePago { get; set; }
    
    /// <summary>
    /// Monto de cambio de la venta
    /// </summary>
    public decimal MontoDeCambio { get; set; }
    
    /// <summary>
    /// Nombre del estado de la venta
    /// </summary>
    public string NombreEstadoDeLaVenta { get; set; }
    
    /// <summary>
    /// Informacion completa del proveedor asociado a la venta
    /// </summary>
    public ProveedorReporteDTO Proveedor { get; set; }
    
    /// <summary>
    /// Detalles de la venta
    /// </summary>
    public List<DetalleVentaReporteDTO> Detalles { get; set; }
}

/// <summary>
/// DTO para visualizar datos completos de un detalle de una venta
/// </summary>
public class DetalleVentaReporteDTO
{
    /// <summary>
    /// Identificador único del producto asociado al detalle de la venta
    /// </summary>
    public int IdProducto { get; set; }
    
    /// <summary>
    /// Nombre del producto asociado al detalle de la venta
    /// </summary>
    public string NombreDelProducto { get; set; }
    
    /// <summary>
    /// Código del producto asociado al detalle de la venta
    /// </summary>
    public string CodigoDelProducto { get; set; }
    
    /// <summary>
    /// Imagen del producto asociado al detalle de la venta
    /// </summary>
    public string ImagenDelProducto { get; set; }
    
    /// <summary>
    /// Precio de venta del producto asociado al detalle de la venta
    /// </summary>
    public decimal PrecioDeVenta { get; set; }
    
    /// <summary>
    /// Cantidad del producto asociado al detalle de la venta
    /// </summary>
    public int Cantidad { get; set; }
    
    /// <summary>
    /// Subtotal del detalle de la venta
    /// </summary>
    public decimal Subtotal { get; set; }
    
    /// <summary>
    /// Nombre de la categoria del producto asociado al detalle de la venta
    /// </summary>
    public string NombreCategoria { get; set; }
    
    /// <summary>
    /// Informacion completa del proveedor asociado al detalle de la venta
    /// </summary>
    public ProveedorReporteDTO Proveedor { get; set; }
}

/// <summary>
/// DTO para visualizar datos completos de un cliente en una venta
/// </summary>
public class ClienteReporteDTO
{
    /// <summary>
    /// Identificador único del cliente
    /// </summary>
    public int Id { get; set; }
    
    /// <summary>
    /// Nombre completo del cliente
    /// </summary>
    public string NombreCompleto { get; set; }
    
    /// <summary>
    /// Email del cliente
    /// </summary>
    public string Email { get; set; }
    
    /// <summary>
    /// Teléfono del cliente
    /// </summary>
    public string Telefono { get; set; }
    
    /// <summary>
    /// Dirección del cliente
    /// </summary>
    public string Direccion { get; set; }
}

/// <summary>
/// DTO para visualizar datos completos de un proveedor en una venta
/// </summary>
public class ProveedorReporteDTO
{
    /// <summary>
    /// Identificador único del proveedor
    /// </summary>
    public int Id { get; set; }
    
    /// <summary>
    /// Nombre del proveedor
    /// </summary>
    public string Nombre { get; set; }
    
    /// <summary>
    /// Teléfono del proveedor
    /// </summary>
    public string Telefono { get; set; }
    
    /// <summary>
    /// Dirección del proveedor
    /// </summary>
    public string Direccion { get; set; }
    
    /// <summary>
    /// Email del proveedor
    /// </summary>
    public string Email { get; set; }
}