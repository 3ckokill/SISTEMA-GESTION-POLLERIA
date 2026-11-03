using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PolleriaLovely.Data;
using PolleriaLovely.Models;
using PolleriaLovely.ViewModels;
using System.Security.Claims;

namespace PolleriaLovely.Controllers
{
    [Authorize(Roles = "Cajero,Mozo,Administrador")]
    public class PedidosController : Controller
    {
        private readonly BibliotecaDbContext _context;

        public PedidosController(BibliotecaDbContext context)
        {
            _context = context;
        }

        // GET: Pedidos — consulta (Cajero solo lee; Mozo y Administrador también operan).
        // Los cancelados solo los ve el Administrador (auditoría).
        public async Task<IActionResult> Index()
        {
            IQueryable<Pedido> query = _context.Pedidos
                .AsNoTracking()
                .Include(p => p.Mesa)
                .Include(p => p.Usuario)
                .Include(p => p.DetallePedidos).ThenInclude(d => d.Producto)
                .OrderByDescending(p => p.IdPedido);

            if (!User.IsInRole("Administrador"))
            {
                query = query.Where(p => p.Estado != EstadoPedido.Cancelado);
            }

            var pedidos = await query.ToListAsync();
            return View(pedidos);
        }

        // GET: Pedidos/Create — registro de pedido (solo Mozo)
        [Authorize(Roles = "Mozo")]
        public async Task<IActionResult> Create()
        {
            await CargarMesasYProductos();
            var modelo = new PedidoRegistroViewModel();
            modelo.Detalles.Add(new LineaPedidoInput());
            return View(modelo);
        }

        // POST: Pedidos/Create — registro con cantidades validadas en el servidor (solo Mozo)
        [Authorize(Roles = "Mozo")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PedidoRegistroViewModel modelo)
        {
            if (modelo.Detalles == null || modelo.Detalles.Count == 0)
            {
                ModelState.AddModelError("", "Debe agregar al menos un producto al pedido.");
            }

            int idUsuario = 0;
            if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out idUsuario))
            {
                ModelState.AddModelError("", "No se pudo identificar al mozo que registra el pedido.");
            }

            Mesa? mesaLibre = null;
            if (ModelState.IsValid)
            {
                mesaLibre = await _context.Mesas
                    .AsNoTracking()
                    .FirstOrDefaultAsync(m => m.IdMesa == modelo.IdMesa && m.Estado);

                if (mesaLibre == null)
                {
                    ModelState.AddModelError(nameof(PedidoRegistroViewModel.IdMesa), "La mesa seleccionada no es válida.");
                }
                else if (await MesaTienePedidoEnCurso(modelo.IdMesa))
                {
                    ModelState.AddModelError(nameof(PedidoRegistroViewModel.IdMesa), "La mesa ya tiene un pedido en curso.");
                }
            }

            if (ModelState.IsValid && modelo.Detalles != null && modelo.Detalles.Count > 0)
            {
                var ids = modelo.Detalles.Select(d => d.IdProducto).Distinct().ToList();
                var productos = await _context.Productos
                    .Where(p => ids.Contains(p.IdProducto))
                    .ToDictionaryAsync(p => p.IdProducto);

                var pedido = new Pedido
                {
                    Fecha = DateTime.Now,
                    Estado = EstadoPedido.Pendiente,
                    IdMesa = modelo.IdMesa,
                    IdUsuario = idUsuario
                };

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

                    pedido.DetallePedidos.Add(new DetallePedido
                    {
                        IdProducto = producto.IdProducto,
                        Cantidad = linea.Cantidad
                    });
                }

