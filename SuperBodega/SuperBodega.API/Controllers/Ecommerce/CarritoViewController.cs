using Microsoft.AspNetCore.Mvc;
using SuperBodega.API.Services.Admin;
using SuperBodega.API.Services.Ecommerce;

namespace SuperBodega.API.Controllers.Ecommerce
{
    [Route("Carrito")]
    public class CarritoViewController : Controller
    {
        private readonly ClienteService _clienteService;

        public CarritoViewController(ClienteService clienteService)
        {
            _clienteService = clienteService;
        }

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