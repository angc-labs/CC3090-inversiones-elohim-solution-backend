using System.Data.Common;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using ElohimShop.Application.Platform;
using ElohimShop.Domain.Platform;
using ElohimShop.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ElohimShop.Infrastructure.Platform;

public class PlatformService : IPlatformService
{
    private static readonly Regex UnsafeSqlPattern = new(@"\b(drop|delete|update|insert|alter|truncate|grant|revoke|create)\b", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private readonly PlatformDbContext _dbContext;
    private readonly ITenantProvider _tenantProvider;

    public PlatformService(PlatformDbContext dbContext, ITenantProvider tenantProvider)
    {
        _dbContext = dbContext;
        _tenantProvider = tenantProvider;
    }

    public async Task<TiendaDto> CrearTiendaAsync(CrearTiendaRequest request, CancellationToken cancellationToken)
    {
        var normalizedSlug = request.Slug.Trim().ToLowerInvariant();
        if (await _dbContext.Tiendas.IgnoreQueryFilters().AnyAsync(x => x.Slug == normalizedSlug, cancellationToken))
        {
            throw new InvalidOperationException("El slug ya está registrado.");
        }

        var tienda = new Tienda
        {
            Nombre = request.Nombre.Trim(),
            Slug = normalizedSlug,
            ConfiguracionVisual = "{}"
        };

        _dbContext.Tiendas.Add(tienda);
        _dbContext.CredencialesIntegraciones.Add(new CredencialesIntegracion { TiendaId = tienda.Id });
        await _dbContext.SaveChangesAsync(cancellationToken);

        return await MapTiendaAsync(tienda.Id, cancellationToken);
    }

    public Task<bool> SlugDisponibleAsync(string slug, CancellationToken cancellationToken)
    {
        var normalizedSlug = slug.Trim().ToLowerInvariant();
        return _dbContext.Tiendas.IgnoreQueryFilters().AllAsync(x => x.Slug != normalizedSlug, cancellationToken);
    }

    public async Task<TiendaDto> ActualizarConfiguracionVisualAsync(ActualizarConfiguracionVisualRequest request, CancellationToken cancellationToken)
    {
        var tienda = await GetTenantStoreAsync(cancellationToken);
        tienda.ConfiguracionVisual = request.ConfiguracionVisual.GetRawText();
        await _dbContext.SaveChangesAsync(cancellationToken);
        return await MapTiendaAsync(tienda.Id, cancellationToken);
    }

    public async Task<TiendaDto> GuardarIntegracionesAsync(GuardarIntegracionesRequest request, CancellationToken cancellationToken)
    {
        var tienda = await GetTenantStoreAsync(cancellationToken);
        var credenciales = await _dbContext.CredencialesIntegraciones.FirstOrDefaultAsync(x => x.TiendaId == tienda.Id, cancellationToken)
            ?? new CredencialesIntegracion { TiendaId = tienda.Id };

        credenciales.StripeSecretKey = request.StripeSecretKey;
        credenciales.StripePublicKey = request.StripePublicKey;
        credenciales.CloudinaryCloudName = request.CloudinaryCloudName;
        credenciales.CloudinaryApiKey = request.CloudinaryApiKey;
        credenciales.CloudinaryApiSecret = request.CloudinaryApiSecret;

        if (_dbContext.Entry(credenciales).State == EntityState.Detached)
        {
            _dbContext.CredencialesIntegraciones.Add(credenciales);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        return await MapTiendaAsync(tienda.Id, cancellationToken);
    }

    public async Task<MediaSignatureResponse> GenerarFirmaMediaAsync(MediaSignatureRequest request, CancellationToken cancellationToken)
    {
        var tenantId = RequireTenantId();
        var credenciales = await _dbContext.CredencialesIntegraciones.FirstOrDefaultAsync(x => x.TiendaId == tenantId, cancellationToken);
        if (credenciales is null ||
            string.IsNullOrWhiteSpace(credenciales.CloudinaryApiSecret) ||
            string.IsNullOrWhiteSpace(credenciales.CloudinaryApiKey) ||
            string.IsNullOrWhiteSpace(credenciales.CloudinaryCloudName))
        {
            throw new InvalidOperationException("Las credenciales de Cloudinary no están configuradas.");
        }

        var timestamp = request.Timestamp ?? DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var payload = $"public_id={request.PublicId}&timestamp={timestamp}&{credenciales.CloudinaryApiSecret}";
        var signatureBytes = SHA256.HashData(Encoding.UTF8.GetBytes(payload));

        return new MediaSignatureResponse(
            Signature: Convert.ToHexString(signatureBytes).ToLowerInvariant(),
            Timestamp: timestamp,
            ApiKey: credenciales.CloudinaryApiKey,
            CloudName: credenciales.CloudinaryCloudName);
    }

    public Task<bool> EliminarMediaAsync(string publicId, CancellationToken cancellationToken)
    {
        return Task.FromResult(!string.IsNullOrWhiteSpace(publicId));
    }

    public async Task<IReadOnlyList<ProductoDto>> ListarProductosAsync(CancellationToken cancellationToken)
    {
        return await _dbContext.Productos
            .AsNoTracking()
            .OrderBy(x => x.Nombre)
            .Select(x => new ProductoDto(
                x.Id,
                x.TiendaId,
                x.CategoriaId,
                x.Nombre,
                x.Descripcion,
                x.Sku,
                x.PrecioMayoreo,
                x.PrecioDetalle,
                x.ImagenUrl,
                x.Publicado,
                x.FechaCreacion))
            .ToListAsync(cancellationToken);
    }

    public async Task<ProductoDto?> ObtenerProductoAsync(string id, CancellationToken cancellationToken)
    {
        return await _dbContext.Productos
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new ProductoDto(
                x.Id,
                x.TiendaId,
                x.CategoriaId,
                x.Nombre,
                x.Descripcion,
                x.Sku,
                x.PrecioMayoreo,
                x.PrecioDetalle,
                x.ImagenUrl,
                x.Publicado,
                x.FechaCreacion))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<ProductoDto> CrearProductoAsync(CrearProductoRequest request, CancellationToken cancellationToken)
    {
        var producto = new Producto
        {
            TiendaId = RequireTenantId(),
            CategoriaId = request.CategoriaId,
            Nombre = request.Nombre.Trim(),
            Descripcion = request.Descripcion,
            Sku = request.Sku,
            PrecioMayoreo = request.PrecioMayoreo,
            PrecioDetalle = request.PrecioDetalle,
            ImagenUrl = request.ImagenUrl,
            Publicado = request.Publicado
        };

        _dbContext.Productos.Add(producto);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return await ObtenerProductoAsync(producto.Id, cancellationToken) ?? throw new InvalidOperationException("No se pudo crear el producto.");
    }

    public async Task<ProductoDto?> ActualizarProductoAsync(string id, ActualizarProductoRequest request, CancellationToken cancellationToken)
    {
        var producto = await _dbContext.Productos.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (producto is null)
        {
            return null;
        }

        producto.Nombre = request.Nombre.Trim();
        producto.CategoriaId = request.CategoriaId;
        producto.Descripcion = request.Descripcion;
        producto.Sku = request.Sku;
        producto.PrecioMayoreo = request.PrecioMayoreo;
        producto.PrecioDetalle = request.PrecioDetalle;
        producto.ImagenUrl = request.ImagenUrl;
        producto.Publicado = request.Publicado;
        await _dbContext.SaveChangesAsync(cancellationToken);
        return await ObtenerProductoAsync(id, cancellationToken);
    }

    public async Task<bool> EliminarProductoAsync(string id, CancellationToken cancellationToken)
    {
        var producto = await _dbContext.Productos.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (producto is null)
        {
            return false;
        }

        _dbContext.Productos.Remove(producto);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<IReadOnlyList<InventarioDto>> ObtenerInventarioSucursalAsync(string sucursalId, CancellationToken cancellationToken)
    {
        var tenantId = RequireTenantId();
        return await _dbContext.Inventarios
            .AsNoTracking()
            .Where(x => x.TiendaId == tenantId && x.SucursalId == sucursalId)
            .Select(x => new InventarioDto(
                x.Id,
                x.TiendaId,
                x.SucursalId,
                x.ProductoId,
                x.Stock,
                x.Producto!.Nombre,
                x.Sucursal!.Nombre))
            .ToListAsync(cancellationToken);
    }

    public async Task<InventarioDto> AjustarInventarioAsync(AjustarInventarioRequest request, CancellationToken cancellationToken)
    {
        var tenantId = RequireTenantId();
        var inventario = await _dbContext.Inventarios.FirstOrDefaultAsync(
            x => x.TiendaId == tenantId && x.SucursalId == request.SucursalId && x.ProductoId == request.ProductoId,
            cancellationToken);

        if (inventario is null)
        {
            inventario = new Inventario
            {
                TiendaId = tenantId,
                SucursalId = request.SucursalId,
                ProductoId = request.ProductoId,
                Stock = request.Stock
            };
            _dbContext.Inventarios.Add(inventario);
        }
        else
        {
            inventario.Stock = request.Stock;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        return new InventarioDto(inventario.Id, inventario.TiendaId, inventario.SucursalId, inventario.ProductoId, inventario.Stock);
    }

    public async Task<IReadOnlyList<CarritoElementoDto>> ObtenerCarritoAsync(string usuarioId, CancellationToken cancellationToken)
    {
        var tenantId = RequireTenantId();
        return await _dbContext.CarritoElementos
            .AsNoTracking()
            .Where(x => x.TiendaId == tenantId && x.UsuarioId == usuarioId)
            .Include(x => x.Producto)
            .OrderByDescending(x => x.FechaAdicion)
            .Select(x => new CarritoElementoDto(
                x.Id,
                x.TiendaId,
                x.UsuarioId,
                x.ProductoId,
                x.Cantidad,
                x.FechaAdicion,
                x.Producto!.Nombre,
                x.Producto!.PrecioDetalle,
                x.Producto!.PrecioDetalle * x.Cantidad))
            .ToListAsync(cancellationToken);
    }

    public async Task<CarritoElementoDto> AgregarArticuloAsync(string usuarioId, AgregarCarritoRequest request, CancellationToken cancellationToken)
    {
        var tenantId = RequireTenantId();
        var item = await _dbContext.CarritoElementos.FirstOrDefaultAsync(
            x => x.TiendaId == tenantId && x.UsuarioId == usuarioId && x.ProductoId == request.ProductoId,
            cancellationToken);

        if (item is null)
        {
            item = new CarritoElemento
            {
                TiendaId = tenantId,
                UsuarioId = usuarioId,
                ProductoId = request.ProductoId,
                Cantidad = request.Cantidad,
                FechaAdicion = DateTime.UtcNow
            };
            _dbContext.CarritoElementos.Add(item);
        }
        else
        {
            item.Cantidad += request.Cantidad;
            item.FechaAdicion = DateTime.UtcNow;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        return await MapCarritoAsync(item.Id, cancellationToken) ?? throw new InvalidOperationException("No se pudo agregar el artículo al carrito.");
    }

    public async Task<CarritoElementoDto?> ActualizarArticuloAsync(string usuarioId, string id, ActualizarCarritoRequest request, CancellationToken cancellationToken)
    {
        var tenantId = RequireTenantId();
        var item = await _dbContext.CarritoElementos.FirstOrDefaultAsync(x => x.Id == id && x.UsuarioId == usuarioId && x.TiendaId == tenantId, cancellationToken);
        if (item is null)
        {
            return null;
        }

        item.Cantidad = request.Cantidad;
        await _dbContext.SaveChangesAsync(cancellationToken);
        return await MapCarritoAsync(item.Id, cancellationToken);
    }

    public async Task<bool> EliminarArticuloAsync(string usuarioId, string id, CancellationToken cancellationToken)
    {
        var tenantId = RequireTenantId();
        var item = await _dbContext.CarritoElementos.FirstOrDefaultAsync(x => x.Id == id && x.UsuarioId == usuarioId && x.TiendaId == tenantId, cancellationToken);
        if (item is null)
        {
            return false;
        }

        _dbContext.CarritoElementos.Remove(item);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<CrearIntentoPagoResponse> CrearIntentoPagoAsync(string usuarioId, CrearIntentoPagoRequest request, CancellationToken cancellationToken)
    {
        var carrito = await ObtenerCarritoAsync(usuarioId, cancellationToken);
        if (carrito.Count == 0)
        {
            throw new InvalidOperationException("El carrito está vacío.");
        }

        var montoTotal = carrito.Sum(item => item.Subtotal ?? 0m);
        var paymentIntentId = $"pi_{Guid.NewGuid():N}";
        return new CrearIntentoPagoResponse(paymentIntentId, $"secret_{paymentIntentId}", montoTotal, "gtq");
    }

    public async Task<ReservacionDto> CrearReservacionAsync(string usuarioId, CrearReservacionRequest request, CancellationToken cancellationToken)
    {
        var tenantId = RequireTenantId();
        var carrito = await _dbContext.CarritoElementos
            .Include(x => x.Producto)
            .Where(x => x.TiendaId == tenantId && x.UsuarioId == usuarioId)
            .ToListAsync(cancellationToken);

        if (carrito.Count == 0)
        {
            throw new InvalidOperationException("El carrito está vacío.");
        }

        var reservacion = new Reservacion
        {
            TiendaId = tenantId,
            SucursalId = request.SucursalId,
            UsuarioId = usuarioId,
            MontoTotal = carrito.Sum(item => ObtenerPrecio(item) * item.Cantidad),
            EstadoPago = string.IsNullOrWhiteSpace(request.StripeIntentId) ? "pendiente" : "pagado",
            EstadoDespacho = "procesando",
            StripeIntentId = request.StripeIntentId,
            FechaReserva = DateTime.UtcNow
        };

        foreach (var item in carrito)
        {
            reservacion.Detalles.Add(new DetalleReservacion
            {
                ProductoId = item.ProductoId,
                Cantidad = item.Cantidad,
                PrecioCobrado = ObtenerPrecio(item)
            });
        }

        _dbContext.Reservaciones.Add(reservacion);
        _dbContext.CarritoElementos.RemoveRange(carrito);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return await MapReservacionAsync(reservacion.Id, cancellationToken) ?? throw new InvalidOperationException("No se pudo crear la reservación.");
    }

    public async Task<IReadOnlyList<ReservacionDto>> ObtenerMisComprasAsync(string usuarioId, CancellationToken cancellationToken)
    {
        var tenantId = RequireTenantId();
        var reservaciones = await _dbContext.Reservaciones
            .AsNoTracking()
            .Include(x => x.Detalles)
            .ThenInclude(x => x.Producto)
            .Where(x => x.TiendaId == tenantId && x.UsuarioId == usuarioId)
            .OrderByDescending(x => x.FechaReserva)
            .ToListAsync(cancellationToken);

        return reservaciones.Select(MapReservacion).ToList();
    }

    public async Task<IReadOnlyList<ReservacionDto>> ObtenerReservacionesStaffAsync(CancellationToken cancellationToken)
    {
        var tenantId = RequireTenantId();
        var reservaciones = await _dbContext.Reservaciones
            .AsNoTracking()
            .Include(x => x.Detalles)
            .ThenInclude(x => x.Producto)
            .Where(x => x.TiendaId == tenantId)
            .OrderByDescending(x => x.FechaReserva)
            .ToListAsync(cancellationToken);

        return reservaciones.Select(MapReservacion).ToList();
    }

    public async Task<ReservacionDto?> CambiarEstadoReservacionAsync(string id, CambiarEstadoReservacionRequest request, CancellationToken cancellationToken)
    {
        var tenantId = RequireTenantId();
        var reservacion = await _dbContext.Reservaciones
            .Include(x => x.Detalles)
            .ThenInclude(x => x.Producto)
            .FirstOrDefaultAsync(x => x.Id == id && x.TiendaId == tenantId, cancellationToken);

        if (reservacion is null)
        {
            return null;
        }

        if (!string.IsNullOrWhiteSpace(request.EstadoPago))
        {
            reservacion.EstadoPago = request.EstadoPago.Trim();
        }

        if (!string.IsNullOrWhiteSpace(request.EstadoDespacho))
        {
            reservacion.EstadoDespacho = request.EstadoDespacho.Trim();
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        return MapReservacion(reservacion);
    }

    public async Task<SqlExecutionResult> EjecutarRawReporteAsync(EjecutarRawReporteRequest request, CancellationToken cancellationToken)
    {
        ValidarSqlSeleccion(request.QuerySql);

        var connection = _dbContext.Database.GetDbConnection();
        await connection.OpenAsync(cancellationToken);
        try
        {
            await using var command = connection.CreateCommand();
            command.CommandText = request.QuerySql;
            await using var reader = await command.ExecuteReaderAsync(cancellationToken);

            var rows = new List<Dictionary<string, object?>>();
            while (await reader.ReadAsync(cancellationToken))
            {
                var row = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
                for (var i = 0; i < reader.FieldCount; i++)
                {
                    row[reader.GetName(i)] = reader.IsDBNull(i) ? null : reader.GetValue(i);
                }

                rows.Add(row);
            }

            return new SqlExecutionResult(rows);
        }
        finally
        {
            await connection.CloseAsync();
        }
    }

    public async Task<ReportePersonalizadoDto> GuardarReporteAsync(GuardarReporteRequest request, string? creadoPor, CancellationToken cancellationToken)
    {
        ValidarSqlSeleccion(request.QuerySql);

        var reporte = new ReportePersonalizado
        {
            TiendaId = RequireTenantId(),
            Nombre = request.Nombre.Trim(),
            Descripcion = request.Descripcion,
            QuerySql = request.QuerySql.Trim(),
            CreadoPor = creadoPor,
            FechaCreacion = DateTime.UtcNow
        };

        _dbContext.ReportesPersonalizados.Add(reporte);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return MapReporte(reporte);
    }

    public async Task<IReadOnlyList<ReportePersonalizadoDto>> ListarReportesAsync(CancellationToken cancellationToken)
    {
        var tenantId = RequireTenantId();
        return await _dbContext.ReportesPersonalizados
            .AsNoTracking()
            .Where(x => x.TiendaId == tenantId)
            .OrderByDescending(x => x.FechaCreacion)
            .Select(x => new ReportePersonalizadoDto(x.Id, x.TiendaId, x.Nombre, x.Descripcion, x.QuerySql, x.CreadoPor, x.FechaCreacion))
            .ToListAsync(cancellationToken);
    }

    public async Task<SqlExecutionResult> CorrerReporteAsync(string id, CancellationToken cancellationToken)
    {
        var tenantId = RequireTenantId();
        var reporte = await _dbContext.ReportesPersonalizados.FirstOrDefaultAsync(x => x.Id == id && x.TiendaId == tenantId, cancellationToken)
            ?? throw new InvalidOperationException("Reporte no encontrado.");

        return await EjecutarRawReporteAsync(new EjecutarRawReporteRequest(reporte.QuerySql), cancellationToken);
    }

    private async Task<TiendaDto> MapTiendaAsync(string tiendaId, CancellationToken cancellationToken)
    {
        var tienda = await _dbContext.Tiendas.AsNoTracking().FirstAsync(x => x.Id == tiendaId, cancellationToken);
        var credenciales = await _dbContext.CredencialesIntegraciones.AsNoTracking().FirstOrDefaultAsync(x => x.TiendaId == tiendaId, cancellationToken);
        return new TiendaDto(
            tienda.Id,
            tienda.Nombre,
            tienda.Slug,
            tienda.Estado,
            tienda.ConfiguracionVisual,
            tienda.FechaCreacion,
            credenciales is null ? null : new CredencialesIntegracionDto(
                credenciales.TiendaId,
                credenciales.StripePublicKey,
                credenciales.CloudinaryCloudName,
                credenciales.CloudinaryApiKey));
    }

    private async Task<CarritoElementoDto?> MapCarritoAsync(string itemId, CancellationToken cancellationToken)
    {
        return await _dbContext.CarritoElementos
            .AsNoTracking()
            .Include(x => x.Producto)
            .Where(x => x.Id == itemId)
            .Select(x => new CarritoElementoDto(
                x.Id,
                x.TiendaId,
                x.UsuarioId,
                x.ProductoId,
                x.Cantidad,
                x.FechaAdicion,
                x.Producto!.Nombre,
                x.Producto!.PrecioDetalle,
                x.Producto!.PrecioDetalle * x.Cantidad))
            .FirstOrDefaultAsync(cancellationToken);
    }

    private static ReservacionDto MapReservacion(Reservacion reservacion)
    {
        var detalles = reservacion.Detalles
            .Select(x => new DetalleReservacionDto(
                x.Id,
                x.ReservacionId,
                x.ProductoId,
                x.Cantidad,
                x.PrecioCobrado,
                x.Subtotal,
                x.Producto?.Nombre))
            .ToList();

        return new ReservacionDto(
            reservacion.Id,
            reservacion.TiendaId,
            reservacion.SucursalId,
            reservacion.UsuarioId,
            reservacion.MontoTotal,
            reservacion.EstadoPago,
            reservacion.EstadoDespacho,
            reservacion.StripeIntentId,
            reservacion.FechaReserva,
            detalles);
    }

    private async Task<ReservacionDto?> MapReservacionAsync(string reservacionId, CancellationToken cancellationToken)
    {
        var reservacion = await _dbContext.Reservaciones
            .AsNoTracking()
            .Include(x => x.Detalles)
            .ThenInclude(x => x.Producto)
            .FirstOrDefaultAsync(x => x.Id == reservacionId, cancellationToken);

        return reservacion is null ? null : MapReservacion(reservacion);
    }

    private static ReportePersonalizadoDto MapReporte(ReportePersonalizado reporte)
    {
        return new ReportePersonalizadoDto(
            reporte.Id,
            reporte.TiendaId,
            reporte.Nombre,
            reporte.Descripcion,
            reporte.QuerySql,
            reporte.CreadoPor,
            reporte.FechaCreacion);
    }

    private async Task<Tienda> GetTenantStoreAsync(CancellationToken cancellationToken)
    {
        var tenantId = RequireTenantId();
        return await _dbContext.Tiendas.FirstAsync(x => x.Id == tenantId, cancellationToken);
    }

    private string RequireTenantId()
    {
        var tenantId = _tenantProvider.GetTenantId();
        if (string.IsNullOrWhiteSpace(tenantId))
        {
            throw new InvalidOperationException("Se requiere el header X-Tenant-ID.");
        }

        return tenantId;
    }

    private static decimal ObtenerPrecio(CarritoElemento item)
    {
        return item.Producto is null
            ? 0m
            : (item.Cantidad >= 10 ? item.Producto.PrecioMayoreo : item.Producto.PrecioDetalle);
    }

    private static void ValidarSqlSeleccion(string querySql)
    {
        var normalized = querySql.Trim();
        if (!normalized.StartsWith("select", StringComparison.OrdinalIgnoreCase) &&
            !normalized.StartsWith("with", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Solo se permiten consultas SELECT.");
        }

        if (UnsafeSqlPattern.IsMatch(normalized))
        {
            throw new InvalidOperationException("La consulta contiene comandos no permitidos.");
        }
    }
}