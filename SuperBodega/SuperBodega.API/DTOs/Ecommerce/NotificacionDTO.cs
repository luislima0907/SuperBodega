namespace SuperBodega.API.DTOs.Ecommerce;

public class NotificacionDTO
{
    public int Id { get; set; }
    public string Titulo { get; set; }
    public string Mensaje { get; set; }
    public DateTime Fecha { get; set; }
    public string EstadoDeLaVenta { get; set; }
    public string NumeroDeFactura { get; set; }
    public int IdVenta { get; set; }
    public bool Leida { get; set; }
}