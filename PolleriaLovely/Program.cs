using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PolleriaLovely.Data;
using PolleriaLovely.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<BibliotecaDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IPasswordHasher<Usuario>,
    PasswordHasher<Usuario>>();

builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(opciones =>
    {
        opciones.LoginPath = "/Usuario/Login";
        opciones.AccessDeniedPath = "/Usuario/AccesoDenegado";
        opciones.Cookie.HttpOnly = true;
        opciones.Cookie.IsEssential = true;
        opciones.SlidingExpiration = true;
    });

var app = builder.Build();

// Crear los roles que falten.
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider
        .GetRequiredService<BibliotecaDbContext>();

    await InicializadorDatos.InicializarAsync(context);
}

// Crear usuarios de prueba solo en desarrollo.
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();

    var context = scope.ServiceProvider
        .GetRequiredService<BibliotecaDbContext>();

    var hasher = scope.ServiceProvider
        .GetRequiredService<IPasswordHasher<Usuario>>();

    var usuariosPrueba = new[]
    {
        new
        {
            NombreUsuario = "admin.prueba",
            Nombre = "Administrador",
            Apellido = "Prueba",
            Correo = "admin.prueba@lovely.local",
            Telefono = "999999999",
            NombreRol = "Administrador",
            Contrasena = "LovelyDemo2026!"
        },
        new
        {
            NombreUsuario = "cajero.prueba",
            Nombre = "Cajero",
            Apellido = "Prueba",
            Correo = "cajero.prueba@lovely.local",
            Telefono = "999999998",
            NombreRol = "Cajero",
            Contrasena = "LovelyCaja2026!"
        },
        new
        {
            NombreUsuario = "mozo.prueba",
            Nombre = "Mozo",
            Apellido = "Prueba",
            Correo = "mozo.prueba@lovely.local",
            Telefono = "999999997",
            NombreRol = "Mozo",
            Contrasena = "LovelyMozo2026!"
        }
    };

    foreach (var datos in usuariosPrueba)
    {
        var existeUsuario = await context.Usuarios
            .AnyAsync(u => u.NombreUsuario == datos.NombreUsuario);

        if (existeUsuario)
            continue;

        var rol = await context.Roles
            .FirstOrDefaultAsync(r => r.NombreRol == datos.NombreRol);

        if (rol == null)
            continue;

        var usuario = new Usuario
        {
            NombreUsuario = datos.NombreUsuario,
            Nombre = datos.Nombre,
            Apellido = datos.Apellido,
            Correo = datos.Correo,
            Telefono = datos.Telefono,
            Estado = true,
            IdRol = rol.IdRol
        };

        usuario.Contrasena = hasher.HashPassword(
            usuario,
            datos.Contrasena);

        context.Usuarios.Add(usuario);
    }

    await context.SaveChangesAsync();
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Usuario}/{action=Login}/{id?}")
    .WithStaticAssets();

app.Run();
