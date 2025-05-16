using System.Text;
using Microsoft.EntityFrameworkCore.Metadata;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using SuperBodega.API.Models.Ecommerce;
using SuperBodega.API.Repositories.Interfaces.Ecommerce;

namespace SuperBodega.API.Services.Ecommerce;

public class NotificacionWorkerService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<NotificacionWorkerService> _logger;
    private readonly IConfiguration _configuration;
    private ConnectionFactory _connectionFactory;
    private IConnection _connection;
    private IChannel _channel;
    private readonly string _queueName;

    public NotificacionWorkerService
    (
        IServiceProvider serviceProvider,
        ILogger<NotificacionWorkerService> logger,
        IConfiguration configuration
    )
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _configuration = configuration;
        _queueName = _configuration["RABBITMQ_QUEUE_NAME"];
        _logger.LogInformation("NotificacionWorkerService inicializado. Cola: {QueueName}", _queueName);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        stoppingToken.ThrowIfCancellationRequested();

        await Task.Delay(TimeSpan.FromSeconds(15), stoppingToken);

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

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                if (_connection == null || !_connection.IsOpen)
                {
                    _connection = await _connectionFactory.CreateConnectionAsync();
                    _logger.LogInformation("Conexión a RabbitMQ establecida.");
                }

                if (_channel == null || !_channel.IsOpen)
                {
                    _logger.LogInformation("Creando canal de RabbitMQ");
                    _channel = await _connection.CreateChannelAsync();

                    _logger.LogInformation($"Declarando la cola: {_queueName}");

                    await _channel.QueueDeclareAsync(
                        queue: _queueName,
                        durable: true,
                        exclusive: false,
                        autoDelete: false,
                        arguments: null
                    );

                    _channel.BasicQosAsync(prefetchSize: 0, prefetchCount: 1, global: false).Wait();                    
                    _logger.LogInformation("NotificacionWorkerService esta iniciado y escuchando mensajes.");
                    var consumer = new AsyncEventingBasicConsumer(_channel);
                    consumer.ReceivedAsync += async (model, ea) =>
                    {
                        var body = ea.Body.ToArray();
                        var message = Encoding.UTF8.GetString(body);

                        try
                        {
                            _logger.LogInformation($"Mensaje recibido: {message}");
                            var notificacion = JsonConvert.DeserializeObject<NotificacionEmail>(message);

                            if (notificacion != null)
                            {
                                await ProcesarNotificacion(notificacion);
                                await _channel.BasicAckAsync(deliveryTag: ea.DeliveryTag, multiple: false);
                                _logger.LogInformation("Notificacion procesada y confirmada con exito.");
                            }
                            else
                            {
                                _logger.LogError("Formato de notificacion invalido");
                                await _channel.BasicNackAsync(deliveryTag: ea.DeliveryTag, multiple: false, requeue: false);
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError($"Error al procesar la notificacion: {ex.Message}");
                            await _channel.BasicNackAsync(deliveryTag: ea.DeliveryTag, multiple: false, requeue: false);
                        }
                    };

                    await _channel.BasicConsumeAsync(
                        queue: _queueName,
                        autoAck: false,
                        consumer: consumer
                    );

                    while (!stoppingToken.IsCancellationRequested && _connection.IsOpen && _channel.IsOpen)
                    {
                        await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
                    }
                }
            }
            catch (Exception ex) when (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogError($"Error en el worker de notificaciones: {ex.Message}, reintentando en 10 segundos.");
                _channel?.Dispose();
                _channel = null;
                _connection?.Dispose();
                _connection = null;
                await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error inesperado en el worker de notificaciones: {ex.Message}");
                throw;
            }
            finally
            {
                _channel?.CloseAsync();
                _connection?.CloseAsync();
            }
        }
    }

    public async Task ProcesarNotificacion(NotificacionEmail notificacionEmail)
    {
        using (var scope = _serviceProvider.CreateScope())
        {
            var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
            var result = await emailService.EnviarEmailAsync(notificacionEmail);
            if (result)
            {
                _logger.LogInformation($"Email enviado a {notificacionEmail.Para} con éxito.");
            }
            else
            {
                _logger.LogError($"Error al enviar el email a {notificacionEmail.Para}.");
            }
        }
    }

    public override void Dispose()
    {
        _connection?.CloseAsync();
        _connection?.Dispose();
        base.Dispose();
    }
}