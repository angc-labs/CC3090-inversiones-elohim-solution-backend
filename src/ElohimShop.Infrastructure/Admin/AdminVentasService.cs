using ElohimShop.Application.Admin;
using ElohimShop.Infrastructure.Persistence;
using ElohimShop.Domain.Platform;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ElohimShop.Infrastructure.Admin;

public class AdminVentasService : IAdminVentasService
{
    private readonly PlatformDbContext _dbContext;

    public AdminVentasService(PlatformDbContext dbContext)
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
        var query = _dbContext.Reservaciones
            .AsNoTracking()
            .Include(r => r.Usuario)
            .Include(r => r.Detalles)
            .Where(r => r.EstadoPago == "pagado");

        var ventasDb = await query
            .OrderByDescending(v => v.FechaReserva)
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

    private static VentaAdminItemDto MapVenta(Reservacion reservacion)
    {
        var detalles = reservacion.Detalles ?? new List<DetalleReservacion>();
        var subtotal = detalles.Sum(d => d.Subtotal);
        if (subtotal <= 0)
        {
            subtotal = reservacion.MontoTotal;
        }

        var cliente = reservacion.Usuario;
        var nombreCliente = cliente is null
            ? "Cliente no registrado"
            : cliente.Name;

        return new VentaAdminItemDto(
            reservacion.Id,
            nombreCliente,
            detalles.Sum(d => d.Cantidad),
            subtotal,
            0,
            reservacion.MontoTotal,
            reservacion.FechaReserva,
            string.IsNullOrEmpty(reservacion.StripeIntentId) ? "efectivo" : "tarjeta",
            "Venta en línea",
            "completada");
    }
}

