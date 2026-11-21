using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PolleriaLovely.Data;
using PolleriaLovely.Models;
using PolleriaLovely.ViewModels;

namespace PolleriaLovely.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class MesasController : Controller
    {
        private readonly BibliotecaDbContext _context;

        public MesasController(BibliotecaDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var ocupadas = await _context.Pedidos
                .AsNoTracking()
                .Where(p => p.Estado != EstadoPedido.Cancelado && p.Estado != EstadoPedido.Cobrado)
                .Select(p => p.IdMesa)
                .Distinct()
                .ToListAsync();

            var ocupadasSet = ocupadas.ToHashSet();
            var mesas = await _context.Mesas
                .AsNoTracking()
                .OrderBy(m => m.Numero)
                .ToListAsync();

            var modelo = mesas.Select(m => new MesaListadoViewModel
            {
                IdMesa = m.IdMesa,
                Numero = m.Numero,
                Estado = m.Estado,
                Ocupada = ocupadasSet.Contains(m.IdMesa)
            });

            return View(modelo);
        }

        public IActionResult Create() => View(new MesaEdicionViewModel());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(MesaEdicionViewModel modelo)
        {
            if (await NumeroExisteAsync(modelo.Numero, null))
                ModelState.AddModelError(nameof(modelo.Numero), "Ya existe una mesa con ese número.");

            if (ModelState.IsValid)
            {
                _context.Mesas.Add(new Mesa { Numero = modelo.Numero, Estado = true });
                await _context.SaveChangesAsync();
                TempData["Mensaje"] = $"La Mesa {modelo.Numero} se creó correctamente.";
                return RedirectToAction(nameof(Index));
            }

            return View(modelo);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var mesa = await _context.Mesas.AsNoTracking().FirstOrDefaultAsync(m => m.IdMesa == id);
            if (mesa == null) return NotFound();
            return View(new MesaEdicionViewModel { IdMesa = mesa.IdMesa, Numero = mesa.Numero, Estado = mesa.Estado });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(MesaEdicionViewModel modelo)
        {
            var mesa = await _context.Mesas.FirstOrDefaultAsync(m => m.IdMesa == modelo.IdMesa);
            if (mesa == null) return NotFound();

            if (await NumeroExisteAsync(modelo.Numero, modelo.IdMesa))
                ModelState.AddModelError(nameof(modelo.Numero), "Ya existe una mesa con ese número.");

            if (mesa.Estado && !modelo.Estado && await MesaTienePedidoEnCurso(modelo.IdMesa))
                ModelState.AddModelError(nameof(modelo.Estado), "No se puede desactivar una mesa que tiene un pedido en curso.");

            if (ModelState.IsValid)
            {
                mesa.Numero = modelo.Numero;
                mesa.Estado = modelo.Estado;
                await _context.SaveChangesAsync();
                TempData["Mensaje"] = $"La Mesa {mesa.Numero} se actualizó correctamente.";
                return RedirectToAction(nameof(Index));
            }

            return View(modelo);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CambiarEstado(int id)
        {
            var mesa = await _context.Mesas.FirstOrDefaultAsync(m => m.IdMesa == id);
            if (mesa == null) return NotFound();

            if (mesa.Estado && await MesaTienePedidoEnCurso(id))
            {
                TempData["Error"] = $"No se puede desactivar la Mesa {mesa.Numero} porque tiene un pedido en curso.";
                return RedirectToAction(nameof(Index));
            }

            mesa.Estado = !mesa.Estado;
            await _context.SaveChangesAsync();
            TempData["Mensaje"] = mesa.Estado
                ? $"La Mesa {mesa.Numero} se activó correctamente."
                : $"La Mesa {mesa.Numero} se desactivó correctamente.";
            return RedirectToAction(nameof(Index));
        }

        private Task<bool> NumeroExisteAsync(int numero, int? idExcluir) =>
            _context.Mesas.AnyAsync(m => m.Numero == numero && (!idExcluir.HasValue || m.IdMesa != idExcluir.Value));

        private Task<bool> MesaTienePedidoEnCurso(int idMesa) =>
            _context.Pedidos.AnyAsync(p => p.IdMesa == idMesa && p.Estado != EstadoPedido.Cancelado && p.Estado != EstadoPedido.Cobrado);
    }
}
