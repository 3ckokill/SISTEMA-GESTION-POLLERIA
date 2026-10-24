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
                .OnDelete(DeleteBehavior.Cascade);
            // Configuración de la relación entre Rol y Usuario
            modelBuilder.Entity<Usuario>()
                .HasOne(u => u.Rol)
                .WithMany(r => r.Usuarios)
                .HasForeignKey(u => u.IdRol)
                .OnDelete(DeleteBehavior.Cascade);

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
