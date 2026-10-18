using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PolleriaLovely.Models;
using PolleriaLovely.Data;

namespace PolleriaLovely.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class CategoriasController : Controller
    {
        private readonly BibliotecaDbContext _context;

        public CategoriasController(BibliotecaDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _context.Categorias.Include(c => c.Productos).AsNoTracking().OrderBy(c => c.NombreCategoria).ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var categoria = await _context.Categorias.FirstOrDefaultAsync(m => m.IdCategoria == id);
            if (categoria == null) return NotFound();
            return View(categoria);
        }

        public IActionResult Create() => View(new Categoria { Estado = true });

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("NombreCategoria,Descripcion,Estado")] Categoria categoria)
        {
            NormalizarErroresCategoria();
            if (ModelState.IsValid)
            {
                _context.Add(categoria);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(categoria);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var categoria = await _context.Categorias.FindAsync(id);
            if (categoria == null) return NotFound();
            return View(categoria);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int? id, [Bind("IdCategoria,NombreCategoria,Descripcion,Estado")] Categoria categoria)
        {
            if (id != categoria.IdCategoria) return NotFound();
            NormalizarErroresCategoria();
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(categoria);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CategoriaExists(categoria.IdCategoria)) return NotFound();
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(categoria);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var categoria = await _context.Categorias.Include(c => c.Productos).FirstOrDefaultAsync(m => m.IdCategoria == id);
            if (categoria == null) return NotFound();
            return View(categoria);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int? id)
        {
            var categoria = await _context.Categorias.Include(c => c.Productos).FirstOrDefaultAsync(c => c.IdCategoria == id);
            if (categoria == null) return NotFound();
            if (categoria.Productos.Count > 0)
            {
                TempData["Error"] = $"No se puede eliminar «{categoria.NombreCategoria}» porque tiene {categoria.Productos.Count} producto(s) asociado(s).";
                return RedirectToAction(nameof(Index));
            }
            _context.Categorias.Remove(categoria);
            await _context.SaveChangesAsync();
            TempData["Mensaje"] = "La categoría se eliminó correctamente.";
            return RedirectToAction(nameof(Index));
        }

        private bool CategoriaExists(int? id) =>
            _context.Categorias.Any(e => e.IdCategoria == id);

        private void NormalizarErroresCategoria()
        {
            if (string.IsNullOrWhiteSpace(Request.Form[nameof(Categoria.NombreCategoria)]))
            {
                ModelState.Remove(nameof(Categoria.NombreCategoria));
                ModelState.AddModelError(nameof(Categoria.NombreCategoria), "El nombre de la categoría es obligatorio.");
            }
        }
    }
}
