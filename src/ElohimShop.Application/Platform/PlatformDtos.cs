using System.Text.Json;

namespace ElohimShop.Application.Platform;

public sealed record TiendaDto(
    string Id,
    string Nombre,
    string Slug,
    string Estado,
    string ConfiguracionVisual,
    DateTime FechaCreacion,
    CredencialesIntegracionDto? CredencialesIntegracion);

public sealed record CredencialesIntegracionDto(
    string TiendaId,
    string? StripePublicKey,
    string? CloudinaryCloudName,
    string? CloudinaryApiKey);

public sealed record CrearTiendaRequest(string Nombre, string Slug);
public sealed record ActualizarConfiguracionVisualRequest(JsonElement ConfiguracionVisual);
public sealed record GuardarIntegracionesRequest(
    string? StripeSecretKey,
    string? StripePublicKey,
    string? CloudinaryCloudName,
    string? CloudinaryApiKey,
    string? CloudinaryApiSecret);

public sealed record MediaSignatureRequest(
    string PublicId,
    long? Timestamp = null,
    string? Folder = null);

public sealed record MediaSignatureResponse(
    string Signature,
    long Timestamp,
    string? ApiKey,
    string? CloudName);

public sealed record ProductoDto(
    string Id,
    string TiendaId,
    string? CategoriaId,
    string Nombre,
    string? Descripcion,
    string? Sku,
    decimal PrecioMayoreo,
    decimal PrecioDetalle,
    string? ImagenUrl,
    bool Publicado,
    DateTime FechaCreacion);

public sealed record CrearProductoRequest(
    string Nombre,
    decimal PrecioMayoreo,
    decimal PrecioDetalle,
    string? CategoriaId = null,
    string? Sku = null,
    string? Descripcion = null,
    string? ImagenUrl = null,
    bool Publicado = true);

public sealed record ActualizarProductoRequest(
    string Nombre,
    decimal PrecioMayoreo,
    decimal PrecioDetalle,
    string? CategoriaId = null,
    string? Sku = null,
    string? Descripcion = null,
    string? ImagenUrl = null,
    bool Publicado = true);

public sealed record InventarioDto(
    string Id,
    string TiendaId,
    string SucursalId,
    string ProductoId,
    int Stock,
    string? ProductoNombre = null,
    string? SucursalNombre = null);

public sealed record AjustarInventarioRequest(
    string SucursalId,
    string ProductoId,
    int Stock);

public sealed record CarritoElementoDto(
    string Id,
    string TiendaId,
    string UsuarioId,
    string ProductoId,
    int Cantidad,
    DateTime FechaAdicion,
    string? ProductoNombre = null,
    decimal? PrecioDetalle = null,
    decimal? Subtotal = null);

public sealed record AgregarCarritoRequest(string ProductoId, int Cantidad);

public sealed record ActualizarCarritoRequest(int Cantidad);

public sealed record CrearIntentoPagoRequest(string SucursalId);

public sealed record CrearIntentoPagoResponse(
    string PaymentIntentId,
    string ClientSecret,
    decimal MontoTotal,
    string Moneda);

public sealed record CrearReservacionRequest(
    string SucursalId,
    string? StripeIntentId = null);

public sealed record CambiarEstadoReservacionRequest(
    string? EstadoPago = null,
    string? EstadoDespacho = null);

public sealed record DetalleReservacionDto(
    string Id,
    string ReservacionId,
    string ProductoId,
    int Cantidad,
    decimal PrecioCobrado,
    decimal Subtotal,
    string? ProductoNombre = null);

public sealed record ReservacionDto(
    string Id,
    string TiendaId,
    string SucursalId,
    string UsuarioId,
    decimal MontoTotal,
    string EstadoPago,
    string EstadoDespacho,
    string? StripeIntentId,
    DateTime FechaReserva,
    IReadOnlyList<DetalleReservacionDto> Detalles);

