namespace SuperBodega.API.DTOs.Ecommerce;

/// <summary>
/// DTO para visualizar datos completos de una notificación
/// </summary>
public class NotificacionDTO
{
    /// <summary>
    /// Identificador único de la notificación
    /// </summary>
    public int Id { get; set; }
    
    /// <summary>
    /// Título de la notificación
    /// </summary>
    public string Titulo { get; set; }
    
    /// <summary>
    /// Mensaje de la notificación
    /// </summary>
    public string Mensaje { get; set; }
    
    /// <summary>
    /// Fecha de la notificación
    /// </summary>
    public DateTime Fecha { get; set; }
    
    /// <summary>
    /// Estado de la venta
    /// </summary>
    public string EstadoDeLaVenta { get; set; }
    
    /// <summary>
    /// Número de factura de la venta
    /// </summary>
    public string NumeroDeFactura { get; set; }
    
    /// <summary>
    /// Identificador de la venta
    /// </summary>
    public int IdVenta { get; set; }
    
    /// <summary>
    /// Estado de lectura
    /// </summary>
    public bool Leida { get; set; }
}