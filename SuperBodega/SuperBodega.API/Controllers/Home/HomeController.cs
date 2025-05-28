using Microsoft.AspNetCore.Mvc;
using SuperBodega.API.Data;

namespace SuperBodega.API.Controllers.Home
{
    /// <summary>
    /// Controlador para la página de inicio de la aplicación.
    /// </summary>
    /// <remarks>
    /// Proporciona acceso a la página de inicio y verifica la conexión a la base de datos.
    /// </remarks>
    [ApiExplorerSettings(GroupName = "Inicio")]
    [Route("/")]
    public class HomeController : Controller
    {
        private readonly SuperBodegaContext _context;
        private readonly ILogger<HomeController> _logger;

        /// <summary>
        /// Inicializa una nueva instancia del controlador de inicio.
        /// </summary>
        /// <param name="context">Contexto de la base de datos a utilizar</param>
        /// <param name="logger">Logger para registrar información y errores</param>
        public HomeController(SuperBodegaContext context, ILogger<HomeController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Muestra la página de inicio.
        /// </summary>
        /// <returns>Vista de inicio con las opciones de ver area del cliente y administracion</returns>
        [HttpGet]
        public IActionResult Index()
        {
            bool dbConnectionSuccess = false;
            string dbConnectionMessage = "No se ha intentado conectar a la base de datos.";
            
            try
            {
                // Intentar una consulta simple para verificar la conexión
                dbConnectionSuccess = _context.Database.CanConnect();
                dbConnectionMessage = dbConnectionSuccess 
                    ? "Conexión a SQL Server establecida exitosamente." 
                    : "No se pudo establecer conexión con SQL Server.";
                
                _logger.LogInformation(dbConnectionMessage);
            }
            catch (Exception ex)
            {
                dbConnectionSuccess = false;
                dbConnectionMessage = $"Error al conectar con la base de datos: {ex.Message}";
                _logger.LogError(ex, "Error al conectar con la base de datos");
            }

            ViewBag.DbConnectionSuccess = dbConnectionSuccess;
            ViewBag.DbConnectionMessage = dbConnectionMessage;
            
            return View();
        }
    }
}