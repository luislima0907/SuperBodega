using Microsoft.AspNetCore.Mvc;
using SuperBodega.API.DTOs.Admin;
using SuperBodega.API.Services.Admin;

namespace SuperBodega.API.Controllers.Admin
{
    /// <summary>
    /// Controlador para gestionar los estados de la venta en el sistema
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Produces("application/json")]
    [ApiConventionType(typeof(DefaultApiConventions))]
    public class EstadoDeLaVentaController : ControllerBase
    {
        private readonly EstadoDeLaVentaService _estadoDeLaVentaService;

        /// <summary>
        /// Constructor del controlador del estado de la venta
        /// </summary>
        /// <param name="estadoDeLaVentaService">Servicio para gestionar los estados de la venta</param>
        /// <remarks>
        /// Este controlador proporciona acceso a los estados de la venta
        /// para los administradores y clientes.
        /// </remarks>
        public EstadoDeLaVentaController(EstadoDeLaVentaService estadoDeLaVentaService)
        {
            _estadoDeLaVentaService = estadoDeLaVentaService;
        }

        /// <summary>
        /// Obtiene todos los estados de la venta registrados en el sistema
        /// </summary>
        /// <returns>Lista de todos los estados de la venta</returns>
        /// <response code="200">Retorna la lista de estados de la venta</response>
        /// <response code="500">Error interno del servidor</response>
        /// <response code="404">No se encontraron estados de la venta</response>
        // GET: api/EstadoDeLaVenta
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<EstadoDeLaVentaDTO>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<EstadoDeLaVentaDTO>>> GetAll()
        {
            var estados = await _estadoDeLaVentaService.GetAllEstadosDeLaVentaAsync();
            return Ok(estados);
        }

        /// <summary>
        /// Obtiene un estado de la venta por su ID
        /// </summary>
        /// <param name="id">ID del estado de la venta a buscar</param>
        /// <returns>Datos del estado de la venta solicitado</returns>
        /// <response code="200">Devuelve el estado de la venta solicitado</response>
        /// <response code="404">Si el estado de la venta no existe</response>
        /// <response code="500">Error interno del servidor</response>
        // GET: api/EstadoDeLaVenta/5
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(EstadoDeLaVentaDTO))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<EstadoDeLaVentaDTO>> Get(int id)
        {
            var estado = await _estadoDeLaVentaService.GetEstadoDeLaVentaByIdAsync(id);
            if (estado == null)
            {
                return NotFound();
            }
            return Ok(estado);
        }

        /// <summary>
        /// Crea un nuevo estado de la venta en el sistema
        /// </summary>
        /// <param name="dto">Datos del nuevo estado de la venta</param>
        /// <returns>Datos del estado de la venta creado</returns>
        /// <response code="201">El estado de la venta fue creado exitosamente</response>
        /// <response code="400">Si los datos del estado de la venta son inválidos</response>
        /// <response code="500">Error interno del servidor</response>
        // POST: api/EstadoDeLaVenta/Create
        [HttpPost("Create")]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(EstadoDeLaVentaDTO))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<EstadoDeLaVentaDTO>> Create(EstadoDeLaVentaDTO dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var estado = await _estadoDeLaVentaService.CreateEstadoDeLaVentaAsync(dto);
            return CreatedAtAction(nameof(Get), new { id = estado.Id }, estado);
        }

        /// <summary>
        /// Actualiza un estado de la venta existente en el sistema
        /// </summary>
        /// <param name="id">ID del estado de la venta a actualizar</param>
        /// <param name="dto">Datos actualizados del estado de la venta</param>
        /// <returns>El estado de la venta actualizado</returns>
        /// <response code="200">El estado de la venta fue actualizado exitosamente</response>
        /// <response code="400">Si los datos del estado de la venta son inválidos</response>
        /// <response code="404">Si el estado de la venta no existe</response>
        /// <response code="500">Error interno del servidor</response>
        // PUT: api/EstadoDeLaVenta/Edit/5
        [HttpPut("Edit/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(EstadoDeLaVentaDTO))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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

        /// <summary>
        /// Elimina un estado de la venta del sistema
        /// </summary>
        /// <param name="id">ID del estado de la venta a eliminar</param>
        /// <returns>Sin contenido</returns>
        /// <response code="204">El estado de la venta fue eliminado exitosamente</response>
        /// <response code="404">Si el estado de la venta no existe</response>
        /// <response code="500">Error interno del servidor</response>
        // DELETE: api/EstadoDeLaVenta/Delete/5
        [HttpDelete("Delete/{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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