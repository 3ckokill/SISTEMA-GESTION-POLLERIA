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
    public class ProductosController : Controller
    {
        private readonly BibliotecaDbContext _context;

        public ProductosController(BibliotecaDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _context.Productos.Include(p => p.Categoria).AsNoTracking().OrderBy(p => p.NombreProducto).ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var producto = await _context.Productos
                .Include(p => p.Categoria).FirstOrDefaultAsync(m => m.IdProducto == id);
            if (producto == null) return NotFound();
            return View(producto);
        }

        public async Task<IActionResult> Create()
        {
            ViewBag.IdCategoria = new SelectList(await _context.Categorias.AsNoTracking().OrderBy(c => c.NombreCategoria).ToListAsync(), "IdCategoria", "NombreCategoria");
            return View(new Producto { Estado = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("NombreProducto,Descripcion,Precio,Estado,IdCategoria")] Producto producto)
        {
            NormalizarErrorPrecio(producto);
            NormalizarErrorCategoria(producto);
            if (ModelState.IsValid && !await CategoriaExiste(producto.IdCategoria))
            {
                ModelState.AddModelError(nameof(Producto.IdCategoria), "La categoría seleccionada no es válida.");
                ModelState[nameof(Producto.IdCategoria)].ValidationState = ModelValidationState.Invalid;
            }
            if (ModelState.IsValid)
            {
                _context.Add(producto);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewBag.IdCategoria = new SelectList(await _context.Categorias.AsNoTracking().OrderBy(c => c.NombreCategoria).ToListAsync(), "IdCategoria", "NombreCategoria", producto.IdCategoria);
            return View(producto);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var producto = await _context.Productos.FindAsync(id);
            if (producto == null) return NotFound();
            ViewBag.IdCategoria = new SelectList(await _context.Categorias.AsNoTracking().OrderBy(c => c.NombreCategoria).ToListAsync(), "IdCategoria", "NombreCategoria", producto.IdCategoria);
            return View(producto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdProducto,NombreProducto,Descripcion,Precio,Estado,IdCategoria")] Producto producto)
        {
            if (id != producto.IdProducto) return NotFound();
            NormalizarErrorPrecio(producto);
            NormalizarErrorCategoria(producto);
            if (ModelState.IsValid && !await CategoriaExiste(producto.IdCategoria))
            {
                ModelState.AddModelError(nameof(Producto.IdCategoria), "La categoría seleccionada no es válida.");
                ModelState[nameof(Producto.IdCategoria)].ValidationState = ModelValidationState.Invalid;
            }
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(producto);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductoExists(producto.IdProducto)) return NotFound();
                    throw;
                }
            }
            ViewBag.IdCategoria = new SelectList(await _context.Categorias.AsNoTracking().OrderBy(c => c.NombreCategoria).ToListAsync(), "IdCategoria", "NombreCategoria", producto.IdCategoria);
            return View(producto);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var producto = await _context.Productos.Include(p => p.Categoria).FirstOrDefaultAsync(m => m.IdProducto == id);
            if (producto == null) return NotFound();
            return View(producto);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int? id)
        {
            var producto = await _context.Productos.FindAsync(id);
            if (producto != null) _context.Productos.Remove(producto);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ProductoExists(int? idproduto) =>
            _context.Productos.Any(e => e.IdProducto == idproduto);

        private void NormalizarErrorPrecio(Producto producto)
        {
            ModelState.Remove(nameof(Producto.Precio));
            var texto = Request.Form[nameof(Producto.Precio)].ToString().Trim();
            ModelState.SetModelValue(nameof(Producto.Precio), new ValueProviderResult(texto));
            var normalizado = ValidarFormatoPrecio(texto);
            if (decimal.TryParse(normalizado, NumberStyles.Number, CultureInfo.InvariantCulture, out var precio) &&
                precio >= 0.01m && precio <= 1000000m)
            {
                producto.Precio = precio;
                ModelState[nameof(Producto.Precio)].ValidationState = ModelValidationState.Valid;
                return;
            }
            producto.Precio = 0;
            ModelState.AddModelError(nameof(Producto.Precio), "Ingrese un precio válido mayor que cero.");
            ModelState[nameof(Producto.Precio)].ValidationState = ModelValidationState.Invalid;
        }

        private static string ValidarFormatoPrecio(string texto)
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

        private void NormalizarErrorCategoria(Producto producto)
        {
            ModelState.Remove(nameof(Producto.IdCategoria));
            var texto = Request.Form[nameof(Producto.IdCategoria)].ToString().Trim();
            ModelState.SetModelValue(nameof(Producto.IdCategoria), new ValueProviderResult(texto));
            if (int.TryParse(texto, out var categoriaId) && categoriaId > 0)
            {
                producto.IdCategoria = categoriaId;
                ModelState[nameof(Producto.IdCategoria)].ValidationState = ModelValidationState.Valid;
                return;
            }
            producto.IdCategoria = 0;
            ModelState.AddModelError(nameof(Producto.IdCategoria), "Debe seleccionar una categoría.");
            ModelState[nameof(Producto.IdCategoria)].ValidationState = ModelValidationState.Invalid;
        }

        private Task<bool> CategoriaExiste(int idCategoria) =>
            _context.Categorias.AnyAsync(c => c.IdCategoria == idCategoria);
    }
}


