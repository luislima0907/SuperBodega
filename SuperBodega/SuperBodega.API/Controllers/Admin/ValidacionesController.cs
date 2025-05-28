using Microsoft.AspNetCore.Mvc;
using SuperBodega.API.Repositories.Interfaces.Admin;

namespace SuperBodega.API.Controllers.Admin
{
    /// <summary>
    /// Controlador para validar la existencia de ventas activas
    /// en productos, categorías, clientes y proveedores.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    [ApiConventionType(typeof(DefaultApiConventions))]
    public class ValidacionesController : ControllerBase
    {
        private readonly IVentaRepository _ventaRepository;
        private readonly IDetalleDeLaVentaRepository _detallesRepository;
        
        public ValidacionesController(IVentaRepository ventaRepository, IDetalleDeLaVentaRepository detallesRepository)
        {
            _ventaRepository = ventaRepository;
            _detallesRepository = detallesRepository;
        }
        
        /// <summary>
        /// Verifica si un producto tiene ventas activas.
        /// </summary>
        /// <param name="id">ID del producto a verificar.</param>
        /// <returns>Un objeto JSON que indica si el producto tiene ventas activas.</returns>
        [HttpGet("producto/{id}/tieneVentasActivas")]
        public async Task<IActionResult> VerificarProductoEnVentasActivas(int id)
        {
            var tieneVentasActivas = await _detallesRepository.ProductoTieneVentasActivas(id);
            return Ok(new { tieneVentasActivas });
        }
        
        /// <summary>
        /// Verifica si una categoría tiene ventas activas.
        /// </summary>
        /// <param name="id">ID de la categoría a verificar.</param>
        /// <returns>Un objeto JSON que indica si la categoría tiene ventas activas.</returns>
        [HttpGet("categoria/{id}/tieneVentasActivas")]
        public async Task<IActionResult> VerificarCategoriaEnVentasActivas(int id)
        {
            var tieneVentasActivas = await _detallesRepository.CategoriaTieneVentasActivas(id);
            return Ok(new { tieneVentasActivas });
        }
        
        /// <summary>
        /// Verifica si un cliente tiene ventas activas.
        /// </summary>
        /// <param name="id">ID del cliente a verificar.</param>
        /// <returns>Un objeto JSON que indica si el cliente tiene ventas activas.</returns>
        [HttpGet("cliente/{id}/tieneVentasActivas")]
        public async Task<IActionResult> VerificarClienteConVentasActivas(int id)
        {
            var tieneVentasActivas = await _ventaRepository.ClienteTieneVentasActivas(id);
            return Ok(new { tieneVentasActivas });
        }
        
        /// <summary>
        /// Verifica si un proveedor tiene ventas activas.
        /// </summary>
        /// <param name="id">ID del proveedor a verificar.</param>
        /// <returns>Un objeto JSON que indica si el proveedor tiene ventas activas.</returns>
        [HttpGet("proveedor/{id}/tieneVentasActivas")]
        public async Task<IActionResult> VerificarProveedorConVentasActivas(int id)
        {
            var tieneVentasActivas = await _ventaRepository.ProveedorTieneVentasActivas(id);
            return Ok(new { tieneVentasActivas });
        }
    }
}