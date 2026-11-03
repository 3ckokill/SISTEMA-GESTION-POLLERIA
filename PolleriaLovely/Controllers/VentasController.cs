using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PolleriaLovely.Data;
using PolleriaLovely.Models;
using PolleriaLovely.ViewModels;
using System.Security.Claims;

namespace PolleriaLovely.Controllers
{
    [Authorize(Roles = "Cajero,Administrador")]
    public class VentasController : Controller
    {
        private readonly BibliotecaDbContext _context;
        private const decimal MaximoDecimal = 99999999.99m;

        public VentasController(BibliotecaDbContext context)
        {
            _context = context;
        }

        // GET: Ventas — historial (solo Administrador)
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Index()
        {
            var ventas = await _context.Ventas
                .AsNoTracking()
                .Include(v => v.Usuario)
                .Include(v => v.DetalleVentas)
                .OrderByDescending(v => v.IdVenta)
                .ToListAsync();
            return View(ventas);
        }

        // GET: Ventas/Details/5 — detalle de una venta (solo Administrador)
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return RedirectToAction(nameof(Index));

            var venta = await _context.Ventas
                .AsNoTracking()
                .Include(v => v.Usuario)
                .Include(v => v.DetalleVentas).ThenInclude(d => d.Producto)
                .FirstOrDefaultAsync(v => v.IdVenta == id);

            if (venta == null) return RedirectToAction(nameof(Index));
            return View(venta);
        }

        // GET: Ventas/Create — registro de venta (solo Cajero)
        [Authorize(Roles = "Cajero")]
        public async Task<IActionResult> Create()
        {
            await CargarProductos();
            var modelo = new VentaRegistroViewModel();
            modelo.Detalles.Add(new LineaVentaInput());
            return View(modelo);
        }

        // POST: Ventas/Create — registro de venta con cálculo en el servidor (solo Cajero)
        [Authorize(Roles = "Cajero")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(VentaRegistroViewModel modelo)
        {
            if (modelo.Detalles == null || modelo.Detalles.Count == 0)
            {
                ModelState.AddModelError("", "Debe agregar al menos un producto a la venta.");
            }

            int idUsuario = 0;
            if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out idUsuario))
            {
                ModelState.AddModelError("", "No se pudo identificar al usuario que registra la venta.");
            }

            if (ModelState.IsValid && modelo.Detalles != null && modelo.Detalles.Count > 0)
            {
                var ids = modelo.Detalles.Select(d => d.IdProducto).Distinct().ToList();
                var productos = await _context.Productos
                    .Where(p => ids.Contains(p.IdProducto))
                    .ToDictionaryAsync(p => p.IdProducto);

                var venta = new Venta
                {
                    Fecha = DateTime.Now,
                    IdUsuario = idUsuario
                };

                decimal total = 0;
                foreach (var linea in modelo.Detalles)
                {
                    if (!productos.TryGetValue(linea.IdProducto, out var producto))
                    {
                        ModelState.AddModelError("", "Uno de los productos seleccionados no es válido.");
                        break;
                    }
                    if (!producto.Estado)
                    {
                        ModelState.AddModelError("", $"El producto «{producto.NombreProducto}» no está disponible.");
                        break;
                    }

                    decimal subtotal = producto.Precio * linea.Cantidad;
                    if (subtotal > MaximoDecimal)
                    {
                        ModelState.AddModelError("", $"El subtotal del producto «{producto.NombreProducto}» supera el máximo permitido.");
                        break;
                    }

                    total += subtotal;
                    if (total > MaximoDecimal)
                    {
                        ModelState.AddModelError("", "El total de la venta supera el máximo permitido.");
                        break;
                    }

                    venta.DetalleVentas.Add(new DetalleVenta
                    {
                        IdProducto = producto.IdProducto,
                        Cantidad = linea.Cantidad,
                        PrecioUnitario = producto.Precio,
                        Subtotal = subtotal
                    });
                }

                if (ModelState.IsValid && venta.DetalleVentas.Count > 0)
                {
                    venta.Total = total;
                    _context.Ventas.Add(venta);
                    await _context.SaveChangesAsync();

                    TempData["Mensaje"] = $"Venta #{venta.IdVenta} registrada correctamente: {venta.DetalleVentas.Count} producto(s), total S/ {venta.Total:N2}.";
                    return RedirectToAction(nameof(Create));
                }
            }

            await CargarProductos();
            return View(modelo);
        }

        private async Task CargarProductos()
        {
            ViewBag.Productos = await _context.Productos
                .AsNoTracking()
                .Where(p => p.Estado)
                .OrderBy(p => p.NombreProducto)
                .ToListAsync();
        }
    }
}
