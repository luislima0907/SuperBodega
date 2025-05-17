using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using SuperBodega.API.Repositories.Interfaces.Admin;

namespace SuperBodega.API.Controllers.Admin
{
    [ApiController]
    [Route("api/[controller]")]
    public class ValidacionesController : ControllerBase
    {
        private readonly IVentaRepository _ventaRepository;
        private readonly IDetalleDeLaVentaRepository _detallesRepository;

        public ValidacionesController(IVentaRepository ventaRepository, IDetalleDeLaVentaRepository detallesRepository)
        {
            _ventaRepository = ventaRepository;
            _detallesRepository = detallesRepository;
        }

        [HttpGet("producto/{id}/tieneVentasActivas")]
        public async Task<IActionResult> VerificarProductoEnVentasActivas(int id)
        {
            var tieneVentasActivas = await _detallesRepository.ProductoTieneVentasActivas(id);
            return Ok(new { tieneVentasActivas });
        }

        [HttpGet("categoria/{id}/tieneVentasActivas")]
        public async Task<IActionResult> VerificarCategoriaEnVentasActivas(int id)
        {
            var tieneVentasActivas = await _detallesRepository.CategoriaTieneVentasActivas(id);
            return Ok(new { tieneVentasActivas });
        }

        [HttpGet("cliente/{id}/tieneVentasActivas")]
        public async Task<IActionResult> VerificarClienteConVentasActivas(int id)
        {
            var tieneVentasActivas = await _ventaRepository.ClienteTieneVentasActivas(id);
            return Ok(new { tieneVentasActivas });
        }

        [HttpGet("proveedor/{id}/tieneVentasActivas")]
        public async Task<IActionResult> VerificarProveedorConVentasActivas(int id)
        {
            var tieneVentasActivas = await _ventaRepository.ProveedorTieneVentasActivas(id);
            return Ok(new { tieneVentasActivas });
        }
    }
}