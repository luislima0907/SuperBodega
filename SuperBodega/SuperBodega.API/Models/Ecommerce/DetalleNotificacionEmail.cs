using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SuperBodega.API.Models.Ecommerce
{
    /// <summary>
    /// Clase que representa un detalle de la notificación de email.
    /// </summary>
    public class DetalleNotificacionEmail
    {
        /// <summary>
        /// Nombre del producto.
        /// </summary>
        [Required (ErrorMessage = "El nombre del producto es obligatorio.")]
        [MaxLength(100, ErrorMessage = "El nombre del producto no puede exceder los 100 caracteres.")]
        public string NombreDelProducto { get; set; }
        
        /// <summary>
        /// Código único del producto.
        /// </summary>
        [Required (ErrorMessage = "El codigo del producto es obligatorio.")]
        [StringLength(5, ErrorMessage = "El código no puede exceder los 5 caracteres.")]
        public string CodigoDelProducto { get; set; }
        
        /// <summary>
        /// Imagen URL del producto.
        /// </summary>
        public string ImagenUrlDelProducto { get; set; }
        
        /// <summary>
        /// Nombre de la categoría del producto.
        /// </summary>
        [Required (ErrorMessage = "El nombre de la categoria es obligatorio.")]
        [StringLength(100, ErrorMessage = "El nombre de la categoría no puede exceder los 100 caracteres.")]
        public string CategoriaDelProducto { get; set; }
        
        /// <summary>
        /// Cantidad del producto.
        /// </summary>
        [Required (ErrorMessage = "La cantidad del producto es obligatoria.")]
        public int Cantidad { get; set; }
        
        /// <summary>
        /// Precio unitario del producto.
        /// </summary>
        [Required (ErrorMessage = "El precio unitario es obligatorio.")]
        [Column(TypeName = "decimal(10,2)")] 
        public decimal PrecioUnitario { get; set; }
        
        /// <summary>
        /// Monto de pago del cliente.
        /// </summary>
        [Required (ErrorMessage = "El monto de pago es obligatorio.")]
        [Column(TypeName = "decimal(10,2)")] 
        public decimal MontoDePago { get; set; }
        
        /// <summary>
        /// Monto de cambio que se le debe dar al cliente.
        /// </summary>
        [Required (ErrorMessage = "El monto de cambio es obligatorio.")]
        [Column(TypeName = "decimal(10,2)")]
        public decimal MontoDeCambio { get; set; }
        
        /// <summary>
        /// Subtoal de la compra.
        /// </summary>
        [Required (ErrorMessage = "El SubTotal es obligatorio.")]
        [Column(TypeName = "decimal(10,2)")] 
        public decimal SubTotal { get; set; }
    }
}