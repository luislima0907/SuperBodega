using Microsoft.AspNetCore.Mvc;
using SuperBodega.API.Services.Admin;

namespace SuperBodega.API.Controllers.Admin;

/// <summary>
/// Controlador para las vistas relacionadas con la gestión de clientes.
/// </summary>
/// <remarks>
/// Proporciona acceso a las páginas web para administrar clientes,
/// como listado, creación, edición y visualización de detalles.
/// </remarks>
[Route("Clientes")]
public class ClienteViewController : Controller
{
    private readonly ClienteService _clienteService;
    
    /// <summary>
    /// Inicializa una nueva instancia del controlador de vistas de clientes.
    /// </summary>
    /// <param name="clienteService">Servicio de clientes a utilizar</param>
    public ClienteViewController(ClienteService clienteService)
    {
        _clienteService = clienteService;
    }
    
    /// <summary>
    /// Muestra la página de listado de clientes.
    /// </summary>
    /// <returns>Vista de listado de clientes</returns>
    /// <remarks>
    /// Esta vista carga los datos dinámicamente mediante JavaScript.
    /// </remarks>
    [HttpGet("Index")]
    public IActionResult Index()
    {
        return View();
    }
    
    /// <summary>
    /// Muestra la página para crear un nuevo cliente.
    /// </summary>
    /// <returns>Vista de creación de cliente</returns>
    [HttpGet("Create")]
    public IActionResult Create()
    {
        return View();
    }

    /// <summary>
    /// Muestra la página para editar un cliente existente.
    /// </summary>
    /// <param name="id">ID del cliente a editar</param>
    /// <returns>Vista de edición de cliente</returns>
    /// <remarks>
    /// Si el cliente no existe, retorna NotFound (404).
    /// </remarks>
    [HttpGet("Edit/{id}")]
    public async Task<IActionResult> Edit(int id)
    {
        var cliente = await _clienteService.GetClienteByIdAsync(id);
        if (cliente == null)
        {
            return NotFound();
        }
        return View(cliente);
    }
}