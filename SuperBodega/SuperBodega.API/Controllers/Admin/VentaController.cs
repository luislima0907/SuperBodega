using Microsoft.AspNetCore.Mvc;
using SuperBodega.API.DTOs.Admin;
using SuperBodega.API.Services.Admin;
using SuperBodega.API.Services.Ecommerce;

namespace SuperBodega.API.Controllers.Admin
{
    /// <summary>
    /// Controlador para gestionar ventas en el sistema
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Produces("application/json")]
    [ApiConventionType(typeof(DefaultApiConventions))]
    public class VentaController : ControllerBase
    {
        private readonly VentaService _ventaService;
        private readonly NotificacionService _notificacionService;
        private readonly ILogger<VentaController> _logger;

        public VentaController
        (VentaService ventaService, 
        ILogger<VentaController> logger,
        NotificacionService notificacionService)
        {
            _notificacionService = notificacionService;
            _ventaService = ventaService;
            _logger = logger;
        }

        /// <summary>
        /// Crea una nueva venta en el sistema.
        /// </summary>
        /// <param name="createVentaDto">Datos de la venta a crear.</param>
        /// <returns>Detalles de la venta creada.</returns>
        /// <remarks>
        /// Este método crea una nueva venta y envía una notificación
        /// al cliente sobre el estado de la venta.
        /// </remarks>
        /// <response code="201">Venta creada exitosamente.</response>
        /// <response code="400">Datos de entrada inválidos.</response>
        /// <response code="500">Error interno del servidor.</response>
        // POST: api/Venta/Create
        [HttpPost("Create")]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(VentaDTO))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<VentaDTO>> CreateVenta([FromBody] CreateVentaDTO createVentaDto)
        {
            try {
                var nuevaVenta = await _ventaService.CreateVentaAsync(createVentaDto);
        
                // Usar el modo de notificación del DTO
                bool usarModoSincrono = createVentaDto.UsarNotificacionSincronica;
                
                // Enviar notificación con el modo elegido
                await _notificacionService.EnviarNotificacionCambioEstadoAsync(nuevaVenta.Id, usarModoSincrono);
                
                return CreatedAtAction(nameof(GetVentaById), new { id = nuevaVenta.Id }, nuevaVenta);
            }
            catch (Exception ex) {
                _logger.LogError(ex, "Error al crear una venta: {Message}", ex.Message);
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene todas las ventas registradas en el sistema.
        /// </summary>
        /// <returns>Lista de todas las ventas.</returns>
        /// <remarks>
        /// Este método devuelve una lista de todas las ventas
        /// registradas en el sistema, incluyendo detalles
        /// como el cliente, estado y monto total.
        /// </remarks>
        /// <response code="200">Lista de ventas obtenida exitosamente.</response>
        /// <response code="404">No se encontraron ventas.</response>
        /// <response code="500">Error interno del servidor.</response>
        // GET: api/Venta
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<VentaDTO>))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<VentaDTO>>> GetAllVentas()
        {
            var ventas = await _ventaService.GetAllVentasWithDetailsAsync();
            return Ok(ventas);
        }

        /// <summary>
        /// Obtiene una venta específica por su ID.
        /// </summary>
        /// <param name="id">ID de la venta a buscar.</param>
        /// <returns>Detalles de la venta solicitada.</returns>
        /// <remarks>
        /// Este método devuelve los detalles de una venta específica,
        /// incluyendo información del cliente, estado y monto total.
        /// </remarks>
        /// <response code="200">Venta encontrada exitosamente.</response>
        /// <response code="404">Venta no encontrada.</response>
        /// <response code="500">Error interno del servidor.</response>
        // GET: api/Venta/{id}
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(VentaDTO))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<VentaDTO>> GetVentaById(int id)
        {
            var venta = await _ventaService.GetVentaByIdWithDetailsAsync(id);
            if (venta == null) return NotFound();

            var response = new
            {
                venta.Id,
                venta.NumeroDeFactura,
                venta.IdCliente,
                venta.NombreCompletoCliente,
                venta.EmailCliente,
                venta.IdEstadoDeLaVenta,
                venta.NombreEstadoDeLaVenta,
                venta.FechaDeRegistro,
                venta.MontoTotal,
                venta.MontoDePago,
                venta.MontoDeCambio,
                venta.DetallesDeLaVenta
            };
            return Ok(response);
        }

        /// <summary>
        /// Obtiene todas las ventas de un cliente específico.
        /// </summary>
        /// <param name="clienteId">ID del cliente a buscar.</param>
        /// <returns>Lista de ventas del cliente.</returns>
        /// <remarks>
        /// Este método devuelve una lista de todas las ventas
        /// realizadas por un cliente específico,
        /// incluyendo detalles como el estado y monto total.
        /// </remarks>
        /// <response code="200">Lista de ventas del cliente obtenida exitosamente.</response>
        /// <response code="404">No se encontraron ventas para el cliente.</response>
        /// <response code="500">Error interno del servidor.</response>
        // GET: api/Venta/cliente/{clienteId}
        [HttpGet("cliente/{clienteId}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<VentaDTO>))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<VentaDTO>>> GetVentasByCliente(int clienteId)
        {
            var ventas = await _ventaService.GetVentasByClienteIdAsync(clienteId);
            return Ok(ventas);
        }

        /// <summary>
        /// Obtiene todas las ventas de un estado específico.
        /// </summary>
        /// <param name="estadoId">ID del estado a buscar.</param>
        /// <returns>Lista de ventas del estado.</returns>
        /// <remarks>
        /// Este método devuelve una lista de todas las ventas
        /// realizadas en un estado específico,
        /// incluyendo detalles como el cliente y monto total.
        /// </remarks>
        /// <response code="200">Lista de ventas del estado obtenida exitosamente.</response>
        /// <response code="404">No se encontraron ventas para el estado.</response>
        /// <response code="500">Error interno del servidor.</response>
        // GET: api/Venta/estado/{estadoId}
        [HttpGet("estado/{estadoId}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<VentaDTO>))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<VentaDTO>>> GetVentasByEstado(int estadoId)
        {
            var ventas = await _ventaService.GetVentasByEstadoIdAsync(estadoId);
            return Ok(ventas);
        }

        /// <summary>
        /// Actualiza el estado de una venta específica.
        /// </summary>
        /// <param name="id">ID de la venta a actualizar.</param>
        /// <param name="updateDto">Datos del nuevo estado de la venta.</param>
        /// <returns>Detalles de la venta actualizada.</returns>
        /// <remarks>
        /// Este método actualiza el estado de una venta específica
        /// y envía una notificación al cliente
        /// sobre el cambio de estado.
        /// </remarks>
        /// <response code="200">Estado de la venta actualizado exitosamente.</response>
        /// <response code="404">Venta no encontrada o cambio de estado no permitido.</response>
        /// <response code="500">Error interno del servidor.</response>
        // PUT: api/Venta/estado/edit/{id}
        [HttpPut("estado/edit/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(VentaDTO))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateEstadoDeLaVenta(int id, [FromBody] UpdateEstadoDeLaVentaDTO updateDto)
        {
            try
            {
                // Pasar el modo sincrónico al servicio de ventas DIRECTAMENTE
                var result = await _ventaService.ChangeVentaStateAsync(id, updateDto.IdEstadoDeLaVenta, updateDto.UsarNotificacionSincronica);
                if (!result)
                {
                    return NotFound(new { message = "Venta no encontrada o cambio de estado no permitido" });
                }
                
                return Ok(new { 
                    message = $"Estado actualizado correctamente. Notificación enviada en modo {(updateDto.UsarNotificacionSincronica ? "sincrónico" : "asincrónico")}" 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cambiar estado de venta");
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>
        /// Procesa la devolución de una venta específica.
        /// </summary>
        /// <param name="id">ID de la venta a devolver.</param>
        /// <param name="updateDto">Datos del nuevo estado de la venta.</param>
        /// <returns>Detalles de la venta actualizada.</returns>
        /// <remarks>
        /// Este método procesa la devolución de una venta específica
        /// y envía una notificación al cliente
        /// sobre el cambio de estado.
        /// </remarks>
        /// <response code="200">Devolución procesada exitosamente.</response>
        /// <response code="404">Venta no encontrada o no está en estado de devolución solicitada.</response>
        /// <response code="500">Error interno del servidor.</response>
        [HttpPost("devolucion/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(VentaDTO))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ProcessReturn(int id, [FromBody] UpdateEstadoDeLaVentaDTO updateDto = null)
        {
            try
            {
                // Usar el modo sincrónico si se especifica, por defecto asincrónico
                bool usarModoSincrono = updateDto?.UsarNotificacionSincronica ?? false;
                var result = await _ventaService.ProcessReturnAsync(id, usarModoSincrono);
                
                if (result)
                    return Ok(new { success = true, message = "Devolución procesada correctamente" });
                else
                    return NotFound(new { success = false, message = "No se encontró la venta o no está en estado de devolución solicitada" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { 
                    success = false, 
                    message = "Error al procesar la devolución", 
                    details = ex.Message 
                });
            }
        }
    }
}