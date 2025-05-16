namespace SuperBodega.API.Models.Ecommerce
{
    public class DetalleNotificacionEmail
    {
        public string NombreDelProducto { get; set; }
        public string CodigoDelProducto { get; set; }
        public string ImagenUrlDelProducto { get; set; }
        public string CategoriaDelProducto { get; set; }
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal MontoDePago { get; set; }
        public decimal MontoDeCambio { get; set; }
        public decimal SubTotal { get; set; }
    }
}