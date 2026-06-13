using ElohimShop.Domain.Platform;
using Microsoft.EntityFrameworkCore;
using PlatformUser = ElohimShop.Domain.Platform.User;

namespace ElohimShop.Infrastructure.Persistence;

public class PlatformDbContext : DbContext
{
    private readonly ITenantProvider _tenantProvider;

    public PlatformDbContext(
        DbContextOptions<PlatformDbContext> options,
        ITenantProvider tenantProvider) : base(options)
    {
        _tenantProvider = tenantProvider;
    }

    public DbSet<Tienda> Tiendas => Set<Tienda>();
    public DbSet<Sucursal> Sucursales => Set<Sucursal>();
    public DbSet<PlatformUser> Users => Set<PlatformUser>();
    public DbSet<Session> Sessions => Set<Session>();
    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<Verification> Verifications => Set<Verification>();
    public DbSet<Categoria> Categorias => Set<Categoria>();
    public DbSet<Producto> Productos => Set<Producto>();
    public DbSet<Inventario> Inventarios => Set<Inventario>();
    public DbSet<CarritoElemento> CarritoElementos => Set<CarritoElemento>();
    public DbSet<Reservacion> Reservaciones => Set<Reservacion>();
    public DbSet<DetalleReservacion> DetallesReservacion => Set<DetalleReservacion>();
    public DbSet<ReportePersonalizado> ReportesPersonalizados => Set<ReportePersonalizado>();
    public DbSet<CredencialesIntegracion> CredencialesIntegraciones => Set<CredencialesIntegracion>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Tienda>(builder =>
        {
            builder.ToTable("Tienda");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).HasMaxLength(255).HasColumnName("id");
            builder.Property(x => x.Nombre).HasMaxLength(100).IsRequired().HasColumnName("nombre");
            builder.Property(x => x.Slug).HasMaxLength(100).IsRequired().HasColumnName("slug");
            builder.Property(x => x.Estado).HasMaxLength(20).IsRequired().HasDefaultValue("activo").HasColumnName("estado");
            builder.Property(x => x.ConfiguracionVisual).HasColumnType("jsonb").HasColumnName("configuracion_visual");
            builder.Property(x => x.FechaCreacion).HasColumnType("timestamp with time zone").HasDefaultValueSql("NOW()").HasColumnName("fecha_creacion");
            builder.HasIndex(x => x.Slug).IsUnique();
        });

        modelBuilder.Entity<Sucursal>(builder =>
        {
            builder.ToTable("Sucursal");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).HasMaxLength(255).HasColumnName("id");
            builder.Property(x => x.TiendaId).HasMaxLength(255).IsRequired().HasColumnName("tienda_id");
            builder.Property(x => x.Nombre).HasMaxLength(100).IsRequired().HasColumnName("nombre");
            builder.Property(x => x.Direccion).HasColumnType("text").HasColumnName("direccion");
            builder.Property(x => x.Telefono).HasMaxLength(30).HasColumnName("telefono");
            builder.Property(x => x.FechaCreacion).HasColumnType("timestamp with time zone").HasDefaultValueSql("NOW()").HasColumnName("fecha_creacion");
            builder.HasOne(x => x.Tienda).WithMany(x => x.Sucursales).HasForeignKey(x => x.TiendaId).OnDelete(DeleteBehavior.Cascade);
            builder.HasIndex(x => x.TiendaId);
            builder.HasQueryFilter(x => x.TiendaId == _tenantProvider.GetTenantId());
        });

        modelBuilder.Entity<PlatformUser>(builder =>
        {
            builder.ToTable("user");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).HasMaxLength(255).HasColumnName("id");
            builder.Property(x => x.Name).HasMaxLength(255).IsRequired().HasColumnName("name");
            builder.Property(x => x.Email).HasMaxLength(255).IsRequired().HasColumnName("email");
            builder.Property(x => x.EmailVerified).IsRequired().HasColumnName("emailVerified");
            builder.Property(x => x.Image).HasMaxLength(500).HasColumnName("image");
            builder.Property(x => x.CreatedAt).HasColumnType("timestamp with time zone").HasColumnName("createdAt");
            builder.Property(x => x.UpdatedAt).HasColumnType("timestamp with time zone").HasColumnName("updatedAt");
            builder.Property(x => x.TiendaId).HasMaxLength(255).IsRequired().HasColumnName("tienda_id");
            builder.Property(x => x.TipoUsuario).HasMaxLength(30).IsRequired().HasColumnName("tipo_usuario");
            builder.Property(x => x.RolStaff).HasMaxLength(30).HasColumnName("rol_staff");
            builder.Property(x => x.Telefono).HasMaxLength(30).HasColumnName("telefono");
            builder.Property(x => x.StripeCustomerId).HasMaxLength(255).HasColumnName("stripe_customer_id");
            builder.HasOne(x => x.Tienda).WithMany(x => x.Users).HasForeignKey(x => x.TiendaId).OnDelete(DeleteBehavior.Cascade);
            builder.HasIndex(x => new { x.Email, x.TiendaId }).IsUnique();
            builder.HasQueryFilter(x => x.TiendaId == _tenantProvider.GetTenantId());
        });

        modelBuilder.Entity<Session>(builder =>
        {
            builder.ToTable("session");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).HasMaxLength(255).HasColumnName("id");
            builder.Property(x => x.ExpiresAt).HasColumnType("timestamp with time zone").HasColumnName("expiresAt");
            builder.Property(x => x.Token).HasMaxLength(255).IsRequired().HasColumnName("token");
            builder.Property(x => x.CreatedAt).HasColumnType("timestamp with time zone").HasColumnName("createdAt");
            builder.Property(x => x.UpdatedAt).HasColumnType("timestamp with time zone").HasColumnName("updatedAt");
            builder.Property(x => x.UserId).HasMaxLength(255).IsRequired().HasColumnName("userId");
            builder.Property(x => x.IpAddress).HasMaxLength(50).HasColumnName("ipAddress");
            builder.Property(x => x.UserAgent).HasColumnType("text").HasColumnName("userAgent");
            builder.HasOne(x => x.User).WithMany(x => x.Sessions).HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
            builder.HasIndex(x => x.Token).IsUnique();
        });

        modelBuilder.Entity<Account>(builder =>
        {
            builder.ToTable("account");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).HasMaxLength(255).HasColumnName("id");
            builder.Property(x => x.AccountId).HasMaxLength(255).IsRequired().HasColumnName("accountId");
            builder.Property(x => x.ProviderId).HasMaxLength(255).IsRequired().HasColumnName("providerId");
            builder.Property(x => x.UserId).HasMaxLength(255).IsRequired().HasColumnName("userId");
            builder.Property(x => x.AccessToken).HasColumnType("text").HasColumnName("accessToken");
            builder.Property(x => x.RefreshToken).HasColumnType("text").HasColumnName("refreshToken");
            builder.Property(x => x.IdToken).HasColumnType("text").HasColumnName("idToken");
            builder.Property(x => x.AccessTokenExpiresAt).HasColumnType("timestamp with time zone").HasColumnName("accessTokenExpiresAt");
            builder.Property(x => x.RefreshTokenExpiresAt).HasColumnType("timestamp with time zone").HasColumnName("refreshTokenExpiresAt");
            builder.Property(x => x.Scope).HasColumnType("text").HasColumnName("scope");
            builder.Property(x => x.Password).HasColumnType("text").HasColumnName("password");
            builder.Property(x => x.CreatedAt).HasColumnType("timestamp with time zone").HasColumnName("createdAt");
            builder.Property(x => x.UpdatedAt).HasColumnType("timestamp with time zone").HasColumnName("updatedAt");
            builder.HasOne(x => x.User).WithMany(x => x.Accounts).HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Verification>(builder =>
        {
            builder.ToTable("verification");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).HasMaxLength(255).HasColumnName("id");
            builder.Property(x => x.Identifier).HasMaxLength(255).IsRequired().HasColumnName("identifier");
            builder.Property(x => x.Value).HasMaxLength(255).IsRequired().HasColumnName("value");
            builder.Property(x => x.ExpiresAt).HasColumnType("timestamp with time zone").HasColumnName("expiresAt");
            builder.Property(x => x.CreatedAt).HasColumnType("timestamp with time zone").HasColumnName("createdAt");
            builder.Property(x => x.UpdatedAt).HasColumnType("timestamp with time zone").HasColumnName("updatedAt");
        });

        modelBuilder.Entity<Categoria>(builder =>
        {
            builder.ToTable("Categoria");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).HasMaxLength(255).HasColumnName("id");
            builder.Property(x => x.TiendaId).HasMaxLength(255).IsRequired().HasColumnName("tienda_id");
            builder.Property(x => x.Nombre).HasMaxLength(100).IsRequired().HasColumnName("nombre");
            builder.Property(x => x.Descripcion).HasColumnType("text").HasColumnName("descripcion");
            builder.Property(x => x.ImagenUrl).HasMaxLength(500).HasColumnName("imagen_url");
            builder.Property(x => x.Slug).HasMaxLength(100).IsRequired().HasColumnName("slug");
            builder.HasOne(x => x.Tienda).WithMany(x => x.Categorias).HasForeignKey(x => x.TiendaId).OnDelete(DeleteBehavior.Cascade);
            builder.HasIndex(x => new { x.Slug, x.TiendaId }).IsUnique();
            builder.HasQueryFilter(x => x.TiendaId == _tenantProvider.GetTenantId());
        });

        modelBuilder.Entity<Producto>(builder =>
        {
            builder.ToTable("Producto");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).HasMaxLength(255).HasColumnName("id");
            builder.Property(x => x.TiendaId).HasMaxLength(255).IsRequired().HasColumnName("tienda_id");
            builder.Property(x => x.CategoriaId).HasMaxLength(255).HasColumnName("categoria_id");
            builder.Property(x => x.Nombre).HasMaxLength(150).IsRequired().HasColumnName("nombre");
            builder.Property(x => x.Descripcion).HasColumnType("text").HasColumnName("descripcion");
            builder.Property(x => x.Sku).HasMaxLength(100).HasColumnName("sku");
            builder.Property(x => x.PrecioMayoreo).HasColumnType("numeric").HasPrecision(18, 2).HasColumnName("precio_mayoreo");
            builder.Property(x => x.PrecioDetalle).HasColumnType("numeric").HasPrecision(18, 2).HasColumnName("precio_detalle");
            builder.Property(x => x.ImagenUrl).HasMaxLength(500).HasColumnName("imagen_url");
            builder.Property(x => x.Publicado).IsRequired().HasDefaultValue(true).HasColumnName("publicado");
            builder.Property(x => x.FechaCreacion).HasColumnType("timestamp with time zone").HasDefaultValueSql("NOW()").HasColumnName("fecha_creacion");
            builder.HasOne(x => x.Tienda).WithMany(x => x.Productos).HasForeignKey(x => x.TiendaId).OnDelete(DeleteBehavior.Cascade);
            builder.HasOne(x => x.Categoria).WithMany(x => x.Productos).HasForeignKey(x => x.CategoriaId).OnDelete(DeleteBehavior.SetNull);
            builder.HasIndex(x => x.TiendaId);
            builder.HasIndex(x => x.Sku);
            builder.HasQueryFilter(x => x.TiendaId == _tenantProvider.GetTenantId());
        });

        modelBuilder.Entity<Inventario>(builder =>
        {
            builder.ToTable("Inventario");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).HasMaxLength(255).HasColumnName("id");
            builder.Property(x => x.TiendaId).HasMaxLength(255).IsRequired().HasColumnName("tienda_id");
            builder.Property(x => x.SucursalId).HasMaxLength(255).IsRequired().HasColumnName("sucursal_id");
            builder.Property(x => x.ProductoId).HasMaxLength(255).IsRequired().HasColumnName("producto_id");
            builder.Property(x => x.Stock).IsRequired().HasColumnName("stock");
            builder.HasOne(x => x.Tienda).WithMany(x => x.Inventarios).HasForeignKey(x => x.TiendaId).OnDelete(DeleteBehavior.Cascade);
            builder.HasOne(x => x.Sucursal).WithMany().HasForeignKey(x => x.SucursalId).OnDelete(DeleteBehavior.Cascade);
            builder.HasOne(x => x.Producto).WithMany(x => x.Inventarios).HasForeignKey(x => x.ProductoId).OnDelete(DeleteBehavior.Cascade);
            builder.HasIndex(x => new { x.SucursalId, x.ProductoId }).IsUnique();
            builder.HasQueryFilter(x => x.TiendaId == _tenantProvider.GetTenantId());
        });

        modelBuilder.Entity<CarritoElemento>(builder =>
        {
            builder.ToTable("CarritoElemento");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).HasMaxLength(255).HasColumnName("id");
            builder.Property(x => x.TiendaId).HasMaxLength(255).IsRequired().HasColumnName("tienda_id");
            builder.Property(x => x.UsuarioId).HasMaxLength(255).IsRequired().HasColumnName("usuario_id");
            builder.Property(x => x.ProductoId).HasMaxLength(255).IsRequired().HasColumnName("producto_id");
            builder.Property(x => x.Cantidad).IsRequired().HasColumnName("cantidad");
            builder.Property(x => x.FechaAdicion).HasColumnType("timestamp with time zone").HasDefaultValueSql("NOW()").HasColumnName("fecha_adicion");
            builder.HasOne(x => x.Tienda).WithMany(x => x.CarritoElementos).HasForeignKey(x => x.TiendaId).OnDelete(DeleteBehavior.Cascade);
            builder.HasOne(x => x.Usuario).WithMany(x => x.CarritoElementos).HasForeignKey(x => x.UsuarioId).OnDelete(DeleteBehavior.Cascade);
            builder.HasOne(x => x.Producto).WithMany(x => x.CarritoElementos).HasForeignKey(x => x.ProductoId).OnDelete(DeleteBehavior.Cascade);
            builder.HasQueryFilter(x => x.TiendaId == _tenantProvider.GetTenantId());
        });

        modelBuilder.Entity<Reservacion>(builder =>
        {
            builder.ToTable("Reservacion");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).HasMaxLength(255).HasColumnName("id");
            builder.Property(x => x.TiendaId).HasMaxLength(255).IsRequired().HasColumnName("tienda_id");
            builder.Property(x => x.SucursalId).HasMaxLength(255).IsRequired().HasColumnName("sucursal_id");
            builder.Property(x => x.UsuarioId).HasMaxLength(255).IsRequired().HasColumnName("usuario_id");
            builder.Property(x => x.MontoTotal).HasColumnType("numeric").HasPrecision(18, 2).HasColumnName("monto_total");
            builder.Property(x => x.EstadoPago).HasMaxLength(30).IsRequired().HasDefaultValue("pendiente").HasColumnName("estado_pago");
            builder.Property(x => x.EstadoDespacho).HasMaxLength(30).IsRequired().HasDefaultValue("procesando").HasColumnName("estado_despacho");
            builder.Property(x => x.StripeIntentId).HasMaxLength(255).HasColumnName("stripe_intent_id");
            builder.Property(x => x.FechaReserva).HasColumnType("timestamp with time zone").HasDefaultValueSql("NOW()").HasColumnName("fecha_reserva");
            builder.HasOne(x => x.Tienda).WithMany(x => x.Reservaciones).HasForeignKey(x => x.TiendaId).OnDelete(DeleteBehavior.Cascade);
            builder.HasOne(x => x.Sucursal).WithMany().HasForeignKey(x => x.SucursalId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.Usuario).WithMany(x => x.Reservaciones).HasForeignKey(x => x.UsuarioId).OnDelete(DeleteBehavior.Restrict);
            builder.HasMany(x => x.Detalles).WithOne(x => x.Reservacion).HasForeignKey(x => x.ReservacionId).OnDelete(DeleteBehavior.Cascade);
            builder.HasQueryFilter(x => x.TiendaId == _tenantProvider.GetTenantId());
        });

        modelBuilder.Entity<DetalleReservacion>(builder =>
        {
            builder.ToTable("DetalleReservacion");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).HasMaxLength(255).HasColumnName("id");
            builder.Property(x => x.ReservacionId).HasMaxLength(255).IsRequired().HasColumnName("reservacion_id");
            builder.Property(x => x.ProductoId).HasMaxLength(255).IsRequired().HasColumnName("producto_id");
            builder.Property(x => x.Cantidad).IsRequired().HasColumnName("cantidad");
            builder.Property(x => x.PrecioCobrado).HasColumnType("numeric").HasPrecision(18, 2).HasColumnName("precio_cobrado");
            builder.Property(x => x.Subtotal).HasColumnType("numeric").HasPrecision(18, 2).HasComputedColumnSql("cantidad * precio_cobrado", stored: true).HasColumnName("subtotal");
            builder.HasOne(x => x.Producto).WithMany(x => x.DetallesReservacion).HasForeignKey(x => x.ProductoId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ReportePersonalizado>(builder =>
        {
            builder.ToTable("ReportePersonalizado");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).HasMaxLength(255).HasColumnName("id");
            builder.Property(x => x.TiendaId).HasMaxLength(255).IsRequired().HasColumnName("tienda_id");
            builder.Property(x => x.Nombre).HasMaxLength(150).IsRequired().HasColumnName("nombre");
            builder.Property(x => x.Descripcion).HasColumnType("text").HasColumnName("descripcion");
            builder.Property(x => x.QuerySql).HasColumnType("text").IsRequired().HasColumnName("query_sql");
            builder.Property(x => x.CreadoPor).HasMaxLength(255).HasColumnName("creado_por");
            builder.Property(x => x.FechaCreacion).HasColumnType("timestamp with time zone").HasDefaultValueSql("NOW()").HasColumnName("fecha_creacion");
            builder.HasOne(x => x.Tienda).WithMany(x => x.ReportesPersonalizados).HasForeignKey(x => x.TiendaId).OnDelete(DeleteBehavior.Cascade);
            builder.HasQueryFilter(x => x.TiendaId == _tenantProvider.GetTenantId());
        });

        modelBuilder.Entity<CredencialesIntegracion>(builder =>
        {
            builder.ToTable("CredencialesIntegracion");
            builder.HasKey(x => x.TiendaId);
            builder.Property(x => x.TiendaId).HasMaxLength(255).HasColumnName("tienda_id");
            builder.Property(x => x.StripeSecretKey).HasColumnType("text").HasColumnName("stripe_secret_key");
            builder.Property(x => x.StripePublicKey).HasColumnType("text").HasColumnName("stripe_public_key");
            builder.Property(x => x.CloudinaryCloudName).HasMaxLength(100).HasColumnName("cloudinary_cloud_name");
            builder.Property(x => x.CloudinaryApiKey).HasMaxLength(100).HasColumnName("cloudinary_api_key");
            builder.Property(x => x.CloudinaryApiSecret).HasColumnType("text").HasColumnName("cloudinary_api_secret");
            builder.HasOne(x => x.Tienda).WithOne(x => x.CredencialesIntegracion).HasForeignKey<CredencialesIntegracion>(x => x.TiendaId).OnDelete(DeleteBehavior.Cascade);
            builder.HasQueryFilter(x => x.TiendaId == _tenantProvider.GetTenantId());
        });
    }
}