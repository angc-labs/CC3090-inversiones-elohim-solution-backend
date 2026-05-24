using ElohimShop.Application.Admin;
using ElohimShop.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ElohimShop.Infrastructure.Admin;

public class AdminVentasService : IAdminVentasService
{
    private readonly ElohimShopDbContext _dbContext;

    public AdminVentasService(ElohimShopDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<VentasAdminListadoDto> ObtenerListadoAsync(
        string? busqueda,
        DateOnly? fecha,
        string? filtroPrecio,
        string? filtroMetodoPago,
        CancellationToken cancellationToken)
    {
        var ventasDb = await _dbContext.Ventas
            .AsNoTracking()
            .Include(v => v.Reservacion)
                .ThenInclude(r => r!.Cliente)
            .Include(v => v.Reservacion)
                .ThenInclude(r => r!.MetodoPago)
            .Include(v => v.Reservacion)
                .ThenInclude(r => r!.Detalles)
            .Include(v => v.UsuarioCajero)
            .OrderByDescending(v => v.FechaVenta)
            .ToListAsync(cancellationToken);

        var items = ventasDb.Select(MapVenta).ToList();

        if (!string.IsNullOrWhiteSpace(busqueda))
        {
            var q = busqueda.Trim().ToLowerInvariant();
            items = items.Where(v =>
                    v.Id.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                    v.Cliente.Contains(q, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        if (fecha.HasValue)
        {
            items = items
                .Where(v => DateOnly.FromDateTime(v.Fecha) == fecha.Value)
                .ToList();
        }

        if (!string.IsNullOrWhiteSpace(filtroMetodoPago) &&
            !string.Equals(filtroMetodoPago, "todos", StringComparison.OrdinalIgnoreCase))
        {
            var metodo = filtroMetodoPago.Trim().ToLowerInvariant();
            items = items.Where(v => v.MetodoPago == metodo).ToList();
        }

        items = filtroPrecio switch
        {
            "mayor-menor" => items.OrderByDescending(v => v.Total).ToList(),
            "menor-mayor" => items.OrderBy(v => v.Total).ToList(),
            _ => items
        };

        var hoy = DateOnly.FromDateTime(DateTime.UtcNow);
        var ventasHoy = items.Where(v => DateOnly.FromDateTime(v.Fecha) == hoy).ToList();

        var ingresosHoy = ventasHoy.Sum(v => v.Total);
        var resumen = new VentasResumenDto(
            ventasHoy.Count,
            ingresosHoy,
            ventasHoy.Count > 0 ? Math.Round(ingresosHoy / ventasHoy.Count, 2) : 0,
            ventasHoy.Sum(v => v.Productos));

        return new VentasAdminListadoDto(resumen, items);
    }

    private static VentaAdminItemDto MapVenta(Domain.Entities.Venta venta)
    {
        var detalles = venta.Reservacion?.Detalles ?? Array.Empty<Domain.Entities.DetalleReservacion>();
        var subtotal = detalles.Sum(d => d.Subtotal);
        if (subtotal <= 0)
        {
            subtotal = venta.MontoTotal;
        }

        var cliente = venta.Reservacion?.Cliente;
        var nombreCliente = cliente is null
            ? "Cliente no registrado"
            : string.Join(" ", new[] { cliente.Nombre, cliente.Apellido }.Where(s => !string.IsNullOrWhiteSpace(s)));

        var cajero = venta.UsuarioCajero;
        var nombreCajero = cajero is null
            ? "—"
            : string.Join(" ", new[] { cajero.Nombre, cajero.Apellido }.Where(s => !string.IsNullOrWhiteSpace(s)));

        return new VentaAdminItemDto(
            venta.IdVenta,
            nombreCliente,
            detalles.Sum(d => d.Cantidad),
            subtotal,
            0,
            venta.MontoTotal,
            venta.FechaVenta,
            NormalizarMetodoPago(venta.Reservacion?.MetodoPago?.NombreMetodo),
            nombreCajero,
            venta.EstadoVenta);
    }

    private static string NormalizarMetodoPago(string? nombreMetodo)
    {
        if (string.IsNullOrWhiteSpace(nombreMetodo))
        {
            return "efectivo";
        }

        var nombre = nombreMetodo.ToLowerInvariant();
        if (nombre.Contains("efectivo") || nombre.Contains("cash"))
        {
            return "efectivo";
        }

        return "tarjeta";
    }
}
