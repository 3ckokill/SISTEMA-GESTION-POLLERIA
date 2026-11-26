using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PolleriaLovely.Data;
using PolleriaLovely.Models;

namespace PolleriaLovely.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class UsuariosController : Controller
    {
        private readonly BibliotecaDbContext _context;
        private readonly IPasswordHasher<Usuario> _hasher;

        public UsuariosController(
            BibliotecaDbContext context,
            IPasswordHasher<Usuario> hasher)
        {
            _context = context;
            _hasher = hasher;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _context.Usuarios
                .Include(u => u.Rol)
                .AsNoTracking()
                .OrderBy(u => u.NombreUsuario)
                .ToListAsync());
        }

        public async Task<IActionResult> Create()
        {
            ViewBag.IdRol = await CrearSelectRolesAsync(null);
            return View(new Usuario { Estado = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("NombreUsuario,Contrasena,Nombre,Apellido,Correo,Telefono,Estado,IdRol")] Usuario usuario)
        {
            ValidarContrasena(usuario);
            await ValidarUsuarioAsync(usuario, null);

            if (ModelState.IsValid)
            {
                usuario.Contrasena = _hasher.HashPassword(usuario, usuario.Contrasena);
                _context.Add(usuario);
                await _context.SaveChangesAsync();
                TempData["Mensaje"] =
                    $"El usuario «{usuario.NombreUsuario}» se registró correctamente.";
                return RedirectToAction(nameof(Index));
            }

            usuario.Contrasena = string.Empty;
            ViewBag.IdRol = await CrearSelectRolesAsync(usuario.IdRol);
            return View(usuario);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null) return NotFound();

            ViewBag.IdRol = await CrearSelectRolesAsync(usuario.IdRol);
            usuario.Contrasena = string.Empty;
            return View(usuario);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            [Bind("IdUsuario,NombreUsuario,Contrasena,Nombre,Apellido,Correo,Telefono,Estado,IdRol")] Usuario usuario)
        {
            if (id != usuario.IdUsuario) return NotFound();

            var existente = await _context.Usuarios
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.IdUsuario == id);
            if (existente == null) return NotFound();

            bool esPropio = UsuarioActualId() == id;
            string textoContrasena = usuario.Contrasena ?? string.Empty;
            bool cambiarContrasena = !string.IsNullOrWhiteSpace(textoContrasena);

            ModelState.Remove(nameof(Usuario.Contrasena));
            if (cambiarContrasena && textoContrasena.Length < 6)
            {
                ModelState.AddModelError(nameof(Usuario.Contrasena),
                    "La contraseña debe tener al menos 6 caracteres.");
            }

            if (esPropio && usuario.IdRol != existente.IdRol)
            {
                ModelState.AddModelError(nameof(Usuario.IdRol),
                    "No puede cambiar el rol de su propia cuenta.");
            }

            if (existente.Estado && !usuario.Estado)
            {
                if (esPropio)
                {
                    ModelState.AddModelError(nameof(Usuario.Estado),
                        "No puede desactivar su propia cuenta.");
                }
                else if (await EsUltimoAdministradorActivo(id))
                {
                    ModelState.AddModelError(nameof(Usuario.Estado),
                        "No se puede desactivar al último administrador activo.");
                }
            }

            await ValidarUsuarioAsync(usuario, id);

            if (ModelState.IsValid)
            {
                usuario.Contrasena = cambiarContrasena
                    ? _hasher.HashPassword(usuario, textoContrasena)
                    : existente.Contrasena;

                try
                {
                    _context.Update(usuario);
                    await _context.SaveChangesAsync();
                    TempData["Mensaje"] = "Los cambios se guardaron correctamente.";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UsuarioExists(usuario.IdUsuario)) return NotFound();
                    throw;
                }
            }

            usuario.Contrasena = string.Empty;
            ViewBag.IdRol = await CrearSelectRolesAsync(usuario.IdRol);
            return View(usuario);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var usuario = await _context.Usuarios
                .Include(u => u.Rol)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.IdUsuario == id);
            if (usuario == null) return NotFound();

            int pedidos = await _context.Pedidos.CountAsync(p => p.IdUsuario == id);
            int ventas = await _context.Ventas.CountAsync(v => v.IdUsuario == id);
            int cajas = await _context.Cajas.CountAsync(c => c.IdUsuario == id);
            int gastos = await _context.Gastos.CountAsync(g => g.IdUsuario == id);

            string? motivo = null;
            if (UsuarioActualId() == id)
            {
                motivo = "No puede eliminar su propia cuenta.";
            }
            else if (await EsUltimoAdministradorActivo(id.Value))
            {
                motivo = "No se puede eliminar al último administrador activo.";
            }
            else if (pedidos > 0 || ventas > 0 || cajas > 0 || gastos > 0)
            {
                var partes = new List<string>();
                if (pedidos > 0) partes.Add($"{pedidos} pedido(s)");
                if (ventas > 0) partes.Add($"{ventas} venta(s)");
                if (cajas > 0) partes.Add($"{cajas} caja(s)");
                if (gastos > 0) partes.Add($"{gastos} gasto(s)");
                motivo =
                    $"Este usuario tiene {string.Join(" y ", partes)} asociado(s); no se puede eliminar. Use la desactivación.";
            }

            ViewBag.MotivoBloqueo = motivo;
            return View(usuario);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int? id)
        {
            if (id == null) return NotFound();
            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.IdUsuario == id);
            if (usuario == null) return NotFound();

            if (UsuarioActualId() == id)
            {
                TempData["Error"] = "No puede eliminar su propia cuenta.";
                return RedirectToAction(nameof(Index));
            }

            if (await EsUltimoAdministradorActivo(id.Value))
            {
                TempData["Error"] = "No se puede eliminar al último administrador activo.";
                return RedirectToAction(nameof(Index));
            }

            int pedidos = await _context.Pedidos.CountAsync(p => p.IdUsuario == id);
            int ventas = await _context.Ventas.CountAsync(v => v.IdUsuario == id);
            int cajas = await _context.Cajas.CountAsync(c => c.IdUsuario == id);
            int gastos = await _context.Gastos.CountAsync(g => g.IdUsuario == id);
            if (pedidos > 0 || ventas > 0 || cajas > 0 || gastos > 0)
            {
                var partes = new List<string>();
                if (pedidos > 0) partes.Add($"{pedidos} pedido(s)");
                if (ventas > 0) partes.Add($"{ventas} venta(s)");
                if (cajas > 0) partes.Add($"{cajas} caja(s)");
                if (gastos > 0) partes.Add($"{gastos} gasto(s)");
                TempData["Error"] =
                    $"No se puede eliminar «{usuario.NombreUsuario}» porque tiene {string.Join(" y ", partes)} asociado(s). Use la desactivación.";
                return RedirectToAction(nameof(Index));
            }

            _context.Usuarios.Remove(usuario);
            await _context.SaveChangesAsync();
            TempData["Mensaje"] = "El usuario se eliminó correctamente.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CambiarEstado(int id)
        {
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null) return NotFound();

            if (UsuarioActualId() == id)
            {
                TempData["Error"] = "No puede cambiar el estado de su propia cuenta.";
                return RedirectToAction(nameof(Index));
            }

            if (usuario.Estado && await EsUltimoAdministradorActivo(id))
            {
                TempData["Error"] = "No se puede desactivar al último administrador activo.";
                return RedirectToAction(nameof(Index));
            }

            usuario.Estado = !usuario.Estado;
            await _context.SaveChangesAsync();
            TempData["Mensaje"] = usuario.Estado
                ? $"El usuario «{usuario.NombreUsuario}» se activó correctamente."
                : $"El usuario «{usuario.NombreUsuario}» se desactivó correctamente.";
            return RedirectToAction(nameof(Index));
        }

        private void ValidarContrasena(Usuario usuario)
        {
            ModelState.Remove(nameof(Usuario.Contrasena));
            if (string.IsNullOrWhiteSpace(usuario.Contrasena))
            {
                ModelState.AddModelError(nameof(Usuario.Contrasena),
                    "La contraseña es obligatoria.");
            }
            else if (usuario.Contrasena.Length < 6)
            {
                ModelState.AddModelError(nameof(Usuario.Contrasena),
                    "La contraseña debe tener al menos 6 caracteres.");
            }
        }

        private async Task ValidarUsuarioAsync(Usuario usuario, int? idExcluir)
        {
            if (!string.IsNullOrWhiteSpace(usuario.NombreUsuario) &&
                await NombreUsuarioExiste(usuario.NombreUsuario, idExcluir))
            {
                ModelState.AddModelError(nameof(Usuario.NombreUsuario),
                    "Ya existe un usuario con ese nombre.");
            }

            if (!string.IsNullOrWhiteSpace(usuario.Correo) &&
                await CorreoExiste(usuario.Correo, idExcluir))
            {
                ModelState.AddModelError(nameof(Usuario.Correo),
                    "Ya existe un usuario con ese correo.");
            }

            if (!await RolValido(usuario.IdRol))
            {
                ModelState.AddModelError(nameof(Usuario.IdRol),
                    "El rol seleccionado no es válido.");
            }
        }

        private async Task<bool> EsUltimoAdministradorActivo(int idExcluir)
        {
            var rolAdmin = await _context.Roles
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.NombreRol == "Administrador");
            if (rolAdmin == null) return false;

            bool esAdminActivo = await _context.Usuarios.AnyAsync(u =>
                u.IdUsuario == idExcluir &&
                u.Estado &&
                u.IdRol == rolAdmin.IdRol);
            if (!esAdminActivo) return false;

            return !await _context.Usuarios.AnyAsync(u =>
                u.Estado &&
                u.IdRol == rolAdmin.IdRol &&
                u.IdUsuario != idExcluir);
        }

        private int? UsuarioActualId()
        {
            var valor = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(valor, out var id) ? id : null;
        }

        private bool UsuarioExists(int id) =>
            _context.Usuarios.Any(e => e.IdUsuario == id);

        private Task<bool> NombreUsuarioExiste(string nombre, int? idExcluir) =>
            _context.Usuarios.AnyAsync(u =>
                u.NombreUsuario == nombre &&
                (!idExcluir.HasValue || u.IdUsuario != idExcluir));

        private Task<bool> CorreoExiste(string correo, int? idExcluir) =>
            _context.Usuarios.AnyAsync(u =>
                u.Correo == correo &&
                (!idExcluir.HasValue || u.IdUsuario != idExcluir));

        private Task<bool> RolValido(int idRol) =>
            _context.Roles.AnyAsync(r => r.IdRol == idRol && r.Estado);

        private async Task<SelectList> CrearSelectRolesAsync(int? seleccionado)
        {
            var roles = await _context.Roles
                .AsNoTracking()
                .Where(r => r.Estado)
                .OrderBy(r => r.NombreRol)
                .ToListAsync();

            if (seleccionado.HasValue && !roles.Any(r => r.IdRol == seleccionado.Value))
            {
                var extra = await _context.Roles
                    .AsNoTracking()
                    .FirstOrDefaultAsync(r => r.IdRol == seleccionado.Value);
                if (extra != null) roles.Insert(0, extra);
            }

            return new SelectList(
                roles, nameof(Rol.IdRol), nameof(Rol.NombreRol), seleccionado);
        }
    }
}