public sealed record GuardarReporteRequest(
    string Nombre,
    string? Descripcion,
    string QuerySql);

public sealed record ReportePersonalizadoDto(
    string Id,
    string TiendaId,
    string Nombre,
    string? Descripcion,
    string QuerySql,
    string? CreadoPor,
    DateTime FechaCreacion);

public sealed record EjecutarRawReporteRequest(string QuerySql);

public sealed record SqlExecutionResult(
    IReadOnlyList<Dictionary<string, object?>> Rows);

public interface IPlatformService
{
    Task<TiendaDto> CrearTiendaAsync(CrearTiendaRequest request, CancellationToken cancellationToken);
    Task<bool> SlugDisponibleAsync(string slug, CancellationToken cancellationToken);
    Task<TiendaDto> ActualizarConfiguracionVisualAsync(ActualizarConfiguracionVisualRequest request, CancellationToken cancellationToken);
    Task<TiendaDto> GuardarIntegracionesAsync(GuardarIntegracionesRequest request, CancellationToken cancellationToken);

    Task<MediaSignatureResponse> GenerarFirmaMediaAsync(MediaSignatureRequest request, CancellationToken cancellationToken);
    Task<bool> EliminarMediaAsync(string publicId, CancellationToken cancellationToken);

    Task<IReadOnlyList<ProductoDto>> ListarProductosAsync(CancellationToken cancellationToken);
    Task<ProductoDto?> ObtenerProductoAsync(string id, CancellationToken cancellationToken);
    Task<ProductoDto> CrearProductoAsync(CrearProductoRequest request, CancellationToken cancellationToken);
    Task<ProductoDto?> ActualizarProductoAsync(string id, ActualizarProductoRequest request, CancellationToken cancellationToken);
    Task<bool> EliminarProductoAsync(string id, CancellationToken cancellationToken);

    Task<IReadOnlyList<InventarioDto>> ObtenerInventarioSucursalAsync(string sucursalId, CancellationToken cancellationToken);
    Task<InventarioDto> AjustarInventarioAsync(AjustarInventarioRequest request, CancellationToken cancellationToken);

    Task<IReadOnlyList<CarritoElementoDto>> ObtenerCarritoAsync(string usuarioId, CancellationToken cancellationToken);
    Task<CarritoElementoDto> AgregarArticuloAsync(string usuarioId, AgregarCarritoRequest request, CancellationToken cancellationToken);
    Task<CarritoElementoDto?> ActualizarArticuloAsync(string usuarioId, string id, ActualizarCarritoRequest request, CancellationToken cancellationToken);
    Task<bool> EliminarArticuloAsync(string usuarioId, string id, CancellationToken cancellationToken);

    Task<CrearIntentoPagoResponse> CrearIntentoPagoAsync(string usuarioId, CrearIntentoPagoRequest request, CancellationToken cancellationToken);
    Task<ReservacionDto> CrearReservacionAsync(string usuarioId, CrearReservacionRequest request, CancellationToken cancellationToken);
    Task<IReadOnlyList<ReservacionDto>> ObtenerMisComprasAsync(string usuarioId, CancellationToken cancellationToken);
    Task<IReadOnlyList<ReservacionDto>> ObtenerReservacionesStaffAsync(CancellationToken cancellationToken);
    Task<ReservacionDto?> CambiarEstadoReservacionAsync(string id, CambiarEstadoReservacionRequest request, CancellationToken cancellationToken);

    Task<SqlExecutionResult> EjecutarRawReporteAsync(EjecutarRawReporteRequest request, CancellationToken cancellationToken);
    Task<ReportePersonalizadoDto> GuardarReporteAsync(GuardarReporteRequest request, string? creadoPor, CancellationToken cancellationToken);
    Task<IReadOnlyList<ReportePersonalizadoDto>> ListarReportesAsync(CancellationToken cancellationToken);
    Task<SqlExecutionResult> CorrerReporteAsync(string id, CancellationToken cancellationToken);
}