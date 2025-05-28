namespace SuperBodega.API.DTOs.Admin;

/// <summary>
/// DTO para visualizar datos completos de una compra
/// </summary>
public class CompraDTO
{
    /// <summary>
    /// Identificador único de la compra
    /// </summary>
    public int Id { get; set; }
    
    /// <summary>
    /// Número de factura de la compra
    /// </summary>
    public string NumeroDeFactura { get; set; }
    
    /// <summary>
    /// Identificador del proveedor asociado a la compra
    /// </summary>
    public int IdProveedor { get; set; }
    
    /// <summary>
    /// Nombre del proveedor asociado a la compra
    /// </summary>
    public string NombreDelProveedor { get; set; }
    
    /// <summary>
    /// Fecha en que se registró la compra
    /// </summary>
    public DateTime FechaDeRegistro { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Monto total de la compra
    /// </summary>
    public decimal MontoTotal { get; set; }
    
    /// <summary>
    /// Detalles de la compra
    /// </summary>
    public List<DetalleDeLaCompraDTO> DetallesDeLaCompra { get; set; } = new List<DetalleDeLaCompraDTO>();
}

/// <summary>
/// DTO para la creación de una nueva compra
/// </summary>
public class CreateCompraDTO
{
    /// <summary>
    /// Número de factura de la compra
    /// </summary>
    public string NumeroDeFactura { get; set; }
    
    /// <summary>
    /// Identificador del proveedor asociado a la compra
    /// </summary>
    public int IdProveedor { get; set; }
    
    /// <summary>
    /// Detalles de la compra
    /// </summary>
    public List<CreateDetalleDeLaCompraDTO> DetallesDeLaCompra { get; set; } = new List<CreateDetalleDeLaCompraDTO>();

    /// <summary>
    /// Fecha en que se registró la compra
    /// </summary>
    public DateTime FechaDeRegistro { get; set; } = DateTime.UtcNow;

}

/// <summary>
/// DTO para la actualización de una compra
/// </summary>
public class UpdateCompraDTO
{
    /// <summary>
    /// Número de factura de la compra
    /// </summary>
    public string NumeroDeFactura { get; set; }
    
    /// <summary>
    /// Identificador del proveedor asociado a la compra
    /// </summary>
    public int IdProveedor { get; set; }

    /// <summary>
    /// Detalles de la compra
    /// </summary>
    public List<UpdateDetalleDeLaCompraDTO> DetallesDeLaCompra { get; set; } = new List<UpdateDetalleDeLaCompraDTO>();
}