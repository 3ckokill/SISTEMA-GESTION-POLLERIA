using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PolleriaLovely.Data;
using PolleriaLovely.Models;
using PolleriaLovely.ViewModels;

namespace PolleriaLovely.Controllers
{
    public class UsuarioController : Controller
    {
        private readonly BibliotecaDbContext _context;
        private readonly IPasswordHasher<Usuario> _hasher;

        public UsuarioController(
            BibliotecaDbContext context,
            IPasswordHasher<Usuario> hasher)
        {
            _context = context;
            _hasher = hasher;
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel modelo)
        {
            if (!ModelState.IsValid)
                return View(modelo);

            var usuario = await _context.Usuarios
                .Include(u => u.Rol)
                .SingleOrDefaultAsync(u =>
                    u.NombreUsuario == modelo.NombreUsuario &&
                    u.Estado);

            if (usuario == null || usuario.Rol == null)
            {
                ModelState.AddModelError(
                    string.Empty, "Usuario o contraseña incorrectos.");
                return View(modelo);
            }

            var resultado = _hasher.VerifyHashedPassword(
                usuario,
                usuario.Contrasena,
                modelo.Contrasena);

            if (resultado == PasswordVerificationResult.Failed)
            {
                ModelState.AddModelError(
                    string.Empty, "Usuario o contraseña incorrectos.");
                return View(modelo);
            }

            string? controlador = usuario.Rol.NombreRol switch
            {
                "Administrador" => "Administrador",
                "Cajero" => "Cajero",
                "Mozo" => "Mozo",
                _ => null
            };

            if (controlador == null)
            {
                ModelState.AddModelError(
                    string.Empty, "El rol no tiene un módulo asignado.");
                return View(modelo);
            }

            var reclamaciones = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier,
                    usuario.IdUsuario.ToString()),
                new(ClaimTypes.Name, usuario.NombreUsuario),
                new(ClaimTypes.Role, usuario.Rol.NombreRol)
            };

            var identidad = new ClaimsIdentity(
                reclamaciones,
                CookieAuthenticationDefaults.AuthenticationScheme);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identidad));

            return RedirectToAction("Index", controlador);
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult AccesoDenegado()
        {
            return View("AccesoDenegado");
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(
                CookieAuthenticationDefaults.AuthenticationScheme);

            return RedirectToAction(nameof(Login));
        }
    }
}