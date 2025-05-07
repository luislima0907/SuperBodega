namespace SuperBodega.API.Models.Admin;

public class Proveedor
{
    public int Id { get; set; }
    public string Nombre { get; set; }
    public string Email { get; set; }
    public string Telefono { get; set; }
    public string Direccion { get; set; }
    public bool Estado { get; set; }
    public DateTime FechaDeRegistro { get; set; } = DateTime.UtcNow;
    //EN UN FUTURO
    //public virutal ICollection<Compra> Compras  { get; set; } = new List
 }