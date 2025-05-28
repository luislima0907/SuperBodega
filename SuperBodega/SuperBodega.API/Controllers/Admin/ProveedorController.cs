using Microsoft.AspNetCore.Mvc;
using SuperBodega.API.DTOs.Admin;
using SuperBodega.API.Services.Admin;

namespace SuperBodega.API.Controllers.Admin;

/// <summary>
/// Controlador para gestionar proveedores en el sistema
/// </summary>
[Route("api/[controller]")]
[ApiController]
[Produces("application/json")]
[ApiConventionType(typeof(DefaultApiConventions))]
public class ProveedorController : ControllerBase
{
    private readonly ProveedorService _proveedorService;
    
    public ProveedorController(ProveedorService proveedorService)
    {
        _proveedorService = proveedorService;
    }
    
    /// <summary>
    /// Obtiene todos los proveedores registrados en el sistema
    /// </summary>
    /// <returns>Lista de todos los proveedores</returns>
    /// <response code="200">Retorna la lista de proveedores</response>
    /// <response code="404">No se encontraron proveedores.</response>
    /// <response code="500">Error interno del servidor</response>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<ProveedorDTO>))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAllProveedoresAsync()
    {
        var proveedores = await _proveedorService.GetAllProveedoresAsync();
        return Ok(proveedores);
    }

    /// <summary>
    /// Obtiene un proveedor por su ID
    /// </summary>
    /// <param name="id">ID del proveedor a buscar</param>
    /// <returns>Datos del proveedor solicitado</returns>
    /// <response code="200">Devuelve el proveedor solicitado</response>
    /// <response code="404">Si el proveedor no existe</response>
    /// <response code="500">Error interno del servidor</response>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ProveedorDTO))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetProveedorById(int id)
    {
        var proveedor = await _proveedorService.GetProveedorByIdAsync(id);
        if (proveedor == null)
        {
            return NotFound();
        }
        return Ok(proveedor);
    }
    
    /// <summary>
    /// Crea un nuevo proveedor en el sistema
    /// </summary>
    /// <param name="proveedorDto">Datos del proveedor a crear</param>
    /// <returns>Datos del proveedor creado</returns>
    /// <response code="201">Retorna el nuevo proveedor creado</response>
    /// <response code="400">Si los datos del proveedor son inválidos</response>
    /// <response code="500">Error interno del servidor</response>
    [HttpPost("Create")]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(ProveedorDTO))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateProveedor([FromBody] CreateProveedorDTO proveedorDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        
        var newProveedor = await _proveedorService.CreateProveedorAsync(proveedorDto);
        return CreatedAtAction(nameof(GetProveedorById), new { id = newProveedor.Id }, newProveedor);
    }
    
    /// <summary>
    /// Actualiza un proveedor existente en el sistema
    /// </summary>
    /// <param name="id">ID del proveedor a actualizar</param>
    /// <param name="proveedorDto">Datos actualizados del proveedor</param>
    /// <returns>Datos del proveedor actualizado</returns>
    /// <response code="200">Retorna el proveedor actualizado</response>
    /// <response code="400">Si los datos del proveedor son inválidos</response>
    /// <response code="404">Si el proveedor no existe</response>
    /// <response code="500">Error interno del servidor</response>
    [HttpPut("Edit/{id}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ProveedorDTO))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateProveedor(int id, [FromBody] UpdateProveedorDTO proveedorDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        
        var updatedProveedor = await _proveedorService.UpdateProveedorAsync(id, proveedorDto);
        if (updatedProveedor == null)
        {
            return NotFound();
        }
        
        return Ok(updatedProveedor);
    }
    
    /// <summary>
    /// Elimina un proveedor del sistema
    /// </summary>
    /// <param name="id">ID del proveedor a eliminar</param>
    /// <response code="204">Si el proveedor se eliminó correctamente</response>
    /// <response code="404">Si el proveedor no existe</response>
    /// <response code="500">Error interno del servidor</response>
    [HttpDelete("Delete/{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteProveedor(int id)
    {
        var result = await _proveedorService.DeleteProveedorAsync(id);
        if (!result)
        {
            return NotFound();
        }
        
        return NoContent();
    }
}