using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PolleriaLovely.Models;
using PolleriaLovely.Data;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace PolleriaLovely.Controllers
{
    public class InsumosController : Controller
    {
        private readonly BibliotecaDbContext _context;

        public InsumosController(BibliotecaDbContext context)
        {
            _context = context;
        }

        // GET: INSUMOS
        public async Task<IActionResult> Index()
        {
            var bibliotecaDbContext = _context.Insumos.Include(i => i.Proveedor);
            return View(await bibliotecaDbContext.ToListAsync());
        }

        // GET: INSUMOS/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var insumo = await _context.Insumos
                .Include(i => i.Proveedor)
                .FirstOrDefaultAsync(m => m.IdInsumo == id);
            if (insumo == null)
            {
                return NotFound();
            }

            return View(insumo);
        }

        // GET: INSUMOS/Create
        public IActionResult Create()
        {
            ViewData["IdProveedor"] = new SelectList(
                _context.Proveedores,
                "IdProveedor",
                "NombreProveedor");
            return View();
        }

        // POST: INSUMOS/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdInsumo,NombreInsumo,UnidadMedida,Stock,StockMinimo,Estado,IdProveedor")] Insumo insumo)
        {
            if (ModelState.IsValid)
            {
                _context.Add(insumo);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["IdProveedor"] = new SelectList(
                _context.Proveedores,
                "IdProveedor",
                "NombreProveedor",
                insumo.IdProveedor);
            return View(insumo);
        }

        // GET: INSUMOS/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var insumo = await _context.Insumos.FindAsync(id);
            if (insumo == null)
            {
                return NotFound();
            }
            ViewData["IdProveedor"] = new SelectList(
                _context.Proveedores,
                "IdProveedor",
                "NombreProveedor",
                insumo.IdProveedor);
            return View(insumo);
        }

        // POST: INSUMOS/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdInsumo,NombreInsumo,UnidadMedida,Stock,StockMinimo,Estado,IdProveedor")] Insumo insumo)
        {
            if (id != insumo.IdInsumo)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(insumo);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!InsumoExists(insumo.IdInsumo))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["IdProveedor"] = new SelectList(
                _context.Proveedores,
                "IdProveedor",
                "NombreProveedor",
                insumo.IdProveedor);
            return View(insumo);
        }

        // GET: INSUMOS/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var insumo = await _context.Insumos
                .Include(i => i.Proveedor)
                .FirstOrDefaultAsync(m => m.IdInsumo == id);
            if (insumo == null)
            {
                return NotFound();
            }

            return View(insumo);
        }

        // POST: INSUMOS/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int? id)
        {
            var insumo = await _context.Insumos.FindAsync(id);
            if (insumo != null)
            {
                _context.Insumos.Remove(insumo);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool InsumoExists(int id)
        {
            return _context.Insumos.Any(e => e.IdInsumo == id);
        }
    }
}
