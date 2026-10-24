using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using PolleriaLovely.Models;
using PolleriaLovely.Data;
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
            if (insumo != null)
            {
                _context.Insumos.Remove(insumo);
                await _context.SaveChangesAsync();
                TempData["Mensaje"] = "El insumo se eliminó correctamente.";
            }

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
                stock >= 0m && stock <= 1000000m)
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
                stockMinimo >= 0m && stockMinimo <= 1000000m)
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