                if (ModelState.IsValid && pedido.DetallePedidos.Count > 0)
                {
                    _context.Pedidos.Add(pedido);
                    await _context.SaveChangesAsync();

                    TempData["Mensaje"] = $"Pedido #{pedido.IdPedido} registrado en la Mesa {mesaLibre?.Numero} ({pedido.DetallePedidos.Count} producto(s)).";
                    return RedirectToAction(nameof(Create));
                }
            }

            await CargarMesasYProductos();
            return View(modelo);
        }

        // GET: Pedidos/Agregar/5 — agregar productos a un pedido abierto (Mozo y Administrador)
        [Authorize(Roles = "Mozo,Administrador")]
        public async Task<IActionResult> Agregar(int? id)
        {
            if (id == null) return RedirectToAction(nameof(Index));

            var pedido = await _context.Pedidos
                .AsNoTracking()
                .Include(p => p.Mesa)
                .Include(p => p.DetallePedidos).ThenInclude(d => d.Producto)
                .FirstOrDefaultAsync(p => p.IdPedido == id);

            if (pedido == null) return RedirectToAction(nameof(Index));
            if (!EstaAbierto(pedido.Estado))
            {
                TempData["Error"] = $"El pedido #{pedido.IdPedido} ya está finalizado y no se le pueden agregar productos.";
                return RedirectToAction(nameof(Index));
            }

            await CargarProductos();
            return View(pedido);
        }

        // POST: Pedidos/Agregar/5 — agrega líneas; si estaba Listo vuelve a Pendiente (Mozo y Administrador)
        [Authorize(Roles = "Mozo,Administrador")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Agregar(int idPedido, PedidoAgregarViewModel modelo)
        {
            var pedido = await _context.Pedidos
                .Include(p => p.DetallePedidos)
                .FirstOrDefaultAsync(p => p.IdPedido == idPedido);

            if (pedido == null)
            {
                TempData["Error"] = "El pedido solicitado no existe.";
                return RedirectToAction(nameof(Index));
            }
            if (!EstaAbierto(pedido.Estado))
            {
                TempData["Error"] = $"El pedido #{pedido.IdPedido} ya está finalizado y no se le pueden agregar productos.";
                return RedirectToAction(nameof(Index));
            }

            if (modelo.Detalles == null || modelo.Detalles.Count == 0)
            {
                ModelState.AddModelError("", "Debe agregar al menos un producto.");
            }

            if (ModelState.IsValid && modelo.Detalles != null && modelo.Detalles.Count > 0)
            {
                var ids = modelo.Detalles.Select(d => d.IdProducto).Distinct().ToList();
                var productos = await _context.Productos
                    .Where(p => ids.Contains(p.IdProducto))
                    .ToDictionaryAsync(p => p.IdProducto);

                int agregados = 0;
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

                    pedido.DetallePedidos.Add(new DetallePedido
                    {
                        IdProducto = producto.IdProducto,
                        Cantidad = linea.Cantidad
                    });
                    agregados++;
                }

                if (ModelState.IsValid && agregados > 0)
                {
                    if (pedido.Estado == EstadoPedido.Listo)
                    {
                        pedido.Estado = EstadoPedido.Pendiente;
                    }

                    await _context.SaveChangesAsync();

                    TempData["Mensaje"] = $"Se agregaron {agregados} producto(s) al Pedido #{pedido.IdPedido}.";
                    return RedirectToAction(nameof(Index));
                }
            }

            await CargarProductos();
            var pedidoVista = await _context.Pedidos
                .AsNoTracking()
                .Include(p => p.Mesa)
                .Include(p => p.DetallePedidos).ThenInclude(d => d.Producto)
                .FirstOrDefaultAsync(p => p.IdPedido == idPedido);

            if (pedidoVista == null) return RedirectToAction(nameof(Index));
            return View(pedidoVista);
        }

        // POST: Pedidos/ActualizarEstado — cambio de estado (Mozo y Administrador).
        // Solo el Administrador puede cancelar; Entregado y Cancelado son terminales.
        [Authorize(Roles = "Mozo,Administrador")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ActualizarEstado(int idPedido, EstadoPedido estado)
        {
            if (!Enum.IsDefined(estado))
            {
                TempData["Error"] = "El estado seleccionado no es válido.";
                return RedirectToAction(nameof(Index));
            }

            var pedido = await _context.Pedidos.FirstOrDefaultAsync(p => p.IdPedido == idPedido);
            if (pedido == null)
            {
                TempData["Error"] = "El pedido solicitado no existe.";
                return RedirectToAction(nameof(Index));
            }

            if (!EstaAbierto(pedido.Estado))
            {
                TempData["Error"] = $"El pedido #{pedido.IdPedido} está finalizado («{NombreEstado(pedido.Estado)}») y no se puede modificar.";
                return RedirectToAction(nameof(Index));
            }

            if (estado == EstadoPedido.Cancelado && !User.IsInRole("Administrador"))
            {
                TempData["Error"] = "Solo el administrador puede cancelar pedidos.";
                return RedirectToAction(nameof(Index));
            }

            pedido.Estado = estado;
            await _context.SaveChangesAsync();

            TempData["Mensaje"] = $"Pedido #{pedido.IdPedido} actualizado a «{NombreEstado(estado)}».";
            return RedirectToAction(nameof(Index));
        }

        private static bool EstaAbierto(EstadoPedido estado) =>
            estado is EstadoPedido.Pendiente or EstadoPedido.EnPreparacion or EstadoPedido.Listo;

        private static string NombreEstado(EstadoPedido estado) => estado switch
        {
            EstadoPedido.EnPreparacion => "En preparación",
            _ => estado.ToString()
        };

        private async Task<bool> MesaTienePedidoEnCurso(int idMesa) =>
            await _context.Pedidos.AnyAsync(p =>
                p.IdMesa == idMesa &&
                (p.Estado == EstadoPedido.Pendiente ||
                 p.Estado == EstadoPedido.EnPreparacion ||
                 p.Estado == EstadoPedido.Listo));

        private async Task CargarMesasYProductos()
        {
            var mesasOcupadas = await _context.Pedidos
                .Where(p => p.Estado == EstadoPedido.Pendiente ||
                            p.Estado == EstadoPedido.EnPreparacion ||
                            p.Estado == EstadoPedido.Listo)
                .Select(p => p.IdMesa)
                .Distinct()
                .ToListAsync();

            ViewBag.Mesas = await _context.Mesas
                .AsNoTracking()
                .Where(m => m.Estado && !mesasOcupadas.Contains(m.IdMesa))
                .OrderBy(m => m.Numero)
                .ToListAsync();

            await CargarProductos();
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
