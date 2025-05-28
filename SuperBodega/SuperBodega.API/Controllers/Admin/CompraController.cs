using Microsoft.AspNetCore.Mvc;
using SuperBodega.API.DTOs.Admin;
using SuperBodega.API.Services.Admin;

namespace SuperBodega.API.Controllers.Admin;

/// <summary>
/// Controlador para gestionar las compras en el sistema
/// </summary>
[Route("api/[controller]")]
[ApiController]
[Produces("application/json")]
[ApiConventionType(typeof(DefaultApiConventions))]
public class CompraController : ControllerBase
{
    private readonly CompraService _compraService;
    
    public CompraController(CompraService compraService)
    {
        _compraService = compraService;
    }
    
    /// <summary>
    /// Obtiene todas las compras registradas en el sistema
    /// </summary>
    /// <response code="200">Retorna la lista de compras</response>
    /// <response code="404">No se encontraron compras.</response>
    /// <response code="500">Error interno del servidor</response>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<CompraDTO>))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAll()
    {
        var compras = await _compraService.GetAllAsync();
        return Ok(compras);
    }
    
    /// <summary>
    /// Obtiene todas las compras con detalles
    /// </summary>
    /// <response code="200">Retorna la lista de compras con detalles</response>
    /// <response code="404">No se encontraron compras.</response>
    /// <response code="500">Error interno del servidor</response>
    [HttpGet("details")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<CompraDTO>))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAllWithDetails()
    {
        var compras = await _compraService.GetAllWithDetailsAsync();
        return Ok(compras);
    }

    /// <summary>
    /// Obtiene una compra por su ID
    /// </summary>
    /// <param name="id">ID de la compra a buscar</param>
    /// <returns>Datos de la compra solicitada</returns>
    /// <response code="200">Devuelve la compra solicitada</response>
    /// <response code="404">Si la compra no existe</response>
    /// <response code="500">Error interno del servidor</response>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CompraDTO))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetById(int id)
    {
        var compra = await _compraService.GetByIdAsync(id);
        if (compra == null)
        {
            return NotFound();
        }
        return Ok(compra);
    }


    /// <summary>
    /// Obtiene una compra por su ID con detalles
    /// </summary>
    /// <param name="id">ID de la compra a buscar</param>
    /// <returns>Datos de la compra solicitada</returns>
    /// <response code="200">Devuelve la compra solicitada</response>
    /// <response code="404">Si la compra no existe</response>
    /// <response code="500">Error interno del servidor</response>
    [HttpGet("{id}/details")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CompraDTO))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetWithDetails(int id)
    {
        var compra = await _compraService.GetWithDetailsAsync(id);
        if (compra == null)
        {
            return NotFound();
        }
        return Ok(compra);
    }

    /// <summary>
    /// Obtiene todas las compras de un proveedor específico
    /// </summary>
    /// <param name="proveedorId">ID del proveedor</param>
    /// <returns>Lista de compras del proveedor</returns>
    /// <response code="200">Devuelve la lista de compras del proveedor</response>
    /// <response code="404">Si no se encuentran compras para el proveedor</response>
    /// <response code="500">Error interno del servidor</response>
    [HttpGet("proveedor/{proveedorId}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<CompraDTO>))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetByProveedor(int proveedorId)
    {
        var compras = await _compraService.GetByProveedorIdAsync(proveedorId);
        if (compras == null || !compras.Any())
        {
            return NotFound();
        }
        return Ok(compras);
    }
    
    /// <summary>
    /// Crea una nueva compra en el sistema
    /// </summary>
    /// <param name="compraDto">Datos de la compra a crear</param>
    /// <returns>La compra recién creada con su ID asignado</returns>
    /// <response code="201">Retorna la compra creada</response>
    /// <response code="400">Si los datos suministrados son inválidos</response>
    /// <response code="500">Error interno del servidor</response>
    [HttpPost("Create")]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(CompraDTO))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Create([FromBody] CreateCompraDTO compraDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        
        var compra = await _compraService.CreateAsync(compraDto);
        return CreatedAtAction(nameof(GetById), new { id = compra.Id }, compra);
    }
    
    /// <summary>
    /// Actualiza una compra existente en el sistema
    /// </summary>
    /// <param name="id">ID de la compra a actualizar</param>
    /// <param name="compraDto">Datos actualizados de la compra</param>
    /// <returns>La compra actualizada</returns>
    /// <response code="200">Retorna la compra actualizada</response>
    /// <response code="400">Si los datos suministrados son inválidos</response>
    /// <response code="404">Si la compra no existe</response>
    /// <response code="500">Error interno del servidor</response>
    [HttpPut("Edit/{id}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CompraDTO))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateCompraDTO compraDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        
        try
        {
            var compra = await _compraService.UpdateAsync(id, compraDto);
            if (compra == null)
            {
                return NotFound();
            }
            
            return Ok(compra);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
    
    /// <summary>
    /// Elimina una compra del sistema
    /// </summary>
    /// <param name="id">ID de la compra a eliminar</param>
    /// <returns>Resultado de la operación</returns>
    /// <response code="204">Si la compra fue eliminada correctamente</response>
    /// <response code="404">Si la compra no existe</response>
    /// <response code="500">Error interno del servidor</response>
    [HttpDelete("Delete/{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _compraService.DeleteAsync(id);
        if (!result)
        {
            return NotFound();
        }
        
        return NoContent();
    }
}