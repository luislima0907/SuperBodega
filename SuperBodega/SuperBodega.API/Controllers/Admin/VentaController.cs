using Microsoft.AspNetCore.Mvc;
using SuperBodega.API.DTOs.Admin;
using SuperBodega.API.Repositories.Interfaces.Admin;
using SuperBodega.API.Services.Admin;
using SuperBodega.API.Services.Ecommerce;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SuperBodega.API.Controllers.Admin
{
    [Route("api/[controller]")]
    [ApiController]
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

        // POST: api/Venta/Create
        [HttpPost("Create")]
        public async Task<ActionResult<VentaDTO>> CreateVenta([FromBody] CreateVentaDTO createVentaDto)
        {
            try {
                // Add validation if needed
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

        // GET: api/Venta
        [HttpGet]
        public async Task<ActionResult<IEnumerable<VentaDTO>>> GetAllVentas()
        {
            var ventas = await _ventaService.GetAllVentasWithDetailsAsync();
            return Ok(ventas);
        }

        // GET: api/Venta/{id}
        [HttpGet("{id}")]
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

        // GET: api/Venta/cliente/{clienteId}
        [HttpGet("cliente/{clienteId}")]
        public async Task<ActionResult<IEnumerable<VentaDTO>>> GetVentasByCliente(int clienteId)
        {
            var ventas = await _ventaService.GetVentasByClienteIdAsync(clienteId);
            return Ok(ventas);
        }

        // GET: api/Venta/estado/{estadoId}
        [HttpGet("estado/{estadoId}")]
        public async Task<ActionResult<IEnumerable<VentaDTO>>> GetVentasByEstado(int estadoId)
        {
            var ventas = await _ventaService.GetVentasByEstadoIdAsync(estadoId);
            return Ok(ventas);
        }

        // PUT: api/Venta/estado/edit/{id}
        [HttpPut("estado/edit/{id}")]
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

                // ELIMINAR esta llamada redundante
                // await _notificacionService.EnviarNotificacionCambioEstadoAsync(id, usarModoSincrono);

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

        [HttpPost("devolucion/{id}")]
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