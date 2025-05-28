using Microsoft.AspNetCore.Mvc;
using SuperBodega.API.Services.Ecommerce;
using SuperBodega.API.DTOs.Ecommerce;

namespace SuperBodega.API.Controllers.Ecommerce
{
    /// <summary>
    /// Controlador para gestionar las notificaciones en el sistema
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    [ApiConventionType(typeof(DefaultApiConventions))]
    public class NotificacionController : ControllerBase
    {
        private readonly NotificacionService _notificacionService;
        private readonly ILogger<NotificacionController> _logger;

        public NotificacionController(
            NotificacionService notificacionService,
            ILogger<NotificacionController> logger)
        {
            _notificacionService = notificacionService;
            _logger = logger;
        }

        /// <summary>
        /// Obtiene todas las notificaciones para un cliente específico
        /// </summary>
        /// <param name="clienteId">ID del cliente</param>
        /// <returns>Lista de notificaciones</returns>
        /// <response code="200">Retorna la lista de notificaciones</response>
        /// <response code="404">No se encontraron notificaciones.</response>
        /// <response code="500">Error interno del servidor</response>
        [HttpGet("cliente/{clienteId}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<NotificacionDTO>))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetByCliente(int clienteId)
        {
            try
            {
                _logger.LogInformation("Obteniendo notificaciones para cliente {clienteId}", clienteId);
                var notificaciones = await _notificacionService.GetNotificacionesPorClienteAsync(clienteId);
                
                // Siempre devolver una lista (vacía si no hay notificaciones)
                if (notificaciones == null)
                {
                    return Ok(new List<NotificacionDTO>());
                }
                
                return Ok(notificaciones);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener notificaciones para cliente {clienteId}", clienteId);
                return StatusCode(500, new { message = "Error al obtener notificaciones: " + ex.Message });
            }
        }

        /// <summary>
        /// Envía una notificación de cambio de estado a un cliente
        /// </summary>
        /// <param name="ventaId">ID de la venta</param>
        /// <returns>Resultado de la operación para notificar</returns>
        /// <response code="200">Notificación enviada correctamente</response>
        /// <response code="404">No se encontró la venta.</response>
        /// <response code="500">Error interno del servidor</response>
        [HttpPost("enviar/{ventaId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> EnviarNotificacion(int ventaId)
        {
            try
            {
                await _notificacionService.EnviarNotificacionCambioEstadoAsync(ventaId);
                return Ok(new { message = "Notificación enviada correctamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al enviar notificación para venta {ventaId}", ventaId);
                return StatusCode(500, new { message = "Error al enviar notificación: " + ex.Message });
            }
        }

        /// <summary>
        /// Marca una notificación como leída
        /// </summary>
        /// <param name="id">ID de la notificación</param>
        /// <returns>Resultado de la operación para marcar como leída</returns>
        /// <response code="200">Notificación marcada como leída</response>
        /// <response code="404">No se encontró la notificación.</response>
        /// <response code="500">Error interno del servidor</response>
        [HttpPost("marcar-leida/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> MarcarComoLeida(int id)
        {
            try
            {
                await _notificacionService.MarcarComoLeidaAsync(id);
                return Ok(new { message = "Notificación marcada como leída" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al marcar notificación {id} como leída", id);
                return StatusCode(500, new { message = "Error al marcar notificación como leída: " + ex.Message });
            }
        }
    }
}