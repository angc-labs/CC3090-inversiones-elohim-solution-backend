using ElohimShop.Domain.Entities;
using ElohimShop.Infrastructure.Persistence.Configurations;
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
    public virtual DbSet<CodigoRecuperacion> CodigosRecuperacion { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Aplicación explícita: en algunos entornos (p. ej. tests + InMemory) ApplyConfigurationsFromAssembly
        // no registraba tipos y dejaba entidades sin PK (IdReservacion / IdMetodoPago no siguen la convención "Id").
        modelBuilder.ApplyConfiguration(new AdministradorPerfilConfiguration());
        modelBuilder.ApplyConfiguration(new ArticuloCarritoConfiguration());
        modelBuilder.ApplyConfiguration(new CarritoConfiguration());
        modelBuilder.ApplyConfiguration(new CategoriaConfiguration());
        modelBuilder.ApplyConfiguration(new ClientePerfilConfiguration());
        modelBuilder.ApplyConfiguration(new ConsultaConfiguration());
        modelBuilder.ApplyConfiguration(new DetalleReservacionConfiguration());
        modelBuilder.ApplyConfiguration(new MarcaConfiguration());
        modelBuilder.ApplyConfiguration(new MetodoPagoConfiguration());
        modelBuilder.ApplyConfiguration(new ProductoConfiguration());
        modelBuilder.ApplyConfiguration(new ReservacionConfiguration());
        modelBuilder.ApplyConfiguration(new TokenRevocadoConfiguration());
        modelBuilder.ApplyConfiguration(new UsuarioConfiguration());
        modelBuilder.ApplyConfiguration(new VentaConfiguration());
        modelBuilder.ApplyConfiguration(new CodigoRecuperacionConfiguration());
    }
}
