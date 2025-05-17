using Microsoft.AspNetCore.Mvc;
using SuperBodega.API.DTOs.Admin;
using SuperBodega.API.Services.Admin;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SuperBodega.API.Controllers.Admin
{
    [Route("Ventas")]
    public class VentaViewController : Controller
    {
        private readonly VentaService _ventaService;
        private readonly EstadoDeLaVentaService _estadoDeLaVentaService;

        public VentaViewController(VentaService ventaService, EstadoDeLaVentaService estadoDeLaVentaService)
        {
            _ventaService = ventaService;
            _estadoDeLaVentaService = estadoDeLaVentaService;
        }

        // GET: /Ventas
        [HttpGet("Index")]
        public IActionResult Index()
        {
            return View();
        }

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