namespace SuperBodega.API.Models
{
    public class Notificacion
    {
        public int Id { get; set; }
        public int VentaId { get; set; }
        public required string Mensaje { get; set; }
        public DateTime Fecha { get; set; }
        public TipoNotificacion Tipo { get; set; }
        public bool Leida { get; set; }
    }
}