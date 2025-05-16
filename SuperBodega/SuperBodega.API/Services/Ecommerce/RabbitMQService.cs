using System.Text;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Newtonsoft.Json;
using RabbitMQ.Client;
using SuperBodega.API.Models.Ecommerce;
using SuperBodega.API.Repositories.Interfaces.Ecommerce;

namespace SuperBodega.API.Services.Ecommerce;

public class RabbitMQService : IRabbitMQService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<RabbitMQService> _logger;
    private readonly ConnectionFactory _connectionFactory;
    private readonly string _queueName;

    public RabbitMQService(IConfiguration configuration, ILogger<RabbitMQService> logger)
    {
        _configuration = configuration;
        _queueName = _configuration["RABBITMQ_QUEUE_NAME"];
        _logger = logger;

        _connectionFactory = new ConnectionFactory
        {
            HostName = _configuration["RABBITMQ_HOST"],
            UserName = _configuration["RABBITMQ_USER"],
            Password = _configuration["RABBITMQ_PASSWORD"],
            Port = 5672,
            VirtualHost = "/",
            AutomaticRecoveryEnabled = true,
            NetworkRecoveryInterval = TimeSpan.FromSeconds(10),
            RequestedHeartbeat = TimeSpan.FromSeconds(30)
        };

        _logger.LogInformation("RabbitMQService inicializado. Cola: {QueueName}", _queueName);
    }

    public async void EnviarNotificacionEmail(NotificacionEmail notificacionEmail)
    {
        try
        {
            _logger.LogInformation("Enviando notificación por email para pedido {Factura} con ID {NotificacionId}", 
            notificacionEmail.NumeroDeFactura, notificacionEmail.IdNotificacion);
            
            // Verificar si la notificación ya incluye todos los datos necesarios
            if (string.IsNullOrEmpty(notificacionEmail.Para) || 
                notificacionEmail.Productos == null || 
                !notificacionEmail.Productos.Any())
            {
                _logger.LogWarning("Se intentó enviar una notificación incompleta. Abortando envío.");
                return;
            }

            using var connection = await _connectionFactory.CreateConnectionAsync();
            using var channel = await connection.CreateChannelAsync();

            await channel.QueueDeclareAsync(
                queue: _queueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null
            );

            var mensaje = JsonConvert.SerializeObject(notificacionEmail);
            var body = Encoding.UTF8.GetBytes(mensaje);

            var properties = channel.BasicPublishAsync(
                exchange: "",
                routingKey: _queueName,
                body: body
            );

            await properties;

            _logger.LogInformation("Mensaje enviado a cola {QueueName} para {Recipient}", 
                        _queueName, notificacionEmail.Para);

            Console.WriteLine($"Mensaje enviado a la cola de RabbitMQ: {mensaje}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al enviar notificación a RabbitMQ");
            Console.WriteLine($"Error al enviar el mensaje a RabbitMQ: {ex.Message}");
        }
    }
}