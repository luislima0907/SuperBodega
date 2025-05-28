using Microsoft.AspNetCore.Mvc;

namespace SuperBodega.API.Controllers.Ecommerce
{
    /// <summary>
    /// Controlador para gestionar las vistas relacionadas con la compra.
    /// </summary>
    [Route("RealizarCompra")]
    public class RealizarCompraViewController : Controller
    {
        /// <summary>
        /// Muestra la página de listado de productos para realizar una compra.
        /// </summary>
        /// <returns>Vista de listado de los productos del carrito</returns>
        // GET: /RealizarCompra/Index
        [HttpGet("Index")]
        public IActionResult Index()
        {
            return View();
        }
    }
}