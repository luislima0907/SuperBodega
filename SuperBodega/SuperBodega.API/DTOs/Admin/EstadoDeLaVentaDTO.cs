using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace SuperBodega.API.DTOs.Admin
{
    public class EstadoDeLaVentaDTO
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
    }

    public class UpdateEstadoDeLaVentaDTO
    {
        [Required]
        public int IdEstadoDeLaVenta { get; set; }
        public bool UsarNotificacionSincronica { get; set; } = false;
    }

}