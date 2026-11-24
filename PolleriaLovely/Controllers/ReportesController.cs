using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PolleriaLovely.Data;
using PolleriaLovely.Documents;
using PolleriaLovely.Models;
using PolleriaLovely.ViewModels;
using System.Globalization;
using System.Text;
using QuestPDF.Fluent;

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

        [HttpGet]
        public async Task<IActionResult> ExportarCsv(DateTime? fechaInicio, DateTime? fechaFinal)
        {
            if (fechaInicio == null || fechaFinal == null || fechaFinal.Value.Date < fechaInicio.Value.Date)
                return RedirectToAction(nameof(Index), new { fechaInicio, fechaFinal });

            var modelo = new ReporteViewModel
            {
                FechaInicio = fechaInicio.Value.Date,
                FechaFinal = fechaFinal.Value.Date,
                ReporteGenerado = true
            };
            await CargarReporte(modelo);

            var separador = ",";
            var csv = new StringBuilder();
            csv.AppendLine("REPORTE DE VENTAS Y GASTOS");
            csv.AppendLine($"Periodo{separador}{modelo.FechaInicio:dd/MM/yyyy} - {modelo.FechaFinal:dd/MM/yyyy}");
            csv.AppendLine();
            csv.AppendLine("RESUMEN");
            csv.AppendLine($"Total vendido{separador}{modelo.TotalVendido.ToString("0.00", CultureInfo.InvariantCulture)}");
            csv.AppendLine($"Cantidad de ventas{separador}{modelo.CantidadVentas}");
            csv.AppendLine($"Promedio por venta{separador}{modelo.PromedioVenta.ToString("0.00", CultureInfo.InvariantCulture)}");
            csv.AppendLine($"Total gastado{separador}{modelo.TotalGastado.ToString("0.00", CultureInfo.InvariantCulture)}");
            csv.AppendLine($"Utilidad estimada{separador}{modelo.UtilidadEstimada.ToString("0.00", CultureInfo.InvariantCulture)}");
            csv.AppendLine();
            csv.AppendLine("VENTAS POR METODO DE PAGO");
            csv.AppendLine("Metodo,Cantidad de ventas,Total");
            foreach (var item in modelo.VentasPorMetodoPago)
                csv.AppendLine($"{Csv(item.NombreMetodo)},{item.CantidadVentas},{item.Total.ToString("0.00", CultureInfo.InvariantCulture)}");
            csv.AppendLine();
            csv.AppendLine("VENTAS POR CAJERO");
            csv.AppendLine("Cajero,Cantidad de ventas,Total");
            foreach (var item in modelo.VentasPorCajero)
                csv.AppendLine($"{Csv(item.Cajero)},{item.CantidadVentas},{item.Total.ToString("0.00", CultureInfo.InvariantCulture)}");
            csv.AppendLine();
            csv.AppendLine("PRODUCTOS MAS VENDIDOS");
            csv.AppendLine("Producto,Cantidad,Importe");
            foreach (var item in modelo.ProductosMasVendidos)
                csv.AppendLine($"{Csv(item.NombreProducto)},{item.Cantidad},{item.Importe.ToString("0.00", CultureInfo.InvariantCulture)}");
            csv.AppendLine();
            csv.AppendLine("VENTAS POR DIA");
            csv.AppendLine("Fecha,Cantidad de ventas,Total");
            foreach (var item in modelo.VentasPorDia)
                csv.AppendLine($"{item.Fecha:dd/MM/yyyy},{item.CantidadVentas},{item.Total.ToString("0.00", CultureInfo.InvariantCulture)}");
            csv.AppendLine();
            csv.AppendLine("RESUMEN POR CAJA");
            csv.AppendLine("Caja,Cajero,Ventas,Efectivo esperado,Contado,Diferencia");
            foreach (var item in modelo.ResumenCajas)
                csv.AppendLine($"{item.IdCaja},{Csv(item.Cajero)},{item.TotalVentas.ToString("0.00", CultureInfo.InvariantCulture)},{item.EfectivoEsperado.ToString("0.00", CultureInfo.InvariantCulture)},{(item.MontoContado?.ToString("0.00", CultureInfo.InvariantCulture) ?? "Pendiente")},{(item.Diferencia?.ToString("0.00", CultureInfo.InvariantCulture) ?? "Pendiente")}");

            var bytes = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(csv.ToString())).ToArray();
            return File(bytes, "text/csv; charset=utf-8", $"reporte-{fechaInicio:yyyyMMdd}-{fechaFinal:yyyyMMdd}.csv");
        }

        [HttpGet]
        public async Task<IActionResult> DescargarPdf(DateTime? fechaInicio, DateTime? fechaFinal)
        {
            if (fechaInicio == null || fechaFinal == null || fechaFinal.Value.Date < fechaInicio.Value.Date)
                return RedirectToAction(nameof(Index), new { fechaInicio, fechaFinal });

            var modelo = new ReporteViewModel
            {
                FechaInicio = fechaInicio.Value.Date,
                FechaFinal = fechaFinal.Value.Date,
                ReporteGenerado = true
            };
            await CargarReporte(modelo);

            var documento = new ReportePdfDocument(modelo);
            var pdf = documento.GeneratePdf();
            return File(pdf, "application/pdf", $"reporte-{fechaInicio:yyyyMMdd}-{fechaFinal:yyyyMMdd}.pdf");
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

            modelo.TotalGastado = await _context.Gastos
                .AsNoTracking()
                .Where(g => !g.Anulado && g.Fecha >= inicio && g.Fecha < finExclusivo)
                .SumAsync(g => (decimal?)g.Monto) ?? 0m;
            modelo.UtilidadEstimada = modelo.TotalVendido - modelo.TotalGastado;

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

            // 5) Ventas agrupadas por método de pago.
            modelo.VentasPorMetodoPago = await _context.Ventas
                .AsNoTracking()
                .Where(v => v.Fecha >= inicio && v.Fecha < finExclusivo)
                .GroupBy(v => v.MetodoPago)
                .Select(g => new VentaPorMetodoPagoViewModel
                {
                    MetodoPago = g.Key,
                    CantidadVentas = g.Count(),
                    Total = g.Sum(v => v.Total)
                })
                .OrderByDescending(x => x.Total)
                .ToListAsync();

            foreach (var item in modelo.VentasPorMetodoPago)
                item.NombreMetodo = NombreMetodoPago(item.MetodoPago);

            // 6) Cajas que tuvieron ventas dentro del rango seleccionado.
            modelo.ResumenCajas = await _context.Cajas
                .AsNoTracking()
                .Where(c => c.Ventas.Any(v => v.Fecha >= inicio && v.Fecha < finExclusivo))
                .Select(c => new ResumenCajaViewModel
                {
                    IdCaja = c.IdCaja,
                    Cajero = c.Usuario.NombreUsuario,
                    FechaApertura = c.FechaApertura,
                    FechaCierre = c.FechaCierre,
                    MontoInicial = c.MontoInicial,
                    TotalVentas = c.Ventas
                        .Where(v => v.Fecha >= inicio && v.Fecha < finExclusivo)
                        .Sum(v => (decimal?)v.Total) ?? 0m,
                    VentasEfectivo = c.Ventas
                        .Where(v => v.Fecha >= inicio && v.Fecha < finExclusivo && v.MetodoPago == MetodoPago.Efectivo)
                        .Sum(v => (decimal?)v.Total) ?? 0m,
                    MontoContado = c.MontoContado,
                    Diferencia = c.Diferencia
                })
                .OrderByDescending(x => x.FechaApertura)
                .ToListAsync();

            foreach (var caja in modelo.ResumenCajas)
                caja.EfectivoEsperado = caja.MontoInicial + caja.VentasEfectivo;
        }

        private static string NombreMetodoPago(MetodoPago metodo) => metodo switch
        {
            MetodoPago.Efectivo => "Efectivo",
            MetodoPago.Tarjeta => "Tarjeta",
            MetodoPago.YapePlin => "Yape/Plin",
            MetodoPago.Otro => "Otro",
            _ => "No registrado"
        };

        private static string Csv(string valor) => $"\"{valor.Replace("\"", "\"\"")}\"";
    }
}
