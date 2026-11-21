using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PolleriaLovely.Data;
using PolleriaLovely.Models;
using PolleriaLovely.ViewModels;

namespace PolleriaLovely.Controllers
{
    [Authorize(Roles = "Cajero,Administrador")]
    public class CajaController : Controller
    {
        private readonly BibliotecaDbContext _context;

        public CajaController(BibliotecaDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var modelo = new CajaPanelViewModel
            {
                EsAdministrador = User.IsInRole("Administrador")
            };

            if (modelo.EsAdministrador)
            {
                modelo.Historial = await _context.Cajas
                    .AsNoTracking()
                    .Include(c => c.Usuario)
                    .OrderByDescending(c => c.FechaApertura)
                    .Take(50)
                    .ToListAsync();
                return View(modelo);
            }

            var idUsuario = UsuarioActualId();
            if (idUsuario == null) return Forbid();

            modelo.CajaActiva = await CajaAbiertaAsync(idUsuario.Value);
            modelo.Historial = await _context.Cajas
                .AsNoTracking()
                .Where(c => c.IdUsuario == idUsuario.Value)
                .OrderByDescending(c => c.FechaApertura)
                .Take(20)
                .ToListAsync();

            if (modelo.CajaActiva != null)
            {
                var resumen = await TotalesAsync(modelo.CajaActiva.IdCaja);
                modelo.TotalVentas = resumen.Total;
                modelo.TotalEfectivo = resumen.Efectivo;
                modelo.TotalTarjeta = resumen.Tarjeta;
                modelo.TotalYapePlin = resumen.YapePlin;
                modelo.TotalOtro = resumen.Otro;
            }

            return View(modelo);
        }

        [Authorize(Roles = "Cajero")]
        public IActionResult Abrir() => View(new CajaAperturaViewModel());

        [Authorize(Roles = "Cajero")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Abrir(CajaAperturaViewModel modelo)
        {
            var idUsuario = UsuarioActualId();
            if (idUsuario == null) return Forbid();

            if (await CajaAbiertaAsync(idUsuario.Value) != null)
            {
                ModelState.AddModelError(string.Empty, "Ya tienes una caja abierta.");
            }

            if (ModelState.IsValid)
            {
                _context.Cajas.Add(new Caja
                {
                    FechaApertura = DateTime.Now,
                    MontoInicial = modelo.MontoInicial,
                    IdUsuario = idUsuario.Value
                });
                await _context.SaveChangesAsync();
                TempData["Mensaje"] = "La caja se abrió correctamente.";
                return RedirectToAction(nameof(Index));
            }

            return View(modelo);
        }

        [Authorize(Roles = "Cajero")]
        public async Task<IActionResult> Cerrar()
        {
            var idUsuario = UsuarioActualId();
            if (idUsuario == null) return Forbid();
            var caja = await CajaAbiertaAsync(idUsuario.Value);
            if (caja == null)
            {
                TempData["Error"] = "No tienes una caja abierta para cerrar.";
                return RedirectToAction(nameof(Index));
            }

            var resumen = await TotalesAsync(caja.IdCaja);
            return View(new CajaCierreViewModel
            {
                IdCaja = caja.IdCaja,
                MontoInicial = caja.MontoInicial,
                TotalEfectivo = resumen.Efectivo,
                EfectivoEsperado = caja.MontoInicial + resumen.Efectivo
            });
        }

        [Authorize(Roles = "Cajero")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cerrar(CajaCierreViewModel modelo)
        {
            var idUsuario = UsuarioActualId();
            if (idUsuario == null) return Forbid();
            var caja = await CajaAbiertaAsync(idUsuario.Value);
            if (caja == null || caja.IdCaja != modelo.IdCaja)
            {
                TempData["Error"] = "La caja ya no está abierta o no pertenece al usuario actual.";
                return RedirectToAction(nameof(Index));
            }

            var resumen = await TotalesAsync(caja.IdCaja);
            modelo.MontoInicial = caja.MontoInicial;
            modelo.TotalEfectivo = resumen.Efectivo;
            modelo.EfectivoEsperado = caja.MontoInicial + resumen.Efectivo;

            if (ModelState.IsValid)
            {
                caja.FechaCierre = DateTime.Now;
                caja.MontoContado = modelo.MontoContado!.Value;
                caja.Diferencia = modelo.MontoContado.Value - modelo.EfectivoEsperado;
                await _context.SaveChangesAsync();
                TempData["Mensaje"] = $"La caja se cerró correctamente. Diferencia: S/ {caja.Diferencia:N2}.";
                return RedirectToAction(nameof(Index));
            }

            return View(modelo);
        }

        private int? UsuarioActualId()
        {
            var valor = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(valor, out var id) ? id : null;
        }

        private Task<Caja?> CajaAbiertaAsync(int idUsuario) =>
            _context.Cajas.FirstOrDefaultAsync(c => c.IdUsuario == idUsuario && c.FechaCierre == null);

        private async Task<(decimal Total, decimal Efectivo, decimal Tarjeta, decimal YapePlin, decimal Otro)> TotalesAsync(int idCaja)
        {
            var ventas = await _context.Ventas
                .AsNoTracking()
                .Where(v => v.IdCaja == idCaja)
                .ToListAsync();

            return (
                ventas.Sum(v => v.Total),
                ventas.Where(v => v.MetodoPago == MetodoPago.Efectivo).Sum(v => v.Total),
                ventas.Where(v => v.MetodoPago == MetodoPago.Tarjeta).Sum(v => v.Total),
                ventas.Where(v => v.MetodoPago == MetodoPago.YapePlin).Sum(v => v.Total),
                ventas.Where(v => v.MetodoPago == MetodoPago.Otro).Sum(v => v.Total));
        }
    }
}
