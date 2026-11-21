using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PolleriaLovely.Data;
using PolleriaLovely.Models;
using PolleriaLovely.ViewModels;

namespace PolleriaLovely.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class ReportesController : Controller
    {
        private readonly BibliotecaDbContext _context;

        public ReportesController(BibliotecaDbContext context)
        {
            _context = context;
        }

        // GET: Reportes — consulta por rango de fechas (solo Administrador)
        public async Task<IActionResult> Index(DateTime? fechaInicio, DateTime? fechaFinal)
        {
            var modelo = new ReporteViewModel();

            // Primera carga (sin parámetros): solo se muestra el formulario.
            if (!Request.QueryString.HasValue)
                return View(modelo);

            modelo.FechaInicio = fechaInicio;
            modelo.FechaFinal = fechaFinal;

            if (fechaInicio == null)
            {
                ModelState.AddModelError(nameof(modelo.FechaInicio),
                    "Debe seleccionar la fecha inicial.");
            }

            if (fechaFinal == null)
            {
                ModelState.AddModelError(nameof(modelo.FechaFinal),
                    "Debe seleccionar la fecha final.");
            }

            if (fechaInicio != null && fechaFinal != null && fechaFinal.Value.Date < fechaInicio.Value.Date)
            {
                ModelState.AddModelError(nameof(modelo.FechaFinal),
                    "La fecha final debe ser igual o posterior a la fecha inicial.");
            }

            if (ModelState.IsValid)
            {
                modelo.FechaInicio = fechaInicio!.Value.Date;
                modelo.FechaFinal = fechaFinal!.Value.Date;
                await CargarReporte(modelo);
                modelo.ReporteGenerado = true;
            }

            return View(modelo);
        }

        private async Task CargarReporte(ReporteViewModel modelo)
        {
            var inicio = modelo.FechaInicio!.Value;
            // La fecha final debe incluir todo el día seleccionado.
            var finExclusivo = modelo.FechaFinal!.Value.AddDays(1);

            // 1) Resumen: total vendido y cantidad de ventas (una sola consulta).
            var resumen = await _context.Ventas
                .AsNoTracking()
                .Where(v => v.Fecha >= inicio && v.Fecha < finExclusivo)
                .GroupBy(_ => 1)
                .Select(g => new
                {
                    Total = g.Sum(v => v.Total),
                    Cantidad = g.Count()
                })
                .FirstOrDefaultAsync();

            modelo.TotalVendido = resumen?.Total ?? 0m;
            modelo.CantidadVentas = resumen?.Cantidad ?? 0;
            modelo.PromedioVenta = modelo.CantidadVentas == 0
                ? 0m
                : modelo.TotalVendido / modelo.CantidadVentas;

            // 2) Ventas agrupadas por cajero (registrador de la venta).
            modelo.VentasPorCajero = await _context.Ventas
                .AsNoTracking()
                .Where(v => v.Fecha >= inicio && v.Fecha < finExclusivo)
                .GroupBy(v => new { v.IdUsuario, v.Usuario.NombreUsuario })
                .Select(g => new VentaPorCajeroViewModel
                {
                    Cajero = g.Key.NombreUsuario,
                    CantidadVentas = g.Count(),
                    Total = g.Sum(v => v.Total)
                })
                .OrderByDescending(x => x.Total)
                .ToListAsync();

            // 3) Productos más vendidos: todos los del rango, por cantidad e importe.
            modelo.ProductosMasVendidos = await _context.DetalleVentas
                .AsNoTracking()
                .Where(d => d.Venta.Fecha >= inicio && d.Venta.Fecha < finExclusivo)
                .GroupBy(d => new { d.IdProducto, d.Producto.NombreProducto })
                .Select(g => new ProductoVendidoViewModel
                {
                    NombreProducto = g.Key.NombreProducto,
                    Cantidad = g.Sum(x => x.Cantidad),
                    Importe = g.Sum(x => x.Subtotal)
                })
                .OrderByDescending(x => x.Cantidad)
                .ThenByDescending(x => x.Importe)
                .ToListAsync();

            // 4) Ventas agrupadas por día.
            modelo.VentasPorDia = await _context.Ventas
                .AsNoTracking()
                .Where(v => v.Fecha >= inicio && v.Fecha < finExclusivo)
                .GroupBy(v => v.Fecha.Date)
                .Select(g => new VentaPorDiaViewModel
                {
                    Fecha = g.Key,
                    CantidadVentas = g.Count(),
                    Total = g.Sum(v => v.Total)
                })
                .OrderBy(x => x.Fecha)
                .ToListAsync();
        }
    }
}
