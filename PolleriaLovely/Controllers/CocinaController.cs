using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PolleriaLovely.Data;
using PolleriaLovely.Models;

namespace PolleriaLovely.Controllers
{
    [Authorize(Roles = "Administrador,Cocinero")]
    public class CocinaController : Controller
    {
        private readonly BibliotecaDbContext _context;

        public CocinaController(BibliotecaDbContext context)
        {
            _context = context;
        }

        // GET: Cocina — muestra los pedidos Pendientes y EnPreparacion
        public async Task<IActionResult> Index()
        {
            var pedidosCocina = await _context.Pedidos
                .AsNoTracking()
                .Include(p => p.Mesa)
                .Include(p => p.Usuario)
                .Include(p => p.DetallePedidos)
                    .ThenInclude(d => d.Producto)
                .Where(p => p.Estado == EstadoPedido.Pendiente || p.Estado == EstadoPedido.EnPreparacion)
                .OrderBy(p => p.Fecha) // Los más antiguos arriba (orden de llegada)
                .ToListAsync();

            return View(pedidosCocina);
        }

        // POST: Cocina/CambiarEstado
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CambiarEstado(int idPedido, EstadoPedido nuevoEstado)
        {
            if (nuevoEstado != EstadoPedido.EnPreparacion && nuevoEstado != EstadoPedido.Listo)
            {
                TempData["Error"] = "Estado no permitido desde cocina.";
                return RedirectToAction(nameof(Index));
            }

            var pedido = await _context.Pedidos.FirstOrDefaultAsync(p => p.IdPedido == idPedido);
            if (pedido == null)
            {
                TempData["Error"] = "El pedido solicitado no existe.";
                return RedirectToAction(nameof(Index));
            }

            if (pedido.Estado != EstadoPedido.Pendiente && pedido.Estado != EstadoPedido.EnPreparacion)
            {
                TempData["Error"] = $"El pedido #{pedido.IdPedido} ya no está en cola de cocina.";
                return RedirectToAction(nameof(Index));
            }

            pedido.Estado = nuevoEstado;
            await _context.SaveChangesAsync();

            string textoEstado = nuevoEstado == EstadoPedido.EnPreparacion ? "En preparación" : "Listo";
            TempData["Mensaje"] = $"Pedido #{pedido.IdPedido} actualizado a «{textoEstado}».";

            return RedirectToAction(nameof(Index));
        }
    }
}