using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SuperBodega.API.Data;
using SuperBodega.API.Models.Admin;
using SuperBodega.API.Models.Ecommerce;
using SuperBodega.API.Repositories.Interfaces.Ecommerce;
using SuperBodega.API.Services.Ecommerce;
using Xunit.Abstractions;

namespace TestxUnitSuperBodega.TestPruebaSyncAndAsync
{
    public class NotificationTest : IClassFixture<TestDatabaseFixture>
    {
        private readonly TestDatabaseFixture _fixture;
        private readonly NotificacionService _notificacionService;
        private readonly SuperBodegaContext _context;
        private readonly ITestOutputHelper _output;

        public NotificationTest(TestDatabaseFixture fixture, ITestOutputHelper output)
        {
            _fixture = fixture;
            _context = _fixture.CreateContext();
            var serviceProvider = _fixture.ServiceProvider;
            _notificacionService = serviceProvider.GetRequiredService<NotificacionService>();
            _output = output;
        }

        [Fact]
        public async Task NotificacionSincrona_EnviaCorreo_Inmediatamente()
        {
            // Arrange
            // Crear una venta de prueba
            var venta = await PrepararVentaPrueba();
            bool usarModoSincrono = true;
            
            // Act
            var stopwatch = Stopwatch.StartNew();
            await _notificacionService.EnviarNotificacionCambioEstadoAsync(venta.Id, usarModoSincrono);
            stopwatch.Stop();
            
            // Assert
            _output.WriteLine($"Notificación sincrónica tomó: {stopwatch.ElapsedMilliseconds}ms");
            
            // La notificación sincrónica debería tomar más tiempo al enviar directamente
            Assert.True(stopwatch.ElapsedMilliseconds > 50);
            
            // Verificar que se creó la notificación interna
            var notificacion = await _context.Notificaciones.FirstOrDefaultAsync(n => n.IdVenta == venta.Id);
            Assert.NotNull(notificacion);
        }
        
        [Fact]
        public async Task NotificacionAsincrona_EncolaCorreo_Rapidamente()
        {
            // Arrange
            var venta = await PrepararVentaPrueba();
            bool usarModoSincrono = false;
            
            // Act
            var stopwatch = Stopwatch.StartNew();
            await _notificacionService.EnviarNotificacionCambioEstadoAsync(venta.Id, usarModoSincrono);
            stopwatch.Stop();
            
            // Assert
            _output.WriteLine($"Notificación asincrónica tomó: {stopwatch.ElapsedMilliseconds}ms");
            
            // La notificación asincrónica debería ser más rápida al solo encolar
            Assert.True(stopwatch.ElapsedMilliseconds < 50 || 
                        stopwatch.ElapsedMilliseconds < stopwatch.ElapsedMilliseconds * 0.5);
            
            // Verificar que se creó la notificación interna
            var notificacion = await _context.Notificaciones.FirstOrDefaultAsync(n => n.IdVenta == venta.Id);
            Assert.NotNull(notificacion);
        }
        
        private async Task<Venta> PrepararVentaPrueba()
        {
            // Crear cliente de prueba
            var cliente = new Cliente
            {
                Nombre = "Cliente",
                Apellido = "Prueba",
                Email = "test@example.com",
                Telefono = "12345678",
                Direccion = "Dirección de prueba",
                Estado = true,
                FechaDeRegistro = DateTime.Now
            };
            _context.Clientes.Add(cliente);
            await _context.SaveChangesAsync();
            
            // Crear venta de prueba
            var venta = new Venta
            {
                NumeroDeFactura = $"TEST-{Guid.NewGuid().ToString().Substring(0, 6)}",
                IdCliente = cliente.Id,
                IdEstadoDeLaVenta = 1, // Recibida
                MontoTotal = 100,
                MontoDePago = 100,
                MontoDeCambio = 0,
                FechaDeRegistro = DateTime.Now,
            };
            
            _context.Ventas.Add(venta);
            await _context.SaveChangesAsync();
            
            return venta;
        }
    }
    
    // Fixture para configurar la base de datos de prueba
    public class TestDatabaseFixture
    {
        public ServiceProvider ServiceProvider { get; }
        
        public TestDatabaseFixture()
        {
            var serviceCollection = new ServiceCollection();
            
            // Configurar servicios para pruebas
            serviceCollection.AddDbContext<SuperBodegaContext>(options =>
                options.UseInMemoryDatabase("TestDatabase"));
                
            // Agregar los servicios necesarios
            serviceCollection.AddLogging(builder => builder.AddConsole());
            serviceCollection.AddScoped<IRabbitMQService, MockRabbitMQService>();
            serviceCollection.AddScoped<IEmailService, MockEmailService>();
            serviceCollection.AddScoped<NotificacionService>();
            
            ServiceProvider = serviceCollection.BuildServiceProvider();
            
            // Inicializar la base de datos
            using var scope = ServiceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<SuperBodegaContext>();
            InitializeDatabase(context);
        }
        
        public SuperBodegaContext CreateContext()
        {
            return ServiceProvider.GetRequiredService<SuperBodegaContext>();
        }
        
        private void InitializeDatabase(SuperBodegaContext context)
        {
            context.Database.EnsureCreated();
            
            // Seed estados de venta
            if (!context.EstadosDeLaVenta.Any())
            {
                context.EstadosDeLaVenta.AddRange(
                    new EstadoDeLaVenta { Id = 1, Nombre = "Recibida" },
                    new EstadoDeLaVenta { Id = 2, Nombre = "Despachada" },
                    new EstadoDeLaVenta { Id = 3, Nombre = "Entregada" },
                    new EstadoDeLaVenta { Id = 4, Nombre = "Devolución Solicitada" },
                    new EstadoDeLaVenta { Id = 5, Nombre = "Devolución Completada" }
                );
                context.SaveChanges();
            }
        }
    }
    
    // Mocks para pruebas
    public class MockRabbitMQService : IRabbitMQService
    {
        public void EnviarNotificacionEmail(NotificacionEmail notificacionEmail)
        {
            // Simulación de envío a RabbitMQ - no hace nada
            Console.WriteLine($"MOCK: Mensaje enviado a RabbitMQ para {notificacionEmail.Para}");
        }
    }
    
    public class MockEmailService : IEmailService
    {
        public Task<bool> EnviarEmailAsync(NotificacionEmail notificacion)
        {
            // Simular tiempo de envío de email (100ms)
            Thread.Sleep(100);
            Console.WriteLine($"MOCK: Email enviado a {notificacion.Para}");
            return Task.FromResult(true);
        }
        
        public Task<bool> SendEmailAsync(string to, string subject, string body)
        {
            // Simular envío de email
            Console.WriteLine($"MOCK: Email enviado a {to} con asunto '{subject}'");
            return Task.FromResult(true);
        }
    }
}