using Microsoft.AspNetCore.Mvc;
using SuperBodega.API.Services.Ecommerce;
using SuperBodega.API.DTOs.Ecommerce;

namespace SuperBodega.API.Controllers.Ecommerce
{
    [ApiController]
    [Route("api/[controller]")]
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

        [HttpGet("cliente/{clienteId}")]
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

        [HttpPost("enviar/{ventaId}")]
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

        [HttpPost("marcar-leida/{id}")]
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