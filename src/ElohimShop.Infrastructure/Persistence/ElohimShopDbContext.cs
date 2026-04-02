using ElohimShop.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ElohimShop.Infrastructure.Persistence;

public class ElohimShopDbContext : DbContext
{
    public ElohimShopDbContext(DbContextOptions<ElohimShopDbContext> options) : base(options)
    {
    }

    public virtual DbSet<Rol> Roles { get; set; } = null!;
    public virtual DbSet<Marca> Marcas { get; set; } = null!;
    public virtual DbSet<Categoria> Categorias { get; set; } = null!;
    public virtual DbSet<MetodoPago> MetodosPago { get; set; } = null!;
    public virtual DbSet<TipoCliente> TiposCliente { get; set; } = null!;
    public virtual DbSet<Cliente> Clientes { get; set; } = null!;
    public virtual DbSet<Administrador> Administradores { get; set; } = null!;
    public virtual DbSet<Consulta> Consultas { get; set; } = null!;
    public virtual DbSet<Producto> Productos { get; set; } = null!;
    public virtual DbSet<Reservacion> Reservaciones { get; set; } = null!;
    public virtual DbSet<DetalleReservacion> DetallesReservacion { get; set; } = null!;
    public virtual DbSet<Venta> Ventas { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ElohimShopDbContext).Assembly);
    }
}
