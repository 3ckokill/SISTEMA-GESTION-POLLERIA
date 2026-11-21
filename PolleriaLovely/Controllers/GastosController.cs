using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PolleriaLovely.Data;
using PolleriaLovely.Models;
using PolleriaLovely.ViewModels;

namespace PolleriaLovely.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class GastosController : Controller
    {
        private readonly BibliotecaDbContext _context;

        public GastosController(BibliotecaDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(DateTime? fechaInicio, DateTime? fechaFinal)
        {
            var query = _context.Gastos
                .AsNoTracking()
                .Include(g => g.Usuario)
                .AsQueryable();

            if (fechaInicio.HasValue)
                query = query.Where(g => g.Fecha >= fechaInicio.Value.Date);
            if (fechaFinal.HasValue)
                query = query.Where(g => g.Fecha < fechaFinal.Value.Date.AddDays(1));

            ViewBag.FechaInicio = fechaInicio?.ToString("yyyy-MM-dd");
            ViewBag.FechaFinal = fechaFinal?.ToString("yyyy-MM-dd");

            var gastos = await query
                .OrderByDescending(g => g.Fecha)
                .ThenByDescending(g => g.IdGasto)
                .Select(g => new GastoListadoViewModel
                {
                    IdGasto = g.IdGasto,
                    Descripcion = g.Descripcion,
                    Categoria = g.Categoria,
                    Monto = g.Monto,
                    Fecha = g.Fecha,
                    MetodoPago = g.MetodoPago,
                    Usuario = g.Usuario.NombreUsuario,
                    Anulado = g.Anulado
                })
                .ToListAsync();

            return View(gastos);
        }

        public IActionResult Create() => View(new GastoEdicionViewModel { Fecha = DateTime.Today });

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(GastoEdicionViewModel modelo)
        {
            if (ModelState.IsValid)
            {
                var idUsuario = UsuarioActualId();
                if (idUsuario == null) return Forbid();

                _context.Gastos.Add(new Gasto
                {
                    Descripcion = modelo.Descripcion.Trim(),
                    Categoria = modelo.Categoria,
                    Monto = modelo.Monto,
                    Fecha = modelo.Fecha.Date,
                    MetodoPago = modelo.MetodoPago,
                    Observaciones = string.IsNullOrWhiteSpace(modelo.Observaciones) ? null : modelo.Observaciones.Trim(),
                    IdUsuario = idUsuario.Value
                });
                await _context.SaveChangesAsync();
                TempData["Mensaje"] = "El gasto se registró correctamente.";
                return RedirectToAction(nameof(Index));
            }

            return View(modelo);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var gasto = await _context.Gastos.AsNoTracking().FirstOrDefaultAsync(g => g.IdGasto == id);
            if (gasto == null || gasto.Anulado) return NotFound();
            return View(ToViewModel(gasto));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(GastoEdicionViewModel modelo)
        {
            var gasto = await _context.Gastos.FirstOrDefaultAsync(g => g.IdGasto == modelo.IdGasto);
            if (gasto == null || gasto.Anulado) return NotFound();

            if (ModelState.IsValid)
            {
                gasto.Descripcion = modelo.Descripcion.Trim();
                gasto.Categoria = modelo.Categoria;
                gasto.Monto = modelo.Monto;
                gasto.Fecha = modelo.Fecha.Date;
                gasto.MetodoPago = modelo.MetodoPago;
                gasto.Observaciones = string.IsNullOrWhiteSpace(modelo.Observaciones) ? null : modelo.Observaciones.Trim();
                await _context.SaveChangesAsync();
                TempData["Mensaje"] = "El gasto se actualizó correctamente.";
                return RedirectToAction(nameof(Index));
            }

            return View(modelo);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Anular(int id)
        {
            var gasto = await _context.Gastos.FirstOrDefaultAsync(g => g.IdGasto == id);
            if (gasto == null) return NotFound();
            if (gasto.Anulado)
            {
                TempData["Error"] = "El gasto ya está anulado.";
                return RedirectToAction(nameof(Index));
            }

            gasto.Anulado = true;
            await _context.SaveChangesAsync();
            TempData["Mensaje"] = "El gasto se anuló y no se incluirá en la utilidad.";
            return RedirectToAction(nameof(Index));
        }

        private int? UsuarioActualId()
        {
            var valor = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(valor, out var id) ? id : null;
        }

        private static GastoEdicionViewModel ToViewModel(Gasto gasto) => new()
        {
            IdGasto = gasto.IdGasto,
            Descripcion = gasto.Descripcion,
            Categoria = gasto.Categoria,
            Monto = gasto.Monto,
            Fecha = gasto.Fecha,
            MetodoPago = gasto.MetodoPago,
            Observaciones = gasto.Observaciones
        };
    }
}
