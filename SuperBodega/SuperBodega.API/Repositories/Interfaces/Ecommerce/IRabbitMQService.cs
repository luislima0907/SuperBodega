using SuperBodega.API.Models.Ecommerce;

namespace SuperBodega.API.Repositories.Interfaces.Ecommerce;

public interface IRabbitMQService
{
    void EnviarNotificacionEmail(NotificacionEmail notificacionEmail);
}