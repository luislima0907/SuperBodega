using System;

namespace SuperBodega.API.Models.Ecommerce
{
    public class Notificacion
    {
        public int Id { get; set; }
        public int IdCliente { get; set; }
        public int IdVenta { get; set; }
        public string Titulo { get; set; }
        public string Mensaje { get; set; }
        public DateTime Fecha { get; set; }
        public bool Leida { get; set; }
        public string EstadoDeLaVenta { get; set; }
        public string NumeroDeFactura { get; set; }
    }
}