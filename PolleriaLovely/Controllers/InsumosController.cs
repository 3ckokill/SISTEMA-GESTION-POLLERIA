using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using PolleriaLovely.Models;
using PolleriaLovely.Data;
using PolleriaLovely.ViewModels;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Globalization;

namespace PolleriaLovely.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class InsumosController : Controller
    {
        private readonly BibliotecaDbContext _context;

        public InsumosController(BibliotecaDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _context.Insumos.Include(i => i.Proveedor).AsNoTracking().OrderBy(i => i.NombreInsumo).ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var insumo = await _context.Insumos
                .Include(i => i.Proveedor)
                .FirstOrDefaultAsync(m => m.IdInsumo == id);
            if (insumo == null) return NotFound();
            return View(insumo);
        }

        public async Task<IActionResult> RegistrarEntrada(int? id)
        {
            if (id == null) return NotFound();

            var insumo = await _context.Insumos.AsNoTracking().FirstOrDefaultAsync(i => i.IdInsumo == id);
            if (insumo == null) return NotFound();

            return View(new RegistrarEntradaInsumoViewModel
            {
                IdInsumo = insumo.IdInsumo,
                NombreInsumo = insumo.NombreInsumo,
                UnidadMedida = insumo.UnidadMedida,
                StockActual = insumo.Stock
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegistrarEntrada(RegistrarEntradaInsumoViewModel modelo)
        {
            var insumo = await _context.Insumos.FirstOrDefaultAsync(i => i.IdInsumo == modelo.IdInsumo);
            if (insumo == null) return NotFound();

            modelo.NombreInsumo = insumo.NombreInsumo;
            modelo.UnidadMedida = insumo.UnidadMedida;
            modelo.StockActual = insumo.Stock;

            var textoCantidad = modelo.Cantidad?.Trim() ?? string.Empty;
            var normalizado = ValidarFormatoStock(textoCantidad);
            if (!decimal.TryParse(normalizado, NumberStyles.Number, CultureInfo.InvariantCulture, out var cantidad) ||
                cantidad <= 0m || cantidad > ReglasNegocio.MaxStockInsumo)
            {
                ModelState.AddModelError(nameof(modelo.Cantidad), "Ingrese una cantidad válida mayor que cero.");
            }
            else if (insumo.Stock + cantidad > ReglasNegocio.MaxStockInsumo)
            {
                ModelState.AddModelError(nameof(modelo.Cantidad), "El stock resultante no puede superar 10,000.");
            }

            if (ModelState.IsValid)
            {
                insumo.Stock += cantidad;
                await _context.SaveChangesAsync();
                TempData["Mensaje"] = $"Se registró una entrada de {cantidad:N2} {insumo.UnidadMedida}(s) para «{insumo.NombreInsumo}».";
                return RedirectToAction(nameof(Index));
            }

            return View(modelo);
        }

        public async Task<IActionResult> Create()
        {
            ViewBag.IdProveedor = new SelectList(await _context.Proveedores.AsNoTracking().OrderBy(p => p.NombreProveedor).ToListAsync(), "IdProveedor", "NombreProveedor");
            ViewBag.UnidadMedida = CrearSelectUnidades(null);
            return View(new Insumo { Estado = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("NombreInsumo,UnidadMedida,Stock,StockMinimo,Estado,IdProveedor")] Insumo insumo)
        {
            NormalizarErrorStock(insumo);
            NormalizarErrorStockMinimo(insumo);
            NormalizarErrorProveedor(insumo);
            if (ModelState.IsValid && !await ProveedorExiste(insumo.IdProveedor))
            {
                ModelState.AddModelError(nameof(Insumo.IdProveedor), "El proveedor seleccionado no es válido.");
                ModelState[nameof(Insumo.IdProveedor)].ValidationState = ModelValidationState.Invalid;
            }
            if (ModelState.IsValid)
            {
                _context.Add(insumo);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewBag.IdProveedor = new SelectList(await _context.Proveedores.AsNoTracking().OrderBy(p => p.NombreProveedor).ToListAsync(), "IdProveedor", "NombreProveedor", insumo.IdProveedor);
            ViewBag.UnidadMedida = CrearSelectUnidades(insumo.UnidadMedida);
            return View(insumo);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var insumo = await _context.Insumos.FindAsync(id);
            if (insumo == null) return NotFound();
            ViewBag.IdProveedor = new SelectList(await _context.Proveedores.AsNoTracking().OrderBy(p => p.NombreProveedor).ToListAsync(), "IdProveedor", "NombreProveedor", insumo.IdProveedor);
            ViewBag.UnidadMedida = CrearSelectUnidades(insumo.UnidadMedida);
            return View(insumo);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdInsumo,NombreInsumo,UnidadMedida,Stock,StockMinimo,Estado,IdProveedor")] Insumo insumo)
        {
            if (id != insumo.IdInsumo) return NotFound();
            NormalizarErrorStock(insumo);
            NormalizarErrorStockMinimo(insumo);
            NormalizarErrorProveedor(insumo);
            if (ModelState.IsValid && !await ProveedorExiste(insumo.IdProveedor))
            {
                ModelState.AddModelError(nameof(Insumo.IdProveedor), "El proveedor seleccionado no es válido.");
                ModelState[nameof(Insumo.IdProveedor)].ValidationState = ModelValidationState.Invalid;
            }
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(insumo);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!InsumoExists(insumo.IdInsumo)) return NotFound();
                    throw;
                }
            }
            ViewBag.IdProveedor = new SelectList(await _context.Proveedores.AsNoTracking().OrderBy(p => p.NombreProveedor).ToListAsync(), "IdProveedor", "NombreProveedor", insumo.IdProveedor);
            ViewBag.UnidadMedida = CrearSelectUnidades(insumo.UnidadMedida);
            return View(insumo);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var insumo = await _context.Insumos
                .Include(i => i.Proveedor)
                .FirstOrDefaultAsync(m => m.IdInsumo == id);
            if (insumo == null) return NotFound();
            return View(insumo);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int? id)
        {
            var insumo = await _context.Insumos.FindAsync(id);
            if (insumo == null) return RedirectToAction(nameof(Index));

            int recetas = await _context.ProductoInsumos.CountAsync(pi => pi.IdInsumo == insumo.IdInsumo);
            if (recetas > 0)
            {
                TempData["Error"] = $"No se puede eliminar «{insumo.NombreInsumo}» porque tiene {recetas} receta(s) asociada(s).";
                return RedirectToAction(nameof(Index));
            }

            _context.Insumos.Remove(insumo);
            await _context.SaveChangesAsync();
            TempData["Mensaje"] = "El insumo se eliminó correctamente.";

            return RedirectToAction(nameof(Index));
        }

        private bool InsumoExists(int id) =>
            _context.Insumos.Any(e => e.IdInsumo == id);

        private static readonly string[] Unidades = { "Kilogramo", "Gramo", "Litro", "Mililitro", "Unidad", "Docena", "Caja", "Quintal", "Bulto" };

        private static SelectList CrearSelectUnidades(string? seleccionado)
        {
            var unidades = Unidades.ToList();
            if (!string.IsNullOrWhiteSpace(seleccionado) && !unidades.Contains(seleccionado))
                unidades.Insert(0, seleccionado);
            return new SelectList(unidades, seleccionado);
        }

        private void NormalizarErrorStock(Insumo insumo)
        {
            ModelState.Remove(nameof(Insumo.Stock));
            var texto = Request.Form[nameof(Insumo.Stock)].ToString().Trim();
            ModelState.SetModelValue(nameof(Insumo.Stock), new ValueProviderResult(texto));
            var normalizado = ValidarFormatoStock(texto);
            if (decimal.TryParse(normalizado, NumberStyles.Number, CultureInfo.InvariantCulture, out var stock) &&
                stock >= 0m && stock <= ReglasNegocio.MaxStockInsumo)
            {
                insumo.Stock = stock;
                ModelState[nameof(Insumo.Stock)].ValidationState = ModelValidationState.Valid;
                return;
            }
            insumo.Stock = 0;
            ModelState.AddModelError(nameof(Insumo.Stock), "Ingrese un stock válido.");
            ModelState[nameof(Insumo.Stock)].ValidationState = ModelValidationState.Invalid;
        }

        private void NormalizarErrorStockMinimo(Insumo insumo)
        {
            ModelState.Remove(nameof(Insumo.StockMinimo));
            var texto = Request.Form[nameof(Insumo.StockMinimo)].ToString().Trim();
            ModelState.SetModelValue(nameof(Insumo.StockMinimo), new ValueProviderResult(texto));
            var normalizado = ValidarFormatoStock(texto);
            if (decimal.TryParse(normalizado, NumberStyles.Number, CultureInfo.InvariantCulture, out var stockMinimo) &&
                stockMinimo >= 0m && stockMinimo <= ReglasNegocio.MaxStockInsumo)
            {
                insumo.StockMinimo = stockMinimo;
                ModelState[nameof(Insumo.StockMinimo)].ValidationState = ModelValidationState.Valid;
                return;
            }
            insumo.StockMinimo = 0;
            ModelState.AddModelError(nameof(Insumo.StockMinimo), "Ingrese un stock mínimo válido.");
            ModelState[nameof(Insumo.StockMinimo)].ValidationState = ModelValidationState.Invalid;
        }

        private static string ValidarFormatoStock(string texto)
        {
            var valor = texto.Replace(" ", "");
            int ultimaComa = valor.LastIndexOf(',');
            int ultimoPunto = valor.LastIndexOf('.');
            if (ultimaComa >= 0 && ultimoPunto >= 0)
                return ultimaComa > ultimoPunto
                    ? valor.Replace(".", "").Replace(',', '.')
                    : valor.Replace(",", "");
            if (ultimaComa >= 0)
            {
                var partes = valor.Split(',');
                return partes.Length == 2 && partes[1].Length != 3
                    ? valor.Replace(',', '.')
                    : valor;
            }
            return valor;
        }

        private void NormalizarErrorProveedor(Insumo insumo)
        {
            ModelState.Remove(nameof(Insumo.IdProveedor));
            var texto = Request.Form[nameof(Insumo.IdProveedor)].ToString().Trim();
            ModelState.SetModelValue(nameof(Insumo.IdProveedor), new ValueProviderResult(texto));
            if (int.TryParse(texto, out var proveedorId) && proveedorId > 0)
            {
                insumo.IdProveedor = proveedorId;
                ModelState[nameof(Insumo.IdProveedor)].ValidationState = ModelValidationState.Valid;
                return;
            }
            insumo.IdProveedor = 0;
            ModelState.AddModelError(nameof(Insumo.IdProveedor), "Debe seleccionar un proveedor.");
            ModelState[nameof(Insumo.IdProveedor)].ValidationState = ModelValidationState.Invalid;
        }

        private Task<bool> ProveedorExiste(int idProveedor) =>
            _context.Proveedores.AnyAsync(p => p.IdProveedor == idProveedor);
    }
}
