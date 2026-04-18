using ElohimShop.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ElohimShop.Infrastructure.Persistence;

public class ElohimShopDbContext : DbContext
{
    public ElohimShopDbContext(DbContextOptions<ElohimShopDbContext> options) : base(options)
    {
    }

    public virtual DbSet<Usuario> Usuarios { get; set; } = null!;
    public virtual DbSet<ClientePerfil> ClientesPerfil { get; set; } = null!;
    public virtual DbSet<AdministradorPerfil> AdministradoresPerfil { get; set; } = null!;
    public virtual DbSet<Marca> Marcas { get; set; } = null!;
    public virtual DbSet<Categoria> Categorias { get; set; } = null!;
    public virtual DbSet<MetodoPago> MetodosPago { get; set; } = null!;
    public virtual DbSet<Consulta> Consultas { get; set; } = null!;
    public virtual DbSet<Producto> Productos { get; set; } = null!;
    public virtual DbSet<Reservacion> Reservaciones { get; set; } = null!;
    public virtual DbSet<DetalleReservacion> DetallesReservacion { get; set; } = null!;
    public virtual DbSet<Venta> Ventas { get; set; } = null!;
    public virtual DbSet<TokenRevocado> TokensRevocados { get; set; } = null!;
    public virtual DbSet<Carrito> Carritos { get; set; } = null!;
    public virtual DbSet<ArticuloCarrito> ArticulosCarrito { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ElohimShopDbContext).Assembly);
    }
}
