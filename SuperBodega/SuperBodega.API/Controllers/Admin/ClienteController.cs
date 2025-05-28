using Microsoft.AspNetCore.Mvc;
using SuperBodega.API.DTOs.Admin;
using SuperBodega.API.Services.Admin;

namespace SuperBodega.API.Controllers.Admin;

/// <summary>
/// Controlador para gestionar clientes en el sistema
/// </summary>
[Route("api/[controller]")]
[ApiController]
[Produces("application/json")]
[ApiConventionType(typeof(DefaultApiConventions))]
public class ClienteController : ControllerBase
{
    private readonly ClienteService _clienteService;
    
    public ClienteController(ClienteService clienteService)
    {
        _clienteService = clienteService;
    }
    
    /// <summary>
    /// Obtiene todos los clientes registrados en el sistema
    /// </summary>
    /// <returns>Lista de todos los clientes</returns>
    /// <response code="200">Retorna la lista de clientes</response>
    /// <response code="404">No se encontraron clientes.</response>
    /// <response code="500">Error interno del servidor</response>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<ClienteDTO>))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAllClientesAsync()
    {
        var clientes = await _clienteService.GetAllClientesAsync();
        return Ok(clientes);
    }

    /// <summary>
    /// Obtiene un cliente por su ID
    /// </summary>
    /// <param name="id">ID del cliente a buscar</param>
    /// <returns>Datos del cliente solicitado</returns>
    /// <response code="200">Devuelve el cliente solicitado</response>
    /// <response code="404">Si el cliente no existe</response>
    /// <response code="500">Error interno del servidor</response>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ClienteDTO))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetClienteById(int id)
    {
        var cliente = await _clienteService.GetClienteByIdAsync(id);
        if (cliente == null)
        {
            return NotFound();
        }
        return Ok(cliente);
    }
    
    /// <summary>
    /// Crea un nuevo cliente en el sistema
    /// </summary>
    /// <param name="createClienteDto">Datos del cliente a crear</param>
    /// <returns>El cliente recién creado con su ID asignado</returns>
    /// <response code="201">Retorna el nuevo cliente creado</response>
    /// <response code="400">Si los datos suministrados son inválidos</response>
    /// <response code="500">Error interno del servidor</response>
    [HttpPost("Create")]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(ClienteDTO))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateCliente([FromBody] CreateClienteDTO createClienteDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        
        var newCliente = await _clienteService.CreateClienteAsync(createClienteDto);
        return CreatedAtAction(nameof(GetClienteById), new { id = newCliente.Id }, newCliente);
    }
    
    /// <summary>
    /// Actualiza los datos de un cliente existente
    /// </summary>
    /// <param name="id">ID del cliente a actualizar</param>
    /// <param name="updateClienteDto">Datos actualizados del cliente</param>
    /// <returns>El cliente con sus datos actualizados</returns>
    /// <response code="200">Si el cliente se actualizó correctamente</response>
    /// <response code="400">Si los datos suministrados son inválidos</response>
    /// <response code="404">Si el cliente no existe</response>
    /// <response code="500">Error interno del servidor</response>
    [HttpPut("Edit/{id}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ClienteDTO))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateCliente(int id, [FromBody] UpdateClienteDTO updateClienteDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        
        var updatedCliente = await _clienteService.UpdateClienteAsync(id, updateClienteDto);
        if (updatedCliente == null)
        {
            return NotFound();
        }
        
        return Ok(updatedCliente);
    }
    
    /// <summary>
    /// Elimina un cliente del sistema
    /// </summary>
    /// <param name="id">ID del cliente a eliminar</param>
    /// <returns>Sin contenido si se eliminó correctamente</returns>
    /// <response code="204">Si el cliente se eliminó correctamente</response>
    /// <response code="404">Si el cliente no existe</response>
    /// <response code="500">Error interno del servidor</response>
    [HttpDelete("Delete/{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteCliente(int id)
    {
        var result = await _clienteService.DeleteClienteAsync(id);
        if (!result)
        {
            return NotFound();
        }
        
        return NoContent();
    }
}