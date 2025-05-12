using Microsoft.AspNetCore.Mvc;
using SuperBodega.API.DTOs.Admin;
using SuperBodega.API.Services.Admin;

namespace SuperBodega.API.Controllers.Admin;

[Route("Compras")]
public class CompraViewController : Controller
{
    private readonly CompraService _compraService;
    private readonly ProveedorService _proveedorService;
    private readonly ProductoService _productoService;

    public CompraViewController
    (
        CompraService compraService, 
        ProveedorService proveedorService,
        ProductoService productoService
        )
    {
        _compraService = compraService;
        _proveedorService = proveedorService;
        _productoService = productoService;
    }

    [HttpGet("Index")]
    public async Task<IActionResult> Index()
    {
        var compras = await _compraService.GetAllWithDetailsAsync();
        return View(compras);
    }

    [HttpGet("Details/{id}")]
    public async Task<IActionResult> Details(int id)
    {
        var compra = await _compraService.GetWithDetailsAsync(id);
        if (compra == null)
        {
            return NotFound();
        }
        return View(compra);
    }

    [HttpGet("Create")]
    public async Task<IActionResult> Create()
    {
        ViewBag.Proveedores = await _proveedorService.GetAllAsync();
        return View();
    }

        // [HttpPost]
        // [ValidateAntiForgeryToken]
        // public async Task<IActionResult> Create(CreateCompraDTO compraDto)
        // {
        //     if (!ModelState.IsValid)
        //     {
        //         ViewBag.Proveedores = await _proveedorService.GetAllProveedoresAsync();
        //         return View(compraDto);
        //     }
        //
        //     try
        //     {
        //         await _compraService.CreateAsync(compraDto);
        //         return RedirectToAction(nameof(Index));
        //     }
        //     catch (Exception ex)
        //     {
        //         ModelState.AddModelError("", $"Error al crear la compra: {ex.Message}");
        //         ViewBag.Proveedores = await _proveedorService.GetAllProveedoresAsync();
        //         return View(compraDto);
        //     }
        // }

    [HttpGet("Edit/{id}")]
    public async Task<IActionResult> Edit(int id)
    {
        try
        {
            var compra = await _compraService.GetWithDetailsAsync(id);
            if (compra == null)
            {
                TempData["Error"] = $"No se encontró la compra con ID {id}";
                return RedirectToAction(nameof(Index));
            }

            // Obtener lista de proveedores para el dropdown, incluyendo su estado
            var proveedores = await _proveedorService.GetAllAsync();
                
            // Verificar si el proveedor actual está inactivo
            var proveedorActual = proveedores.FirstOrDefault(p => p.Id == compra.IdProveedor);
            if (proveedorActual != null && !proveedorActual.Estado)
            {
                ViewBag.ProveedorInactivo = true;
                TempData["Warning"] = "El proveedor de esta compra está inactivo. Se recomienda seleccionar un proveedor activo.";
            }

            ViewBag.Proveedores = proveedores;

            return View(compra);
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Error al cargar la compra: {ex.Message}";
            return RedirectToAction(nameof(Index));
        }
    }

        // [HttpPost]
        // [ValidateAntiForgeryToken]
        // public async Task<IActionResult> Edit(int id, UpdateCompraDTO compraDto)
        // {
        //     if (!ModelState.IsValid)
        //     {
        //         ViewBag.CompraId = id;
        //         ViewBag.Proveedores = await _proveedorService.GetAllProveedoresAsync();
        //         return View(compraDto);
        //     }
        //
        //     try
        //     {
        //         await _compraService.UpdateAsync(id, compraDto);
        //         return RedirectToAction(nameof(Details), new { id });
        //     }
        //     catch (Exception ex)
        //     {
        //         ModelState.AddModelError("", $"Error al actualizar la compra: {ex.Message}");
        //         ViewBag.CompraId = id;
        //         ViewBag.Proveedores = await _proveedorService.GetAllProveedoresAsync();
        //         return View(compraDto);
        //     }
        // }
}