using Microsoft.EntityFrameworkCore;
using PolleriaLovely.Models;

namespace PolleriaLovely.Data
{
    public static class InicializadorDatos
    {
        public static async Task InicializarAsync(
            BibliotecaDbContext context)
        {
            var rolesIniciales = new[]
            {
                new Rol
                {
                    NombreRol = "Administrador",
                    Descripcion = "Gestiona el sistema y sus usuarios.",
                    Estado = true
                },
                new Rol
                {
                    NombreRol = "Cajero",
                    Descripcion = "Gestiona las operaciones de caja.",
                    Estado = true
                },
                new Rol
                {
                    NombreRol = "Mozo",
                    Descripcion = "Atiende a los clientes y registra pedidos.",
                    Estado = true
                }
            };

            foreach (var rolInicial in rolesIniciales)
            {
                bool existe = await context.Roles
                    .AnyAsync(r =>
                        r.NombreRol == rolInicial.NombreRol);

                if (!existe)
                {
                    context.Roles.Add(rolInicial);
                }
            }

            await context.SaveChangesAsync();

            // Sembrar 10 mesas si la tabla está vacía
            if (!await context.Mesas.AnyAsync())
            {
                for (int numero = 1; numero <= 10; numero++)
                {
                    context.Mesas.Add(new Mesa
                    {
                        Numero = numero,
                        Estado = true
                    });
                }

                await context.SaveChangesAsync();
            }
        }
    }
}