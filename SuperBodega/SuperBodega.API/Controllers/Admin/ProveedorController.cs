using Microsoft.AspNetCore.Mvc;
using SuperBodega.API.DTOs.Admin;
using SuperBodega.API.Services.Admin;

namespace SuperBodega.API.Controllers.Admin;

[Route("api/[controller]")]
[ApiController] 
public class ProveedorController : ControllerBase
{
    private readonly ProveedorService _proveedorService;

    public ProveedorController(ProveedorService proveedorService)
    {
        _proveedorService = proveedorService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllProveedoresAsync()
    {
        var proveedores = await _proveedorService.GetAllAsync();
        return Ok(proveedores);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetProveedorByIdAsync(int id)
    {
        var proveedor = await _proveedorService.GetByIdAsync(id);
        if (proveedor == null)
        {
            return NotFound();
        }
        return Ok(proveedor);
    }

    [HttpPost("Create")]
    public async Task<IActionResult> CreateProveedorAsync([FromBody] CreateProveedorDTO proveedorDTO)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var newProveedor = await _proveedorService.CreateProveedorAsync(proveedorDTO);
        return CreatedAtAction(nameof(GetProveedorByIdAsync), new { id = newProveedor.Id }, newProveedor);
    }

    [HttpPut("Edit/{id}")]
    public async Task<IActionResult> UpdateProveedor(int id, [FromBody] UpdateProveedorDTO proveedorDTO)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var updatedProveedor = await _proveedorService.UpdateProveedorAsync(id, proveedorDTO);
        if (updatedProveedor == null)
        {
            return NotFound();
        }

        return Ok(updatedProveedor);
    }

    [HttpDelete("Delete/{id}")]
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