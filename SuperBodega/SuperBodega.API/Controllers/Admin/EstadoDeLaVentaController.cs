using Microsoft.AspNetCore.Mvc;
using SuperBodega.API.DTOs.Admin;
using SuperBodega.API.Services.Admin;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SuperBodega.API.Controllers.Admin
{
  
    [Route("api/[controller]")]
    [ApiController]
    public class EstadoDeLaVentaController : ControllerBase
    {
        private readonly EstadoDeLaVentaService _estadoDeLaVentaService;
        
        public EstadoDeLaVentaController(EstadoDeLaVentaService estadoDeLaVentaService)
        {
            _estadoDeLaVentaService = estadoDeLaVentaService;
        }
        
        [HttpGet]
        public async Task<ActionResult<IEnumerable<EstadoDeLaVentaDTO>>> GetAll()
        {
            var estados = await _estadoDeLaVentaService.GetAllEstadosDeLaVentaAsync();
            return Ok(estados);
        }

        
        [HttpGet("{id}")]
       
        public async Task<ActionResult<EstadoDeLaVentaDTO>> Get(int id)
        {
            var estado = await _estadoDeLaVentaService.GetEstadoDeLaVentaByIdAsync(id);
            if (estado == null)
            {
                return NotFound();
            }
            return Ok(estado);
        }

       
        [HttpPost("Create")]
        
        public async Task<ActionResult<EstadoDeLaVentaDTO>> Create(EstadoDeLaVentaDTO dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var estado = await _estadoDeLaVentaService.CreateEstadoDeLaVentaAsync(dto);
            return CreatedAtAction(nameof(Get), new { id = estado.Id }, estado);
        }
        
        [HttpPut("Edit/{id}")]
        public async Task<IActionResult> Update(int id, EstadoDeLaVentaDTO dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var estado = await _estadoDeLaVentaService.UpdateEstadoDeLaVentaAsync(id, dto);
            if (estado == null)
            {
                return NotFound();
            }
            return NoContent();
        }
        
        [HttpDelete("Delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _estadoDeLaVentaService.DeleteEstadoDeLaVentaAsync(id);
            if (!result)
            {
                return NotFound();
            }
            return NoContent();
        }
    }
}