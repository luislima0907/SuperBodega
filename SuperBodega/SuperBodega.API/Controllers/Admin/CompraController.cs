using Microsoft.AspNetCore.Mvc;
using SuperBodega.API.DTOs.Admin;
using SuperBodega.API.Services.Admin;

namespace SuperBodega.API.Controllers.Admin;

[Route("api/[controller]")]
[ApiController]
public class CompraController : ControllerBase
{
    private readonly CompraService _compraService;
    
    public CompraController(CompraService compraService)
    {
        _compraService = compraService;
    }
    
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var compras = await _compraService.GetAllAsync();
        return Ok(compras);
    }
    
    [HttpGet("details")]
    public async Task<IActionResult> GetAllWithDetails()
    {
        var compras = await _compraService.GetAllWithDetailsAsync();
        return Ok(compras);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var compra = await _compraService.GetByIdAsync(id);
        if (compra == null)
        {
            return NotFound();
        }
        return Ok(compra);
    }

    [HttpGet("{id}/details")]
    public async Task<IActionResult> GetWithDetails(int id)
    {
        var compra = await _compraService.GetWithDetailsAsync(id);
        if (compra == null)
        {
            return NotFound();
        }
        return Ok(compra);
    }

    [HttpGet("proveedor/{proveedorId}")]
    public async Task<IActionResult> GetByProveedor(int proveedorId)
    {
        var compras = await _compraService.GetByProveedorIdAsync(proveedorId);
        if (compras == null || !compras.Any())
        {
            return NotFound();
        }
        return Ok(compras);
    }
    
    [HttpPost("Create")]
    public async Task<IActionResult> Create([FromBody] CreateCompraDTO compraDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        
        var compra = await _compraService.CreateAsync(compraDto);
        return CreatedAtAction(nameof(GetById), new { id = compra.Id }, compra);
    }
    
    [HttpPut("Edit/{id}")]
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
    
    [HttpDelete("Delete/{id}")]
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