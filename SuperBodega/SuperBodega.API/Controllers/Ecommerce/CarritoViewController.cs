using Microsoft.AspNetCore.Mvc;
using SuperBodega.API.Services.Admin;

namespace SuperBodega.API.Controllers.Ecommerce
{
    /// <summary>
    /// Controlador para gestionar las vistas relacionadas con el carrito.
    /// </summary>
    [Route("Carrito")]
    public class CarritoViewController : Controller
    {
        private readonly ClienteService _clienteService;
        
        /// <summary>
        /// Inicializa una nueva instancia del controlador de vistas del carrito.
        /// </summary>
        /// <param name="clienteService">Servicio de clientes a utilizar</param>
        public CarritoViewController(ClienteService clienteService)
        {
            _clienteService = clienteService;
        }
        
        /// <summary>
        /// Muestra la página de listado del carrito.
        /// </summary>
        /// <returns>Vista de listado del carrito</returns>
        // GET: CarritoView/Index
        [HttpGet("Index")]
        public async Task<IActionResult> Index()
        {
            var clientes = await _clienteService.GetAllClientesAsync();
            var clientesActivos = clientes.Where(c => c.Estado).ToList();
            
            ViewData["Clientes"] = clientesActivos;
            return View();
        }
    }
}