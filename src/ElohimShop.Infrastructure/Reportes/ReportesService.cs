using ElohimShop.Application.Reportes;
using ElohimShop.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ElohimShop.Infrastructure.Reportes;

public class ReportesService : IReportesService
{
    private const string ModoVentas = "ventas";
    private const string ModoReservaciones = "reservaciones";
    private const string EmpleadoReservacionWeb = "Reservaciones en línea";

    private static readonly string[] EstadosReservacionCompletada =
    [
        "completada",
        "entregada",
        "finalizada",
        "confirmada"
    ];

    private readonly ElohimShopDbContext _dbContext;

    public ReportesService(ElohimShopDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ReporteProductosDto> ObtenerProductosAsync(
        ReportesFiltroDto filtro,
        CancellationToken cancellationToken)
    {
        var lineas = await ObtenerLineasProductoAsync(filtro, cancellationToken);

        var agrupados = lineas
            .GroupBy(d => d.NombreProducto)
            .Select(g => new ReporteProductoItemDto(
                g.Key,
                g.Sum(x => x.Cantidad),
                g.Sum(x => x.Subtotal)))
            .OrderByDescending(p => p.CantidadVendida)
            .ToList();

        var totalUnidades = agrupados.Sum(p => p.CantidadVendida);
        var ingresosTotales = agrupados.Sum(p => p.Ingresos);
        var top = agrupados.FirstOrDefault();

        var detalle = agrupados
            .Select((p, index) => new ReporteProductoDetalleDto(
                index + 1,
                p.Producto,
                p.CantidadVendida,
                p.Ingresos,
                p.CantidadVendida > 0 ? Math.Round(p.Ingresos / p.CantidadVendida, 2) : 0))
            .ToList();

        return new ReporteProductosDto(
            totalUnidades,
            ingresosTotales,
            top?.Producto,
            top?.CantidadVendida,
            agrupados,
            detalle);
    }

    public async Task<ReporteEmpleadosDto> ObtenerEmpleadosAsync(
        ReportesFiltroDto filtro,
        CancellationToken cancellationToken)
    {
        var transacciones = await ObtenerTransaccionesEmpleadoAsync(filtro, cancellationToken);

        var agrupados = transacciones
            .GroupBy(t => t.Empleado)
            .Select(g => new ReporteEmpleadoItemDto(
                g.Key,
                g.Count(),
                g.Sum(x => x.Monto)))
            .OrderByDescending(e => e.Ventas)
            .ToList();

        var top = agrupados.FirstOrDefault();
        var maxVentas = top?.Ventas ?? 0;

        var detalle = agrupados
            .Select(e => new ReporteEmpleadoDetalleDto(
                e.Empleado,
                e.Ventas,
                e.Monto,
                e.Ventas > 0 ? Math.Round(e.Monto / e.Ventas, 2) : 0,
                ClasificarDesempeno(e.Ventas, maxVentas)))
            .ToList();

        return new ReporteEmpleadosDto(
            agrupados.Count,
            transacciones.Count,
            transacciones.Sum(t => t.Monto),
            top?.Empleado,
            top?.Ventas,
            agrupados,
            detalle);
    }

    public async Task<ReporteStockCriticoDto> ObtenerStockCriticoAsync(CancellationToken cancellationToken)
    {
        var productos = await _dbContext.Productos
            .AsNoTracking()
            .OrderBy(p => p.StockActual)
            .Select(p => new
            {
                p.NombreProducto,
                p.StockActual,
                p.StockMinimo
            })
            .ToListAsync(cancellationToken);

        var criticos = productos
            .Where(p => p.StockActual < p.StockMinimo)
            .Select(p => new
            {
                p.NombreProducto,
                p.StockActual,
                p.StockMinimo,
                Faltante = p.StockActual - p.StockMinimo,
                Frecuencia = ClasificarFrecuenciaQuiebre(p.StockActual - p.StockMinimo)
            })
            .ToList();

        return new ReporteStockCriticoDto(
            criticos.Count,
            criticos.Sum(p => Math.Abs(Math.Min(0, p.Faltante))),
            criticos.Count(p => p.Frecuencia == "Alta"),
            criticos.Select(p => new ReporteStockChartItemDto(p.NombreProducto, p.StockActual, p.StockMinimo)).ToList(),
            criticos.Select(p => new ReporteStockDetalleDto(
                p.NombreProducto,
                p.StockActual,
                p.StockMinimo,
                p.Faltante,
                p.Frecuencia,
                "Crítico")).ToList());
    }

    public async Task<ReporteDemandaDto> ObtenerDemandaAsync(
        ReportesFiltroDto filtro,
        CancellationToken cancellationToken)
    {
        var (desde, hasta) = NormalizarRango(filtro);
        var modo = NormalizarModo(filtro.Modo);
        var reservacionIdsConVenta = await ObtenerReservacionIdsConVentaAsync(cancellationToken);

        var eventos = new List<(DateTime Fecha, string? ClienteId)>();

        if (modo is not ModoVentas)
        {
            var reservaciones = await _dbContext.Reservaciones
                .AsNoTracking()
                .Where(r => r.Pagado || EstadosReservacionCompletada.Contains(r.EstadoRenovacion))
                .Where(r => r.FechaRenovacion >= desde && r.FechaRenovacion <= hasta)
                .Where(r => modo == ModoReservaciones || !reservacionIdsConVenta.Contains(r.IdReservacion))
                .Select(r => new { r.FechaRenovacion, r.ClienteId })
                .ToListAsync(cancellationToken);

            eventos.AddRange(reservaciones.Select(r => (r.FechaRenovacion, r.ClienteId)));
        }

        if (modo is ModoVentas or "todos")
        {
            var ventas = await _dbContext.Ventas
                .AsNoTracking()
                .Where(v => v.EstadoVenta != "cancelada")
                .Where(v => v.FechaVenta >= desde && v.FechaVenta <= hasta)
                .Select(v => new { v.FechaVenta, ClienteId = v.Reservacion != null ? v.Reservacion.ClienteId : null })
                .ToListAsync(cancellationToken);

            eventos.AddRange(ventas.Select(v => (v.FechaVenta, v.ClienteId)));
        }

        var porHora = Enumerable.Range(8, 11)
            .Select(hora =>
            {
                var enHora = eventos.Where(e => e.Fecha.ToUniversalTime().Hour == hora).ToList();
                var ventasCount = enHora.Count;
                var clientes = enHora
                    .Select(e => e.ClienteId)
                    .Where(id => id != null)
                    .Distinct()
                    .Count();
                if (clientes < ventasCount)
                {
                    clientes = ventasCount;
                }

                return new ReporteDemandaChartItemDto($"{hora:00}:00", ventasCount, clientes);
            })
            .ToList();

        var pico = porHora.OrderByDescending(h => h.Ventas).FirstOrDefault();
        var promedio = porHora.Count > 0 ? porHora.Average(h => h.Ventas) : 0;
        var ventasPico = pico?.Ventas ?? 0;
        var horaPicoInicio = pico != null ? int.Parse(pico.Horario[..2]) : 12;

        return new ReporteDemandaDto(
            $"{horaPicoInicio:00}:00 - {(horaPicoInicio + 2):00}:00",
            ventasPico,
            Math.Round(promedio, 1),
            porHora,
            porHora.Select(h =>
            {
                var ratio = h.Clientes > 0 ? Math.Round((decimal)h.Ventas / h.Clientes * 100, 2) : 0;
                return new ReporteDemandaDetalleDto(
                    h.Horario,
                    h.Ventas,
                    h.Clientes,
                    ratio,
                    ClasificarDemanda(h.Ventas, ventasPico, promedio));
            }).ToList());
    }

    public async Task<ReporteMetodosPagoDto> ObtenerMetodosPagoAsync(
        ReportesFiltroDto filtro,
        CancellationToken cancellationToken)
    {
        var (desde, hasta) = NormalizarRango(filtro);
        var modo = NormalizarModo(filtro.Modo);
        var reservacionIdsConVenta = await ObtenerReservacionIdsConVentaAsync(cancellationToken);

        var movimientos = new List<(string Metodo, decimal Monto)>();

        if (modo is not ModoVentas)
        {
            var reservaciones = await _dbContext.Reservaciones
                .AsNoTracking()
                .Where(r => r.Pagado)
                .Where(r => r.FechaRenovacion >= desde && r.FechaRenovacion <= hasta)
                .Where(r => r.MetodoPagoId != null)
                .Where(r => modo == ModoReservaciones || !reservacionIdsConVenta.Contains(r.IdReservacion))
                .Select(r => new
                {
                    Metodo = r.MetodoPago != null ? r.MetodoPago.NombreMetodo : "Otro",
                    Monto = r.TotalRenovacion ?? 0
                })
                .ToListAsync(cancellationToken);

            movimientos.AddRange(reservaciones.Select(r => (NormalizarMetodoPago(r.Metodo), r.Monto)));
        }

        if (modo is ModoVentas or "todos")
        {
            var ventas = await _dbContext.Ventas
                .AsNoTracking()
                .Include(v => v.Reservacion)
                .ThenInclude(r => r!.MetodoPago)
                .Where(v => v.EstadoVenta != "cancelada")
                .Where(v => v.FechaVenta >= desde && v.FechaVenta <= hasta)
                .Where(v => v.Reservacion != null && v.Reservacion.MetodoPago != null)
                .Select(v => new
                {
                    Metodo = v.Reservacion!.MetodoPago!.NombreMetodo,
                    v.MontoTotal
                })
                .ToListAsync(cancellationToken);

            movimientos.AddRange(ventas.Select(v => (NormalizarMetodoPago(v.Metodo), v.MontoTotal)));
        }

        var agrupados = movimientos
            .GroupBy(m => m.Metodo)
            .Select(g => new
            {
                Metodo = g.Key,
                Transacciones = g.Count(),
                Monto = g.Sum(x => x.Monto)
            })
            .OrderByDescending(m => m.Transacciones)
            .ToList();

        var totalTransacciones = agrupados.Sum(m => m.Transacciones);

        return new ReporteMetodosPagoDto(
            agrupados.Select(m => new ReporteMetodoPagoResumenDto(m.Metodo, m.Transacciones, m.Monto)).ToList(),
            agrupados.Select(m => new ReporteMetodoPagoChartItemDto(
                m.Metodo,
                m.Transacciones,
                m.Monto,
                totalTransacciones > 0 ? Math.Round((decimal)m.Transacciones / totalTransacciones * 100, 0) : 0)).ToList(),
            agrupados.Select(m => new ReporteMetodoPagoDetalleDto(
                m.Metodo,
                m.Transacciones,
                totalTransacciones > 0 ? Math.Round((decimal)m.Transacciones / totalTransacciones * 100, 0) : 0,
                m.Monto,
                m.Transacciones > 0 ? Math.Round(m.Monto / m.Transacciones, 2) : 0)).ToList());
    }

    private async Task<List<LineaProducto>> ObtenerLineasProductoAsync(
        ReportesFiltroDto filtro,
        CancellationToken cancellationToken)
    {
        var (desde, hasta) = NormalizarRango(filtro);
        var modo = NormalizarModo(filtro.Modo);
        var reservacionIdsConVenta = await ObtenerReservacionIdsConVentaAsync(cancellationToken);
        var lineas = new List<LineaProducto>();

        if (modo is not ModoVentas)
        {
            var detallesReserva = await _dbContext.DetallesReservacion
                .AsNoTracking()
                .Where(d => d.ReservacionId != null)
                .Join(
                    _dbContext.Reservaciones.AsNoTracking(),
                    d => d.ReservacionId,
                    r => r.IdReservacion,
                    (d, r) => new { Detalle = d, Reservacion = r })
                .Where(x => x.Reservacion.Pagado || EstadosReservacionCompletada.Contains(x.Reservacion.EstadoRenovacion))
                .Where(x => x.Reservacion.FechaRenovacion >= desde && x.Reservacion.FechaRenovacion <= hasta)
                .Where(x => modo == ModoReservaciones || !reservacionIdsConVenta.Contains(x.Reservacion.IdReservacion))
                .Select(x => new LineaProducto(
                    x.Detalle.NombreProducto,
                    x.Detalle.Cantidad,
                    x.Detalle.Subtotal))
                .ToListAsync(cancellationToken);

            lineas.AddRange(detallesReserva);
        }

        if (modo is ModoVentas or "todos")
        {
            var detallesVenta = await _dbContext.DetallesReservacion
                .AsNoTracking()
                .Where(d => d.ReservacionId != null)
                .Join(
                    _dbContext.Ventas.AsNoTracking(),
                    d => d.ReservacionId,
                    v => v.ReservacionId,
                    (d, v) => new { Detalle = d, Venta = v })
                .Where(x => x.Venta.EstadoVenta != "cancelada")
                .Where(x => x.Venta.FechaVenta >= desde && x.Venta.FechaVenta <= hasta)
                .Select(x => new LineaProducto(
                    x.Detalle.NombreProducto,
                    x.Detalle.Cantidad,
                    x.Detalle.Subtotal))
                .ToListAsync(cancellationToken);

            lineas.AddRange(detallesVenta);
        }

        return lineas;
    }

    private async Task<List<TransaccionEmpleado>> ObtenerTransaccionesEmpleadoAsync(
        ReportesFiltroDto filtro,
        CancellationToken cancellationToken)
    {
        var (desde, hasta) = NormalizarRango(filtro);
        var modo = NormalizarModo(filtro.Modo);
        var reservacionIdsConVenta = await ObtenerReservacionIdsConVentaAsync(cancellationToken);
        var transacciones = new List<TransaccionEmpleado>();

        if (modo is not ModoVentas)
        {
            var reservaciones = await _dbContext.Reservaciones
                .AsNoTracking()
                .Where(r => r.Pagado || EstadosReservacionCompletada.Contains(r.EstadoRenovacion))
                .Where(r => r.FechaRenovacion >= desde && r.FechaRenovacion <= hasta)
                .Where(r => modo == ModoReservaciones || !reservacionIdsConVenta.Contains(r.IdReservacion))
                .Select(r => new { r.TotalRenovacion })
                .ToListAsync(cancellationToken);

            transacciones.AddRange(reservaciones.Select(r => new TransaccionEmpleado(
                EmpleadoReservacionWeb,
                r.TotalRenovacion ?? 0)));
        }

        if (modo is ModoVentas or "todos")
        {
            var ventas = await _dbContext.Ventas
                .AsNoTracking()
                .Include(v => v.UsuarioCajero)
                .Where(v => v.EstadoVenta != "cancelada")
                .Where(v => v.FechaVenta >= desde && v.FechaVenta <= hasta)
                .Select(v => new TransaccionEmpleado(
                    v.UsuarioCajero != null
                        ? $"{v.UsuarioCajero.Nombre} {v.UsuarioCajero.Apellido}".Trim()
                        : "Sin asignar",
                    v.MontoTotal))
                .ToListAsync(cancellationToken);

            transacciones.AddRange(ventas);
        }

        return transacciones;
    }

    private async Task<HashSet<string>> ObtenerReservacionIdsConVentaAsync(CancellationToken cancellationToken)
    {
        var ids = await _dbContext.Ventas
            .AsNoTracking()
            .Where(v => v.ReservacionId != null)
            .Select(v => v.ReservacionId!)
            .ToListAsync(cancellationToken);

        return ids.ToHashSet(StringComparer.Ordinal);
    }

    private static (DateTime Desde, DateTime Hasta) NormalizarRango(ReportesFiltroDto filtro)
    {
        var hasta = filtro.Hasta?.ToUniversalTime() ?? DateTime.UtcNow;
        var desde = filtro.Desde?.ToUniversalTime() ?? hasta.AddDays(-30);
        if (desde > hasta)
        {
            (desde, hasta) = (hasta, desde);
        }

        return (desde, hasta);
    }

    private static string NormalizarModo(string? modo) =>
        (modo ?? "todos").Trim().ToLowerInvariant() switch
        {
            ModoVentas => ModoVentas,
            ModoReservaciones => ModoReservaciones,
            _ => "todos"
        };

    private static string ClasificarFrecuenciaQuiebre(int faltante) =>
        faltante <= -10 ? "Alta" : faltante <= -5 ? "Media" : "Baja";

    private static string ClasificarDesempeno(int ventas, int maxVentas)
    {
        if (maxVentas == 0)
        {
            return "Bueno";
        }

        var ratio = (double)ventas / maxVentas;
        return ratio >= 0.95 ? "Excelente"
            : ratio >= 0.8 ? "Muy Bueno"
            : "Bueno";
    }

    private static string ClasificarDemanda(int ventas, int ventasPico, double promedio)
    {
        if (ventasPico > 0 && ventas >= ventasPico)
        {
            return "Hora Pico";
        }

        if (promedio > 0 && ventas >= promedio * 1.15)
        {
            return "Alta";
        }

        return "Normal";
    }

    private static string NormalizarMetodoPago(string metodo)
    {
        var normalizado = metodo.Trim();
        if (normalizado.Contains("yape", StringComparison.OrdinalIgnoreCase))
        {
            return "Yape";
        }

        if (normalizado.Contains("plin", StringComparison.OrdinalIgnoreCase))
        {
            return "Plin";
        }

        if (normalizado.Contains("efect", StringComparison.OrdinalIgnoreCase))
        {
            return "Efectivo";
        }

        if (normalizado.Contains("tarj", StringComparison.OrdinalIgnoreCase)
            || normalizado.Contains("card", StringComparison.OrdinalIgnoreCase)
            || normalizado.Contains("stripe", StringComparison.OrdinalIgnoreCase))
        {
            return "Tarjeta";
        }

        return string.IsNullOrWhiteSpace(normalizado) ? "Otro" : normalizado;
    }

    private sealed record LineaProducto(string NombreProducto, int Cantidad, decimal Subtotal);

    private sealed record TransaccionEmpleado(string Empleado, decimal Monto);
}
