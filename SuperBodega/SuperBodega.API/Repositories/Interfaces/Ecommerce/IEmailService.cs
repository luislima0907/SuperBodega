using SuperBodega.API.Models.Ecommerce;
using System.Threading.Tasks;

namespace SuperBodega.API.Repositories.Interfaces.Ecommerce;

public interface IEmailService
{
    Task<bool> SendEmailAsync(string to, string subject, string body);
    Task<bool> EnviarEmailAsync(NotificacionEmail notificacion);
}