using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SuperBodega.API.Models.Ecommerce;
using SuperBodega.API.Repositories.Interfaces.Ecommerce;

namespace SuperBodega.API.Services.Ecommerce
{
    public class EmailService : IEmailService
    {
        private readonly ILogger<EmailService> _logger;
        private readonly string _smtpHost = "smtp.gmail.com";
        private readonly int _smtpPort = 587;
        private readonly string _smtpUsername = "perezlc440@gmail.com";
        private readonly string _smtpPassword = "zvbvkoboaippqhlf";
        private readonly string _fromEmail = "perezlc440@gmail.com";
        private readonly string _fromName = "SuperBodega";

        public EmailService(ILogger<EmailService> logger)
        {
            _logger = logger;
        }

        public async Task<bool> SendEmailAsync(string to, string subject, string body)
        {
            try
            {
                var message = new MailMessage();
                message.From = new MailAddress(_fromEmail, _fromName);
                message.To.Add(new MailAddress(to));
                message.Subject = subject;
                message.Body = body;
                message.IsBodyHtml = true;

                using (var client = new SmtpClient(_smtpHost, _smtpPort))
                {
                    client.EnableSsl = true;
                    client.Credentials = new NetworkCredential(_smtpUsername, _smtpPassword);
                    client.DeliveryMethod = SmtpDeliveryMethod.Network;

                    await client.SendMailAsync(message);
                    _logger.LogInformation($"Email sent successfully to {to}");
                    return true;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error sending email to {to}: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> EnviarEmailAsync(NotificacionEmail notificacion)
        {
            try
            {
                // Generar el cuerpo del email en formato HTML
                var body = GenerarHtmlEmail(notificacion);
                
                // Enviar el email usando el método existente
                return await SendEmailAsync(notificacion.Para, notificacion.Asunto, body);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al generar y enviar email a {notificacion.Para}: {ex.Message}");
                return false;
            }
        }

        private string GenerarHtmlEmail(NotificacionEmail notificacion)
        {
            // Tabla de productos en formato HTML con categoría - ahora con diseño responsive
            var tablaProductos = "<div style='overflow-x: auto;'>" +
                "<table style='width:100%; border-collapse: collapse; margin-top: 20px;'>" +
                "<thead style='background-color: #f3f3f3;'>" +
                "<tr>" +
                "<th style='padding: 12px 8px; text-align: left; border-bottom: 1px solid #ddd;'>Producto</th>" +
                "<th style='padding: 12px 8px; text-align: left; border-bottom: 1px solid #ddd;'>Categoría</th>" +
                "<th style='padding: 12px 8px; text-align: center; border-bottom: 1px solid #ddd;'>Cant</th>" +
                "<th style='padding: 12px 8px; text-align: right; border-bottom: 1px solid #ddd;'>Precio</th>" +
                "<th style='padding: 12px 8px; text-align: right; border-bottom: 1px solid #ddd;'>Pago</th>" +
                "<th style='padding: 12px 8px; text-align: right; border-bottom: 1px solid #ddd;'>Cambio</th>" +
                "<th style='padding: 12px 8px; text-align: right; border-bottom: 1px solid #ddd;'>Subtotal</th>" +
                "</tr>" +
                "</thead>" +
                "<tbody>";

            foreach (var producto in notificacion.Productos)
            {
                string categoriaProducto = producto.CategoriaDelProducto ?? "Sin categoría";

                tablaProductos += $"<tr>" +
                    $"<td style='padding: 12px 8px; border-bottom: 1px solid #eee;'>{producto.NombreDelProducto}</td>" +
                    $"<td style='padding: 12px 8px; border-bottom: 1px solid #eee;'>{categoriaProducto}</td>" +
                    $"<td style='padding: 12px 8px; text-align: center; border-bottom: 1px solid #eee;'>{producto.Cantidad}</td>" +
                    $"<td style='padding: 12px 8px; text-align: right; border-bottom: 1px solid #eee;'>Q {producto.PrecioUnitario:F2}</td>" +
                    $"<td style='padding: 12px 8px; text-align: right; border-bottom: 1px solid #eee;'>Q {producto.MontoDePago:F2}</td>" +
                    $"<td style='padding: 12px 8px; text-align: right; border-bottom: 1px solid #eee;'>Q {producto.MontoDeCambio:F2}</td>" +
                    $"<td style='padding: 12px 8px; text-align: right; border-bottom: 1px solid #eee;'>Q {producto.SubTotal:F2}</td>" +
                    $"</tr>";
            }

            tablaProductos += "<tr>" +
                "<td colspan='6' style='padding: 12px 8px; text-align: right; font-weight: bold; border-top: 2px solid #ddd;'>Total:</td>" +
                $"<td style='padding: 12px 8px; text-align: right; font-weight: bold; border-top: 2px solid #ddd;'>Q {notificacion.MontoTotal:F2}</td>" +
                "</tr>" +
                "</tbody></table></div>";

            TimeZoneInfo guatemalaZone;
            try {
                // Intentar formato Windows
                guatemalaZone = TimeZoneInfo.FindSystemTimeZoneById("Central America Standard Time");
            } 
            catch {
                try {
                    // Intentar formato Linux
                    guatemalaZone = TimeZoneInfo.FindSystemTimeZoneById("America/Guatemala");
                }
                catch {
                    // Crear zona manualmente con UTC-6 para Guatemala
                    guatemalaZone = TimeZoneInfo.CreateCustomTimeZone("Guatemala", new TimeSpan(-6, 0, 0), "Guatemala", "GT");
                }
            }
            
            DateTime localDateTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, guatemalaZone);

            // Determinar color y emoji según estado
            string colorEstado = ObtenerColorEstado(notificacion.EstadoDeLaVenta);
            string iconoEstado = ObtenerIconoEstado(notificacion.EstadoDeLaVenta);

            return $@"
            <!DOCTYPE html>
            <html>
            <head>
                <meta charset='UTF-8'>
                <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                <title>{notificacion.Asunto}</title>
                <style>
                    @media screen and (max-width: 600px) {{
                        .container {{
                            width: 100% !important;
                            padding: 10px !important;
                        }}
                        .header {{
                            padding: 15px 10px !important;
                        }}
                        .content-block {{
                            padding: 15px !important;
                        }}
                        .status-block {{
                            margin: 15px 0 !important;
                        }}
                        table {{
                            font-size: 14px !important;
                        }}
                        h2 {{
                            font-size: 22px !important;
                        }}
                        h3 {{
                            font-size: 18px !important;
                        }}
                    }}
                </style>
            </head>
            <body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333; background-color: #f5f5f5; margin: 0; padding: 0;'>
                <div class='container' style='max-width: 600px; margin: 0 auto; background-color: #ffffff; border-radius: 8px; box-shadow: 0 2px 5px rgba(0,0,0,0.1); overflow: hidden;'>
                    <div class='header' style='background-color: #2c3e50; padding: 25px; text-align: center;'>
                        <h2 style='color: #ffffff; margin: 0;'>SuperBodega</h2>
                    </div>
                    
                    <div class='content-block' style='padding: 25px; background-color: #ffffff;'>
                        <h3 style='margin-top: 0; color: #2c3e50;'>Hola {notificacion.NombreCompletoDelCliente},</h3>
                        
                        <div class='status-block' style='background-color: {colorEstado}; color: white; padding: 15px; border-radius: 5px; margin: 25px 0; text-align: center; box-shadow: 0 2px 4px rgba(0,0,0,0.1);'>
                            <span style='font-size: 28px; margin-right: 10px; display: inline-block;'>{iconoEstado}</span>
                            <span style='font-size: 20px; font-weight: bold; vertical-align: middle;'>{notificacion.EstadoDeLaVenta}</span>
                        </div>
                        
                        <p style='margin-bottom: 25px; font-size: 16px;'>{notificacion.Contenido}</p>
                    </div>
                    
                    <div class='content-block' style='padding: 0 25px 25px; background-color: #ffffff;'>
                        <h3 style='border-bottom: 2px solid #eee; padding-bottom: 12px; color: #2c3e50;'>Detalles de tu pedido #{notificacion.NumeroDeFactura}</h3>
                        
                        <p style='color: #666;'><strong>Fecha:</strong> {localDateTime:dd/MM/yyyy hh:mm tt}</p>
                        
                        {tablaProductos}
                    </div>
                    
                    <div class='content-block' style='background-color: #f9f9f9; padding: 25px; margin-top: 10px; text-align: center; border-top: 1px solid #eee;'>
                        <p style='margin-bottom: 10px; font-weight: bold;'>¿Necesitas ayuda?</p>
                        <p style='margin-top: 0;'>Puedes contactarnos a <a href='mailto:superbodega@example.com' style='color: #3498db; text-decoration: none;'>superbodega@example.com</a></p>
                    </div>
                    
                    <div class='footer' style='padding: 20px; text-align: center; font-size: 12px; color: #777; background-color: #2c3e50;'>
                        <p style='color: #ddd; margin: 5px 0;'>© {DateTime.Now.Year} SuperBodega. Todos los derechos reservados.</p>
                    </div>
                </div>
            </body>
            </html>";
        }

        private string ObtenerIconoEstado(string estado)
        {
            string estadoLower = estado.ToLower().Normalize(System.Text.NormalizationForm.FormD)
                .Replace("\u0301", "").Replace("\u0308", "");
            
            switch (estadoLower)
            {
                case "recibida":
                    return "📥";
                case "despachada":
                    return "🚚";
                case "entregada":
                    return "✅";
                case "devolucion solicitada":
                    return "↩️";
                case "devolucion completada":
                    return "🔄";
                default:
                    return "📦";
            }
        }

        private string ObtenerColorEstado(string estado)
        {
            string estadoLower = estado.ToLower().Normalize(System.Text.NormalizationForm.FormD)
                .Replace("\u0301", "").Replace("\u0308", "");
            
            switch (estadoLower)
            {
                case "recibida":
                    return "#17a2b8"; // Azul info
                case "despachada":
                    return "#ffc107"; // Amarillo warning
                case "entregada":
                    return "#28a745"; // Verde success
                case "devolucion solicitada":
                    return "#dc3545"; // Rojo danger
                case "devolucion completada":
                    return "#6c757d"; // Gris secondary
                default:
                    return "#6c757d"; // Gris por defecto
            }
        }
    }
}