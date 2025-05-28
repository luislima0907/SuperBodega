using Microsoft.AspNetCore.Mvc;

namespace SuperBodega.API.Controllers.Ecommerce
{
    /// <summary>
    /// Controlador para gestionar las vistas relacionadas con los pedidos.
    /// </summary>
    [Route("MisPedidos")]
    public class MisPedidosViewController : Controller
    {
        /// <summary>
        /// Muestra la página de listado de los pedidos del cliente.
        /// </summary>
        /// <returns>Vista de listado de pedidos</returns>
        // GET: /MisPedidos/Index
        [HttpGet("Index")]
        public IActionResult Index()
        {
            return View();
        }
    }
}