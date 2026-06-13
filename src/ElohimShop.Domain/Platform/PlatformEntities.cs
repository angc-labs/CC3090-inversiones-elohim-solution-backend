namespace ElohimShop.Domain.Platform;

public interface ITenantScopedEntity
{
    string TiendaId { get; set; }
}

public class Tienda
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Nombre { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Estado { get; set; } = "activo";
    public string ConfiguracionVisual { get; set; } = "{}";
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

    public ICollection<Sucursal> Sucursales { get; set; } = new List<Sucursal>();
    public ICollection<User> Users { get; set; } = new List<User>();
    public ICollection<Categoria> Categorias { get; set; } = new List<Categoria>();
    public ICollection<Producto> Productos { get; set; } = new List<Producto>();
    public ICollection<Inventario> Inventarios { get; set; } = new List<Inventario>();
    public ICollection<CarritoElemento> CarritoElementos { get; set; } = new List<CarritoElemento>();
    public ICollection<Reservacion> Reservaciones { get; set; } = new List<Reservacion>();
    public ICollection<ReportePersonalizado> ReportesPersonalizados { get; set; } = new List<ReportePersonalizado>();
    public CredencialesIntegracion? CredencialesIntegracion { get; set; }
}

public class Sucursal : ITenantScopedEntity
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string TiendaId { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string? Direccion { get; set; }
    public string? Telefono { get; set; }
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

    public Tienda? Tienda { get; set; }
}

public class User : ITenantScopedEntity
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool EmailVerified { get; set; }
    public string? Image { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public string TiendaId { get; set; } = string.Empty;
    public string TipoUsuario { get; set; } = "cliente";
    public string? RolStaff { get; set; }
    public string? Telefono { get; set; }
    public string? StripeCustomerId { get; set; }

    public Tienda? Tienda { get; set; }
    public ICollection<Session> Sessions { get; set; } = new List<Session>();
    public ICollection<Account> Accounts { get; set; } = new List<Account>();
    public ICollection<CarritoElemento> CarritoElementos { get; set; } = new List<CarritoElemento>();
    public ICollection<Reservacion> Reservaciones { get; set; } = new List<Reservacion>();
}

public class Session
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public DateTime ExpiresAt { get; set; }
    public string Token { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public string UserId { get; set; } = string.Empty;
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }

    public User? User { get; set; }
}

public class Account
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string AccountId { get; set; } = string.Empty;
    public string ProviderId { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string? AccessToken { get; set; }
    public string? RefreshToken { get; set; }
    public string? IdToken { get; set; }
    public DateTime? AccessTokenExpiresAt { get; set; }
    public DateTime? RefreshTokenExpiresAt { get; set; }
    public string? Scope { get; set; }
    public string? Password { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public User? User { get; set; }
}

public class Verification
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Identifier { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class Categoria : ITenantScopedEntity
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string TiendaId { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public string? ImagenUrl { get; set; }
    public string Slug { get; set; } = string.Empty;

    public Tienda? Tienda { get; set; }
    public ICollection<Producto> Productos { get; set; } = new List<Producto>();
}

public class Producto : ITenantScopedEntity
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string TiendaId { get; set; } = string.Empty;
    public string? CategoriaId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public string? Sku { get; set; }
    public decimal PrecioMayoreo { get; set; }
    public decimal PrecioDetalle { get; set; }
    public string? ImagenUrl { get; set; }
    public bool Publicado { get; set; } = true;
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

    public Tienda? Tienda { get; set; }
    public Categoria? Categoria { get; set; }
    public ICollection<Inventario> Inventarios { get; set; } = new List<Inventario>();
    public ICollection<CarritoElemento> CarritoElementos { get; set; } = new List<CarritoElemento>();
    public ICollection<DetalleReservacion> DetallesReservacion { get; set; } = new List<DetalleReservacion>();
}

public class Inventario : ITenantScopedEntity
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string TiendaId { get; set; } = string.Empty;
    public string SucursalId { get; set; } = string.Empty;
    public string ProductoId { get; set; } = string.Empty;
    public int Stock { get; set; }

    public Tienda? Tienda { get; set; }
    public Sucursal? Sucursal { get; set; }
    public Producto? Producto { get; set; }
}

public class CarritoElemento : ITenantScopedEntity
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string TiendaId { get; set; } = string.Empty;
    public string UsuarioId { get; set; } = string.Empty;
    public string ProductoId { get; set; } = string.Empty;
    public int Cantidad { get; set; }
    public DateTime FechaAdicion { get; set; } = DateTime.UtcNow;

    public Tienda? Tienda { get; set; }
    public User? Usuario { get; set; }
    public Producto? Producto { get; set; }
}

public class Reservacion : ITenantScopedEntity
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string TiendaId { get; set; } = string.Empty;
    public string SucursalId { get; set; } = string.Empty;
    public string UsuarioId { get; set; } = string.Empty;
    public decimal MontoTotal { get; set; }
    public string EstadoPago { get; set; } = "pendiente";
    public string EstadoDespacho { get; set; } = "procesando";
    public string? StripeIntentId { get; set; }
    public DateTime FechaReserva { get; set; } = DateTime.UtcNow;

    public Tienda? Tienda { get; set; }
    public Sucursal? Sucursal { get; set; }
    public User? Usuario { get; set; }
    public ICollection<DetalleReservacion> Detalles { get; set; } = new List<DetalleReservacion>();
}

public class DetalleReservacion
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string ReservacionId { get; set; } = string.Empty;
    public string ProductoId { get; set; } = string.Empty;
    public int Cantidad { get; set; }
    public decimal PrecioCobrado { get; set; }
    public decimal Subtotal { get; private set; }

    public Reservacion? Reservacion { get; set; }
    public Producto? Producto { get; set; }
}

public class ReportePersonalizado : ITenantScopedEntity
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string TiendaId { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public string QuerySql { get; set; } = string.Empty;
    public string? CreadoPor { get; set; }
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

    public Tienda? Tienda { get; set; }
    public User? Autor { get; set; }
}

public class CredencialesIntegracion
{
    public string TiendaId { get; set; } = string.Empty;
    public string? StripeSecretKey { get; set; }
    public string? StripePublicKey { get; set; }
    public string? CloudinaryCloudName { get; set; }
    public string? CloudinaryApiKey { get; set; }
    public string? CloudinaryApiSecret { get; set; }

    public Tienda? Tienda { get; set; }
}