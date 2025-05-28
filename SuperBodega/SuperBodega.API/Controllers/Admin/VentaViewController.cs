using Microsoft.AspNetCore.Mvc;
using SuperBodega.API.Services.Admin;

namespace SuperBodega.API.Controllers.Admin
{
    /// <summary>
    /// Controlador para gestionar las vistas relacionadas con las ventas.
    /// </summary>
    /// <remarks>
    /// Proporciona acceso a las páginas web para administrar ventas,
    /// como listado, edición de estado y procesar devolución.
    /// </remarks>
    [Route("Ventas")] 
    public class VentaViewController : Controller
    {
        private readonly VentaService _ventaService;
        private readonly EstadoDeLaVentaService _estadoDeLaVentaService;

        /// <summary>
        /// Inicializa una nueva instancia del controlador de vistas de ventas.
        /// </summary>
        /// <param name="ventaService">Servicio de ventas a utilizar</param>
        /// <param name="estadoDeLaVentaService">Servicio de estado de la venta a utilizar</param>
        public VentaViewController(VentaService ventaService, EstadoDeLaVentaService estadoDeLaVentaService)
        {
            _ventaService = ventaService;
            _estadoDeLaVentaService = estadoDeLaVentaService;
        }

        /// <summary>
        /// Muestra la página de listado de ventas.
        /// </summary>
        /// <returns>Vista de listado de ventas</returns>
        // GET: /Ventas
        [HttpGet("Index")]
        public IActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Muestra la página para ver el detalle de una venta.
        /// </summary>
        /// <returns>Vista del detalle de la venta</returns>
        /// <remarks>
        /// Si la venta no existe, retorna NotFound (404).
        /// </remarks>
        // GET: /Ventas/Details/{id}
        [HttpGet("Details/{id}")]
        public async Task<IActionResult> Details(int id)
        {
            var venta = await _ventaService.GetVentaByIdWithDetailsAsync(id);
            if (venta == null)
            {
                return NotFound();
            }
            return View(venta);
        }

        /// <summary>
        /// Muestra la página para editar el estado de una venta.
        /// </summary>
        /// <param name="id">ID de la venta a editar</param>
        /// <returns>Vista de edición del estado de la venta</returns>
        /// <remarks>
        /// Si la venta no existe, retorna NotFound (404).
        /// </remarks>
        // GET: /Ventas/Edit/{id}
        [HttpGet("Edit/{id}")]
        public async Task<IActionResult> Edit(int id)
        {
            var venta = await _ventaService.GetVentaByIdWithDetailsAsync(id);
            if (venta == null)
            {
                return NotFound();
            }
            
            return View(venta);
        }
    }
}