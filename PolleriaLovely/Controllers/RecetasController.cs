using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PolleriaLovely.Data;
using PolleriaLovely.Models;
using PolleriaLovely.ViewModels;
using System.Globalization;

namespace PolleriaLovely.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class RecetasController : Controller
    {
        private readonly BibliotecaDbContext _context;

        public RecetasController(BibliotecaDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Editar(int? id)
        {
            if (id == null) return NotFound();

            var producto = await _context.Productos.AsNoTracking().FirstOrDefaultAsync(p => p.IdProducto == id);
            if (producto == null) return NotFound();

            return View(await CrearModelo(producto));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Agregar(AgregarRecetaLineaViewModel modelo)
        {
            var producto = await _context.Productos.FirstOrDefaultAsync(p => p.IdProducto == modelo.IdProducto);
            if (producto == null) return NotFound();

            var cantidad = ParsearCantidad(modelo.Cantidad);
            var insumo = await _context.Insumos.FirstOrDefaultAsync(i => i.IdInsumo == modelo.IdInsumo && i.Estado);
            if (insumo == null)
                ModelState.AddModelError(nameof(modelo.IdInsumo), "Seleccione un insumo activo.");
            if (cantidad == null || cantidad <= 0m)
                ModelState.AddModelError(nameof(modelo.Cantidad), "Ingrese una cantidad mayor que cero.");
            if (cantidad > 1000000m)
                ModelState.AddModelError(nameof(modelo.Cantidad), "La cantidad no puede superar 1,000,000.");

            if (ModelState.IsValid && insumo != null && cantidad.HasValue)
            {
                var existe = await _context.ProductoInsumos
                    .AnyAsync(pi => pi.IdProducto == modelo.IdProducto && pi.IdInsumo == modelo.IdInsumo);
                if (existe)
                {
                    TempData["Error"] = "Ese insumo ya está incluido en la receta. Edite la receta eliminando la línea anterior y vuelva a agregarla.";
                }
                else
                {
                    _context.ProductoInsumos.Add(new ProductoInsumo
                    {
                        IdProducto = modelo.IdProducto,
                        IdInsumo = modelo.IdInsumo,
                        Cantidad = cantidad.Value
                    });
                    await _context.SaveChangesAsync();
                    TempData["Mensaje"] = $"Se agregó «{insumo.NombreInsumo}» a la receta de «{producto.NombreProducto}».";
                }
                return RedirectToAction(nameof(Editar), new { id = modelo.IdProducto });
            }

            TempData["Error"] = string.Join(" ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
            return RedirectToAction(nameof(Editar), new { id = modelo.IdProducto });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Eliminar(int id, int idProducto)
        {
            var linea = await _context.ProductoInsumos.FirstOrDefaultAsync(pi => pi.IdProductoInsumo == id && pi.IdProducto == idProducto);
            if (linea != null)
            {
                _context.ProductoInsumos.Remove(linea);
                await _context.SaveChangesAsync();
                TempData["Mensaje"] = "El insumo se retiró de la receta.";
            }
            return RedirectToAction(nameof(Editar), new { id = idProducto });
        }

        private async Task<RecetaProductoViewModel> CrearModelo(Producto producto)
        {
            var lineas = await _context.ProductoInsumos
                .AsNoTracking()
                .Where(pi => pi.IdProducto == producto.IdProducto)
                .Include(pi => pi.Insumo)
                .OrderBy(pi => pi.Insumo.NombreInsumo)
                .Select(pi => new RecetaLineaViewModel
                {
                    IdProductoInsumo = pi.IdProductoInsumo,
                    NombreInsumo = pi.Insumo.NombreInsumo,
                    UnidadMedida = pi.Insumo.UnidadMedida,
                    Cantidad = pi.Cantidad
                })
                .ToListAsync();

            var insumos = await _context.Insumos
                .AsNoTracking()
                .Where(i => i.Estado)
                .OrderBy(i => i.NombreInsumo)
                .ToListAsync();

            return new RecetaProductoViewModel
            {
                IdProducto = producto.IdProducto,
                NombreProducto = producto.NombreProducto,
                Lineas = lineas,
                InsumosDisponibles = insumos.Select(i => new SelectListItem(i.NombreInsumo, i.IdInsumo.ToString())).ToList()
            };
        }

        private static decimal? ParsearCantidad(string? texto)
        {
            if (string.IsNullOrWhiteSpace(texto)) return null;
            var valor = texto.Trim().Replace(" ", "");
            var coma = valor.LastIndexOf(',');
            var punto = valor.LastIndexOf('.');
            if (coma >= 0 && punto >= 0)
                valor = coma > punto ? valor.Replace(".", "").Replace(',', '.') : valor.Replace(",", "");
            else if (coma >= 0)
                valor = valor.Replace(',', '.');
            return decimal.TryParse(valor, NumberStyles.Number, CultureInfo.InvariantCulture, out var cantidad)
                ? cantidad
                : null;
        }
    }
}
