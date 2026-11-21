using Microsoft.EntityFrameworkCore;
using PolleriaLovely.Models;


namespace PolleriaLovely.Data
{
    public class BibliotecaDbContext : DbContext
    {
        public BibliotecaDbContext(DbContextOptions<BibliotecaDbContext> options) : base(options)
        {

        }
        public DbSet<Proveedor> Proveedores { get; set; }
        public DbSet<Insumo> Insumos { get; set; }
        public DbSet<Categoria> Categorias { get; set; }
        public DbSet<Producto> Productos { get; set; }
        public DbSet<Rol> Roles { get; set; }
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Venta> Ventas { get; set; }
        public DbSet<DetalleVenta> DetalleVentas { get; set; }
        public DbSet<Caja> Cajas { get; set; }
        public DbSet<Gasto> Gastos { get; set; }
        public DbSet<Mesa> Mesas { get; set; }
        public DbSet<Pedido> Pedidos { get; set; }
        public DbSet<DetallePedido> DetallePedidos { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            // Configuración de la relación entre Proveedor e Insumo
            modelBuilder.Entity<Insumo>()
                .HasOne(i => i.Proveedor)
                .WithMany(p => p.Insumos)
                .HasForeignKey(i => i.IdProveedor)
                .OnDelete(DeleteBehavior.Cascade);
            // Configuración de la relación entre Categoria y Producto
            modelBuilder.Entity<Producto>()
                .HasOne(p => p.Categoria)
                .WithMany(c => c.Productos)
                .HasForeignKey(p => p.IdCategoria)
                .OnDelete(DeleteBehavior.Restrict);
            // Configuración de la relación entre Rol y Usuario
            modelBuilder.Entity<Usuario>()
                .HasOne(u => u.Rol)
                .WithMany(r => r.Usuarios)
                .HasForeignKey(u => u.IdRol)
                .OnDelete(DeleteBehavior.Cascade);
            // Configuración de la relación entre Venta y Usuario
            modelBuilder.Entity<Venta>()
                .HasOne(v => v.Usuario)
                .WithMany(u => u.Ventas)
                .HasForeignKey(v => v.IdUsuario)
                .OnDelete(DeleteBehavior.Restrict);
            // Configuración de la relación entre Caja y Usuario
            modelBuilder.Entity<Caja>()
                .HasOne(c => c.Usuario)
                .WithMany()
                .HasForeignKey(c => c.IdUsuario)
                .OnDelete(DeleteBehavior.Restrict);
            // Las ventas históricas pueden no tener una caja asociada.
            modelBuilder.Entity<Venta>()
                .HasOne(v => v.Caja)
                .WithMany(c => c.Ventas)
                .HasForeignKey(v => v.IdCaja)
                .OnDelete(DeleteBehavior.Restrict);
            // Configuración de la relación entre Gasto y Usuario
            modelBuilder.Entity<Gasto>()
                .HasOne(g => g.Usuario)
                .WithMany()
                .HasForeignKey(g => g.IdUsuario)
                .OnDelete(DeleteBehavior.Restrict);
            // Configuración de la relación entre DetalleVenta y Venta
            modelBuilder.Entity<DetalleVenta>()
                .HasOne(d => d.Venta)
                .WithMany(v => v.DetalleVentas)
                .HasForeignKey(d => d.IdVenta)
                .OnDelete(DeleteBehavior.Cascade);
            // Configuración de la relación entre DetalleVenta y Producto
            modelBuilder.Entity<DetalleVenta>()
                .HasOne(d => d.Producto)
                .WithMany(p => p.DetalleVentas)
                .HasForeignKey(d => d.IdProducto)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Venta>()
                .Property(v => v.Total)
                .HasColumnType("decimal(10, 2)");
            modelBuilder.Entity<Venta>()
                .Property(v => v.Fecha)
                .HasDefaultValueSql("GETDATE()");
            modelBuilder.Entity<Caja>()
                .Property(c => c.MontoInicial)
                .HasColumnType("decimal(10, 2)");
            modelBuilder.Entity<Caja>()
                .Property(c => c.MontoContado)
                .HasColumnType("decimal(10, 2)");
            modelBuilder.Entity<Caja>()
                .Property(c => c.Diferencia)
                .HasColumnType("decimal(10, 2)");
            modelBuilder.Entity<Gasto>()
                .Property(g => g.Monto)
                .HasColumnType("decimal(10, 2)");
            modelBuilder.Entity<Gasto>()
                .Property(g => g.Fecha)
                .HasDefaultValueSql("GETDATE()");
            modelBuilder.Entity<DetalleVenta>()
                .Property(d => d.PrecioUnitario)
                .HasColumnType("decimal(10, 2)");
            modelBuilder.Entity<DetalleVenta>()
                .Property(d => d.Subtotal)
                .HasColumnType("decimal(10, 2)");
            // Configuración de la relación entre Pedido y Mesa
            modelBuilder.Entity<Pedido>()
                .HasOne(p => p.Mesa)
                .WithMany(m => m.Pedidos)
                .HasForeignKey(p => p.IdMesa)
                .OnDelete(DeleteBehavior.Restrict);
            // Configuración de la relación entre Pedido y Usuario
            modelBuilder.Entity<Pedido>()
                .HasOne(p => p.Usuario)
                .WithMany(u => u.Pedidos)
                .HasForeignKey(p => p.IdUsuario)
                .OnDelete(DeleteBehavior.Restrict);
            // Configuración de la relación entre Pedido y Venta (cobro)
            modelBuilder.Entity<Pedido>()
                .HasOne(p => p.Venta)
                .WithMany()
                .HasForeignKey(p => p.IdVenta)
                .OnDelete(DeleteBehavior.Restrict);
            // Configuración de la relación entre DetallePedido y Pedido
            modelBuilder.Entity<DetallePedido>()
                .HasOne(d => d.Pedido)
                .WithMany(p => p.DetallePedidos)
                .HasForeignKey(d => d.IdPedido)
                .OnDelete(DeleteBehavior.Cascade);
            // Configuración de la relación entre DetallePedido y Producto
            modelBuilder.Entity<DetallePedido>()
                .HasOne(d => d.Producto)
                .WithMany(p => p.DetallePedidos)
                .HasForeignKey(d => d.IdProducto)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Pedido>()
                .Property(p => p.Fecha)
                .HasDefaultValueSql("GETDATE()");
            modelBuilder.Entity<Pedido>()
                .Property(p => p.Estado)
                .HasDefaultValue(EstadoPedido.Pendiente);

            modelBuilder.Entity<Insumo>()
                .Property(i => i.Stock)
                .HasColumnType("decimal(10, 2)");
            modelBuilder.Entity<Insumo>()
                .Property(i => i.StockMinimo)
                .HasColumnType("decimal(10, 2)");
            modelBuilder.Entity<Producto>()
                .Property(p => p.Precio)
                .HasColumnType("decimal(10, 2)");

        }
    }
}
