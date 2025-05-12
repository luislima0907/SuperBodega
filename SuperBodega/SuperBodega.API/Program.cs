using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Mvc.Razor;
using SuperBodega.API.Data;
using SuperBodega.API.Services;
using System.Net.Mime;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Razor.RuntimeCompilation;
using Microsoft.Extensions.FileProviders;
using SuperBodega.API.Models.Admin;
using SuperBodega.API.Repositories.Implementations.Admin;
using SuperBodega.API.Services.Admin;
using SuperBodega.API.Repositories.Interfaces.Admin;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(8080);
});

// Configuración básica de MVC y Razor
builder.Services.AddControllersWithViews();

// Configurar el motor de vistas Razor
builder.Services.Configure<MvcRazorRuntimeCompilationOptions>(options =>
{
    var viewsPath = Path.Combine(AppContext.BaseDirectory, "Views");
    if (!Directory.Exists(viewsPath))
    {
        viewsPath = Path.Combine(Directory.GetCurrentDirectory(), "Views");
    }
    options.FileProviders.Clear();
    options.FileProviders.Add(new PhysicalFileProvider(viewsPath));
});

// Configurar las rutas de vistas
builder.Services.Configure<RazorViewEngineOptions>(options =>
{
    options.ViewLocationFormats.Clear();
    options.ViewLocationFormats.Add("/Views/{1}/{0}" + RazorViewEngine.ViewExtension);
    options.ViewLocationFormats.Add("/Views/Shared/{0}" + RazorViewEngine.ViewExtension);
    options.ViewLocationFormats.Add("/Views/Dashboard/{0}" + RazorViewEngine.ViewExtension);
    options.ViewLocationFormats.Add("/Views/CategoriaView/{0}" + RazorViewEngine.ViewExtension);
    options.ViewLocationFormats.Add("/Views/ProductoView/{0}" + RazorViewEngine.ViewExtension);
    options.ViewLocationFormats.Add("/Views/ProveedorView/{0}" + RazorViewEngine.ViewExtension);
    options.ViewLocationFormats.Add("/Views/ClienteView/{0}" + RazorViewEngine.ViewExtension);
    options.ViewLocationFormats.Add("/Views/CompraView/{0}" + RazorViewEngine.ViewExtension);
    options.ViewLocationFormats.Add("/Views/ProductoCatalogoView/{0}" + RazorViewEngine.ViewExtension);
    options.ViewLocationFormats.Add("/Views/CarritoView/{0}" + RazorViewEngine.ViewExtension);
    options.ViewLocationFormats.Add("/Views/VentaView/{0}" + RazorViewEngine.ViewExtension);
    options.ViewLocationFormats.Add("/Views/RealizarCompraView/{0}" + RazorViewEngine.ViewExtension);
    options.ViewLocationFormats.Add("/Views/MisPedidosView/{0}" + RazorViewEngine.ViewExtension);
    options.ViewLocationFormats.Add("/Views/NotificacionView/{0}" + RazorViewEngine.ViewExtension);
    options.ViewLocationFormats.Add("/Views/ReporteView/{0}" + RazorViewEngine.ViewExtension);
});

// Configuración de archivos estáticos
builder.Services.Configure<StaticFileOptions>(options =>
{
    options.ServeUnknownFileTypes = true;
});

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var connectionString = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING");
if (string.IsNullOrEmpty(connectionString))
{
    connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
}
Console.WriteLine($"Connection string: {connectionString}");

// Agregar servicio para verificar la conexión a la base de datos
builder.Services.AddHostedService<DatabaseConnectionCheckService>();

// Configurar el contexto de la base de datos con retry policy
builder.Services.AddDbContext<SuperBodegaContext>(options =>
{
    options.UseSqlServer(connectionString, sqlOptions =>
    {
        sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(30),
            errorNumbersToAdd: null);
    });
});



// Registrar el servicio de inicialización de la base de datos
builder.Services.AddScoped<DatabaseInitializerService>();

// implementación del repositorio genérico
builder.Services.AddScoped<IGenericOperationsRepository<Categoria>, CategoriaRepository>();

// Registrar repositorios
builder.Services.AddScoped<IProductoRepository, ProductoRepository>();
builder.Services.AddScoped<IProveedorRepository, ProveedorRepository>();
builder.Services.AddScoped<IClienteRepository, ClienteRepository>();
builder.Services.AddScoped<ICompraRepository, CompraRepository>();

// Registrar servicios
builder.Services.AddScoped<ProductoService>();
builder.Services.AddScoped<ProveedorService>(); 
builder.Services.AddScoped<ClienteService>();
builder.Services.AddScoped<CompraService>();
builder.Services.AddScoped<CategoriaService>();

// HttpClient para Resend
builder.Services.AddHttpClient();

// Agregar CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", 
        builder => builder
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader());
});

// Agregar health checks
builder.Services.AddHealthChecks();

// Agregar controladores
builder.Services.AddControllers();

builder.Services.AddControllersWithViews();

// los middleware
var app = builder.Build();

// Inicializar la base de datos antes de comenzar la aplicación
using (var scope = app.Services.CreateScope())
{
    var dbInitializer = scope.ServiceProvider.GetRequiredService<DatabaseInitializerService>();
    await dbInitializer.InitializeAsync();
}

// Configurar el pipeline HTTP
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Configurar los archivos estáticos antes de los demás middlewares
app.UseRouting();
app.UseStaticFiles();

app.UseCors("AllowAll");

// Configurar el sistema de rutas
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Configure the HTTP request pipeline.
app.UseHttpsRedirection();

app.MapControllers();

// Exponer el endpoint de health check con formato JSON
app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = MediaTypeNames.Application.Json;
        
        var response = new
        {
            Status = report.Status.ToString(),
            HealthChecks = report.Entries.Select(e => new
            {
                Component = e.Key,
                Status = e.Value.Status.ToString(),
                Description = e.Value.Description
            }),
            TotalDuration = report.TotalDuration
        };
        
        await context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }
});

app.Run();