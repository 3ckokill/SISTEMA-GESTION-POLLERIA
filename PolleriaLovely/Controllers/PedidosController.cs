using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using PolleriaLovely.Data;
using PolleriaLovely.Hubs;
using PolleriaLovely.Models;
using PolleriaLovely.Services;
using PolleriaLovely.ViewModels;
using System.Security.Claims;

namespace PolleriaLovely.Controllers
{
    [Authorize(Roles = "Cajero,Mozo,Administrador")]
    public class PedidosController : Controller
    {
        private readonly BibliotecaDbContext _context;
        private readonly InventarioService _inventario;
        private readonly IHubContext<PedidosHub> _hubContext;
        private const decimal MaximoDecimal = 99999999.99m;

        public PedidosController(BibliotecaDbContext context, InventarioService inventario, IHubContext<PedidosHub> hubContext)
        {
            _context = context;
            _inventario = inventario;
            _hubContext = hubContext;
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
            else
            {
                ValidarLimitesPedido(modelo.Detalles);
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
                    await NotificarCambioAsync($"Nuevo pedido #{pedido.IdPedido} registrado en la Mesa {mesaLibre?.Numero}.");

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
            else
            {
                ValidarLimitesPedido(modelo.Detalles, pedido.DetallePedidos.Sum(d => d.Cantidad));
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
                    await NotificarCambioAsync($"Se agregaron productos al Pedido #{pedido.IdPedido}.");

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

            if (estado == EstadoPedido.Cobrado)
            {
                TempData["Error"] = "El estado «Cobrado» solo se asigna al cobrar el pedido.";
                return RedirectToAction(nameof(Index));
            }

            pedido.Estado = estado;
            await _context.SaveChangesAsync();
            await NotificarCambioAsync($"El Pedido #{pedido.IdPedido} cambió a «{NombreEstado(estado)}».");

            TempData["Mensaje"] = $"Pedido #{pedido.IdPedido} actualizado a «{NombreEstado(estado)}».";
            return RedirectToAction(nameof(Index));
        }

        // GET: Pedidos/Precuenta/5 — consulta de la precuenta (solo Cajero).
        // Solo lectura: no crea ventas ni cambia estados; el cobro lo realiza Cobrar.
        [Authorize(Roles = "Cajero")]
        public async Task<IActionResult> Precuenta(int? id)
        {
            if (id == null) return RedirectToAction(nameof(Index));

            var pedido = await _context.Pedidos
                .AsNoTracking()
                .Include(p => p.Mesa)
                .Include(p => p.DetallePedidos).ThenInclude(d => d.Producto)
                .FirstOrDefaultAsync(p => p.IdPedido == id);

            if (pedido == null)
            {
                TempData["Error"] = "El pedido solicitado no existe.";
                return RedirectToAction(nameof(Index));
            }

            if (pedido.IdVenta != null || pedido.Estado == EstadoPedido.Cobrado)
            {
                TempData["Error"] = $"El pedido #{pedido.IdPedido} ya fue cobrado.";
                return RedirectToAction(nameof(Index));
            }

            if (pedido.Estado == EstadoPedido.Cancelado)
            {
                TempData["Error"] = $"No se puede ver la precuenta del pedido #{pedido.IdPedido} porque está cancelado.";
                return RedirectToAction(nameof(Index));
            }

            var modelo = new PrecuentaViewModel
            {
                IdPedido = pedido.IdPedido,
                NumeroMesa = pedido.Mesa?.Numero ?? pedido.IdMesa
            };

            foreach (var detalle in pedido.DetallePedidos)
            {
                var precio = detalle.Producto?.Precio ?? 0m;
                var subtotal = precio * detalle.Cantidad;
                modelo.Lineas.Add(new LineaPrecuenta
                {
                    NombreProducto = detalle.Producto?.NombreProducto ?? $"Producto #{detalle.IdProducto}",
                    Cantidad = detalle.Cantidad,
                    PrecioUnitario = precio,
                    Subtotal = subtotal
                });
                modelo.Total += subtotal;
            }

            return View(modelo);
        }

        // POST: Pedidos/Cobrar — crea la Venta con los precios vigentes del catálogo,
        // asocia el pedido a la venta y lo marca como Cobrado (solo Cajero).
        // Anti doble cobro: chequeo previo + UPDATE condicional con IdVenta IS NULL en transacción.
        [Authorize(Roles = "Cajero")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cobrar(int idPedido, MetodoPago metodoPago)
        {
            var pedido = await _context.Pedidos
                .Include(p => p.DetallePedidos)
                .FirstOrDefaultAsync(p => p.IdPedido == idPedido);

            if (pedido == null)
            {
                TempData["Error"] = "El pedido solicitado no existe.";
                return RedirectToAction(nameof(Index));
            }

            if (pedido.IdVenta != null || pedido.Estado == EstadoPedido.Cobrado)
            {
                TempData["Error"] = $"El pedido #{pedido.IdPedido} ya fue cobrado.";
                return RedirectToAction(nameof(Index));
            }

            if (pedido.Estado == EstadoPedido.Cancelado)
            {
                TempData["Error"] = $"No se puede cobrar el pedido #{pedido.IdPedido} porque está cancelado.";
                return RedirectToAction(nameof(Index));
            }

            if (pedido.DetallePedidos.Count == 0)
            {
                TempData["Error"] = $"El pedido #{pedido.IdPedido} no tiene productos para cobrar.";
                return RedirectToAction(nameof(Index));
            }

            int idUsuario = 0;
            if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out idUsuario))
            {
                TempData["Error"] = "No se pudo identificar al cajero que realiza el cobro.";
                return RedirectToAction(nameof(Index));
            }

            if (!Enum.IsDefined(metodoPago) || metodoPago == MetodoPago.NoRegistrado)
            {
                TempData["Error"] = "Debes seleccionar un método de pago para cobrar el pedido.";
                return RedirectToAction(nameof(Index));
            }

            var caja = await _context.Cajas
                .FirstOrDefaultAsync(c => c.IdUsuario == idUsuario && c.FechaCierre == null);
            if (caja == null)
            {
                TempData["Error"] = "Debes abrir una caja antes de cobrar pedidos.";
                return RedirectToAction(nameof(Index));
            }

            var ids = pedido.DetallePedidos.Select(d => d.IdProducto).Distinct().ToList();
            var productos = await _context.Productos
                .Where(p => ids.Contains(p.IdProducto))
                .ToDictionaryAsync(p => p.IdProducto);

            var venta = new Venta
            {
                Fecha = DateTime.Now,
                IdUsuario = idUsuario,
                MetodoPago = metodoPago,
                IdCaja = caja.IdCaja
            };

            decimal total = 0;
            string? error = null;
            foreach (var linea in pedido.DetallePedidos)
            {
                if (!productos.TryGetValue(linea.IdProducto, out var producto))
                {
                    error = "Uno de los productos del pedido ya no existe.";
                    break;
                }
                if (!producto.Estado)
                {
                    error = $"No se puede cobrar: el producto «{producto.NombreProducto}» ya no está disponible.";
                    break;
                }

                decimal subtotal = producto.Precio * linea.Cantidad;
                if (subtotal > MaximoDecimal)
                {
                    error = $"El subtotal del producto «{producto.NombreProducto}» supera el máximo permitido.";
                    break;
                }

                total += subtotal;
                if (total > MaximoDecimal)
                {
                    error = "El total de la venta supera el máximo permitido.";
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

            if (error != null)
            {
                TempData["Error"] = error;
                return RedirectToAction(nameof(Index));
            }

            venta.Total = total;

            var consumo = await _inventario.PrepararConsumoAsync(
                venta.DetalleVentas.Select(detalle => (detalle.IdProducto, detalle.Cantidad)));
            if (!consumo.EsValido)
            {
                TempData["Error"] = consumo.Mensaje;
                return RedirectToAction(nameof(Index));
            }

            await using var transaccion = await _context.Database.BeginTransactionAsync();
            try
            {
                _context.Ventas.Add(venta);
                _inventario.AplicarConsumo(consumo);
                await _context.SaveChangesAsync();

                var afectados = await _context.Database.ExecuteSqlAsync(
                    $"UPDATE Pedidos SET IdVenta = {venta.IdVenta}, Estado = {(int)EstadoPedido.Cobrado} WHERE IdPedido = {pedido.IdPedido} AND IdVenta IS NULL");

                if (afectados == 0)
                {
                    await transaccion.RollbackAsync();
                    TempData["Error"] = $"El pedido #{pedido.IdPedido} ya fue cobrado.";
                    return RedirectToAction(nameof(Index));
                }

                await transaccion.CommitAsync();
            }
            catch
            {
                await transaccion.RollbackAsync();
                throw;
            }

            await NotificarCambioAsync($"El Pedido #{pedido.IdPedido} fue cobrado.");
            TempData["Mensaje"] = $"Pedido #{pedido.IdPedido} cobrado. Venta #{venta.IdVenta} registrada: {venta.DetalleVentas.Count} producto(s), total S/ {venta.Total:N2}.";
            return RedirectToAction(nameof(Index));
        }

        private static bool EstaAbierto(EstadoPedido estado) =>
            estado is EstadoPedido.Pendiente or EstadoPedido.EnPreparacion or EstadoPedido.Listo
                or EstadoPedido.Entregado or EstadoPedido.CuentaSolicitada;

        private static string NombreEstado(EstadoPedido estado) => estado switch
        {
            EstadoPedido.EnPreparacion => "En preparación",
            EstadoPedido.CuentaSolicitada => "Cuenta solicitada",
            _ => estado.ToString()
        };

        private Task NotificarCambioAsync(string mensaje) =>
            _hubContext.Clients.All.SendAsync("PedidoActualizado", new
            {
                mensaje,
                fecha = DateTime.Now.ToString("O")
            });

        private void ValidarLimitesPedido(ICollection<LineaPedidoInput> detalles, int unidadesExistentes = 0)
        {
            if (detalles.Count > ReglasNegocio.MaxLineasPorPedido)
            {
                ModelState.AddModelError("", $"Un pedido no puede tener más de {ReglasNegocio.MaxLineasPorPedido} productos diferentes.");
            }

            var unidadesNuevas = detalles.Sum(d => (long)d.Cantidad);
            if (unidadesExistentes + unidadesNuevas > ReglasNegocio.MaxUnidadesPorPedido)
            {
                ModelState.AddModelError("", $"Un pedido no puede superar {ReglasNegocio.MaxUnidadesPorPedido} unidades en total.");
            }
        }

        private async Task<bool> MesaTienePedidoEnCurso(int idMesa) =>
            await _context.Pedidos.AnyAsync(p =>
                p.IdMesa == idMesa &&
                p.Estado != EstadoPedido.Cancelado &&
                p.Estado != EstadoPedido.Cobrado);

        private async Task CargarMesasYProductos()
        {
            var mesasOcupadas = await _context.Pedidos
                .Where(p => p.Estado != EstadoPedido.Cancelado &&
                            p.Estado != EstadoPedido.Cobrado)
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
