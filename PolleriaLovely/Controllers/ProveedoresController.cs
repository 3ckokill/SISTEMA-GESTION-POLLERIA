using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PolleriaLovely.Models;
using PolleriaLovely.Data;

namespace PolleriaLovely.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class ProveedoresController : Controller
    {
        private readonly BibliotecaDbContext _context;

        public ProveedoresController(BibliotecaDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _context.Proveedores.AsNoTracking().OrderBy(p => p.NombreProveedor).ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var proveedor = await _context.Proveedores
                .FirstOrDefaultAsync(m => m.IdProveedor == id);
            if (proveedor == null) return NotFound();
            return View(proveedor);
        }

        public IActionResult Create() => View(new Proveedor { Estado = true });

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("NombreProveedor,RucProveedor,Telefono,Direccion,Estado")] Proveedor proveedor)
        {
            if (ModelState.IsValid && await RucExiste(proveedor.RucProveedor, null))
            {
                ModelState.AddModelError(nameof(Proveedor.RucProveedor), "Ya existe un proveedor con ese RUC.");
            }
            if (ModelState.IsValid)
            {
                _context.Add(proveedor);
                await _context.SaveChangesAsync();
                TempData["Mensaje"] = "El proveedor se registró correctamente.";
                return RedirectToAction(nameof(Index));
            }
            return View(proveedor);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var proveedor = await _context.Proveedores.FindAsync(id);
            if (proveedor == null) return NotFound();
            return View(proveedor);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdProveedor,NombreProveedor,RucProveedor,Telefono,Direccion,Estado")] Proveedor proveedor)
        {
            if (id != proveedor.IdProveedor) return NotFound();
            if (ModelState.IsValid && await RucExiste(proveedor.RucProveedor, proveedor.IdProveedor))
            {
                ModelState.AddModelError(nameof(Proveedor.RucProveedor), "Ya existe un proveedor con ese RUC.");
            }
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(proveedor);
                    await _context.SaveChangesAsync();
                    TempData["Mensaje"] = "Los cambios se guardaron correctamente.";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProveedorExists(proveedor.IdProveedor)) return NotFound();
                    throw;
                }
            }
            return View(proveedor);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var proveedor = await _context.Proveedores
                .Include(p => p.Insumos)
                .FirstOrDefaultAsync(m => m.IdProveedor == id);
            if (proveedor == null) return NotFound();
            return View(proveedor);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int? id)
        {
            var proveedor = await _context.Proveedores.Include(p => p.Insumos).FirstOrDefaultAsync(p => p.IdProveedor == id);
            if (proveedor == null) return NotFound();
            if (proveedor.Insumos.Count > 0)
            {
                TempData["Error"] = $"No se puede eliminar «{proveedor.NombreProveedor}» porque tiene {proveedor.Insumos.Count} insumo(s) asociado(s).";
                return RedirectToAction(nameof(Index));
            }
            _context.Proveedores.Remove(proveedor);
            await _context.SaveChangesAsync();
            TempData["Mensaje"] = "El proveedor se eliminó correctamente.";
            return RedirectToAction(nameof(Index));
        }

        private bool ProveedorExists(int id) =>
            _context.Proveedores.Any(e => e.IdProveedor == id);

        private Task<bool> RucExiste(string ruc, int? idExcluir) =>
            _context.Proveedores.AnyAsync(p => p.RucProveedor == ruc && (!idExcluir.HasValue || p.IdProveedor != idExcluir));
    }
}
