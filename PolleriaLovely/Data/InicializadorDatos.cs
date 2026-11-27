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
                },
                new Rol
                {
                    NombreRol = "Cocinero",
                    Descripcion = "Visualiza y prepara los pedidos en cocina.",
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
            // INICIALIZAR DATOS PRECARGADOS

            // 1. Proveedores
            if (!await context.Proveedores.AnyAsync(p => p.RucProveedor == "20100154308"))
            {
                context.Proveedores.AddRange(
                    new Proveedor
                    {
                        NombreProveedor = "Avícola San Fernando",
                        RucProveedor = "20100154308",
                        Telefono = "976543210",
                        Direccion = "Av. El Comercio 450",
                        Estado = true
                    },
                    new Proveedor
                    {
                        NombreProveedor = "Distribuidora El Fogón",
                        RucProveedor = "20458912341",
                        Telefono = "976543211",
                        Direccion = "Jr. Amalia Puga 215",
                        Estado = true
                    },
                    new Proveedor
                    {
                        NombreProveedor = "Bebidas del Norte",
                        RucProveedor = "20512398765",
                        Telefono = "976543212",
                        Direccion = "Av. Hoyos Rubio 890",
                        Estado = true
                    }
                );
                await context.SaveChangesAsync();
            }

            var prov1 = await context.Proveedores.FirstAsync(p => p.RucProveedor == "20100154308");
            var prov2 = await context.Proveedores.FirstAsync(p => p.RucProveedor == "20458912341");
            var prov3 = await context.Proveedores.FirstAsync(p => p.RucProveedor == "20512398765");

            // 2. Insumos
            if (!await context.Insumos.AnyAsync(i => i.NombreInsumo == "Pollo Entero"))
            {
                context.Insumos.AddRange(
                    new Insumo { NombreInsumo = "Pollo Entero", UnidadMedida = "kg", Stock = 120.00m, StockMinimo = 25.00m, Estado = true, IdProveedor = prov1.IdProveedor },
                    new Insumo { NombreInsumo = "Papa Blanca", UnidadMedida = "kg", Stock = 180.00m, StockMinimo = 40.00m, Estado = true, IdProveedor = prov2.IdProveedor },
                    new Insumo { NombreInsumo = "Aceite Vegetal", UnidadMedida = "l", Stock = 50.00m, StockMinimo = 15.00m, Estado = true, IdProveedor = prov2.IdProveedor },
                    new Insumo { NombreInsumo = "Ají Especial", UnidadMedida = "kg", Stock = 20.00m, StockMinimo = 5.00m, Estado = true, IdProveedor = prov2.IdProveedor },
                    new Insumo { NombreInsumo = "Gaseosa Personal", UnidadMedida = "und", Stock = 90.00m, StockMinimo = 20.00m, Estado = true, IdProveedor = prov3.IdProveedor },
                    new Insumo { NombreInsumo = "Chicha Morada", UnidadMedida = "l", Stock = 35.00m, StockMinimo = 10.00m, Estado = true, IdProveedor = prov3.IdProveedor }
                );
                await context.SaveChangesAsync();
            }

            // 3. Categorías
            if (!await context.Categorias.AnyAsync(c => c.NombreCategoria == "Pollos a la Brasa"))
            {
                context.Categorias.AddRange(
                    new Categoria
                    {
                        NombreCategoria = "Pollos a la Brasa",
                        Descripcion = "Pollo entero, medio pollo, cuarto de pollo y octavo de pollo con papas",
                        Estado = true
                    },
                    new Categoria
                    {
                        NombreCategoria = "Guarniciones",
                        Descripcion = "Porciones adicionales de papas fritas y ensaladas",
                        Estado = true
                    },
                    new Categoria
                    {
                        NombreCategoria = "Bebidas",
                        Descripcion = "Refrescos naturales, chicha morada y gaseosas heladas",
                        Estado = true
                    }
                );
                await context.SaveChangesAsync();
            }

            var catPollo = await context.Categorias.FirstAsync(c => c.NombreCategoria == "Pollos a la Brasa");
            var catGuarn = await context.Categorias.FirstAsync(c => c.NombreCategoria == "Guarniciones");
            var catBebida = await context.Categorias.FirstAsync(c => c.NombreCategoria == "Bebidas");

            // 4. Productos
            if (!await context.Productos.AnyAsync(p => p.NombreProducto == "Pollo a la Brasa Entero"))
            {
                context.Productos.AddRange(
                    new Producto
                    {
                        NombreProducto = "Pollo a la Brasa Entero",
                        Descripcion = "Pollo entero crocante con papas fritas y cremas",
                        Precio = 55.00m,
                        Estado = true,
                        IdCategoria = catPollo.IdCategoria
                    },
                    new Producto
                    {
                        NombreProducto = "Medio Pollo a la Brasa",
                        Descripcion = "Medio pollo dorado con papas fritas y ensalada",
                        Precio = 32.00m,
                        Estado = true,
                        IdCategoria = catPollo.IdCategoria
                    },
                    new Producto
                    {
                        NombreProducto = "1/4 de Pollo",
                        Descripcion = "Cuarto de pollo a la brasa con papas y salsas",
                        Precio = 18.00m,
                        Estado = true,
                        IdCategoria = catPollo.IdCategoria
                    },
                    new Producto
                    {
                        NombreProducto = "1/8 de Pollo",
                        Descripcion = "Octavo de pollo a la brasa con papas y salsas",
                        Precio = 10.00m,
                        Estado = true,
                        IdCategoria = catPollo.IdCategoria
                    },
                    new Producto
                    {
                        NombreProducto = "Porción de Papas Fritas",
                        Descripcion = "Porción generosa de papas fritas recién salidas",
                        Precio = 5.00m,
                        Estado = true,
                        IdCategoria = catGuarn.IdCategoria
                    },
                    new Producto
                    {
                        NombreProducto = "Jarra de Chicha Morada",
                        Descripcion = "Refrescante jarra de chicha morada natural",
                        Precio = 10.00m,
                        Estado = true,
                        IdCategoria = catBebida.IdCategoria
                    },
                    new Producto
                    {
                        NombreProducto = "Gaseosa Personal",
                        Descripcion = "Bebida gaseosa personal helada para acompañar",
                        Precio = 2.50m,
                        Estado = true,
                        IdCategoria = catBebida.IdCategoria
                    }
                );
                await context.SaveChangesAsync();
            }

            // GASTOS
            var usuarioGasto = await context.Usuarios.FirstOrDefaultAsync();
            if (usuarioGasto != null && !await context.Gastos.AnyAsync(g => g.Descripcion == "Compra de carbón para horno"))
            {
                context.Gastos.AddRange(
                    new Gasto
                    {
                        Descripcion = "Compra de carbón para horno",
                        Categoria = CategoriaGasto.Compras,
                        Monto = 45.00m,
                        Fecha = DateTime.Now.AddHours(-3),
                        MetodoPago = MetodoPago.Efectivo,
                        Observaciones = "Carbón para el turno del día",
                        Anulado = false,
                        IdUsuario = usuarioGasto.IdUsuario
                    },
                    new Gasto
                    {
                        Descripcion = "Pago de recibo de luz del local",
                        Categoria = CategoriaGasto.Servicios,
                        Monto = 95.00m,
                        Fecha = DateTime.Now.AddDays(-1),
                        MetodoPago = MetodoPago.YapePlin,
                        Observaciones = "Servicio eléctrico mensual",
                        Anulado = false,
                        IdUsuario = usuarioGasto.IdUsuario
                    },
                    new Gasto
                    {
                        Descripcion = "Mantenimiento de campana extractora",
                        Categoria = CategoriaGasto.Mantenimiento,
                        Monto = 60.00m,
                        Fecha = DateTime.Now.AddDays(-2),
                        MetodoPago = MetodoPago.Efectivo,
                        Observaciones = "Limpieza preventiva en cocina",
                        Anulado = false,
                        IdUsuario = usuarioGasto.IdUsuario
                    }
                );
                await context.SaveChangesAsync();
            }
        }
    }
}