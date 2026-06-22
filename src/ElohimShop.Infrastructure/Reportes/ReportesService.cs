using ElohimShop.Application.Reportes;
using ElohimShop.Infrastructure.Persistence;
using ElohimShop.Domain.Platform;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ElohimShop.Infrastructure.Reportes;

public class ReportesService : IReportesService
{
    private const string ModoVentas = "ventas";
    private const string ModoReservaciones = "reservaciones";
    private const string EmpleadoReservacionWeb = "Reservaciones en línea";

    private readonly PlatformDbContext _dbContext;

    public ReportesService(PlatformDbContext dbContext)
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
                p.Nombre,
                p.StockActual,
                p.StockMinimo
            })
            .ToListAsync(cancellationToken);

        var criticos = productos
            .Where(p => p.StockActual < p.StockMinimo)
            .Select(p => new
            {
                NombreProducto = p.Nombre,
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

        var query = _dbContext.Reservaciones
            .AsNoTracking()
            .Where(r => r.FechaReserva >= desde && r.FechaReserva <= hasta);

        if (modo == ModoVentas)
        {
            query = query.Where(r => r.EstadoPago == "pagado");
        }
        else if (modo == ModoReservaciones)
        {
            query = query.Where(r => r.EstadoPago != "pagado");
        }

        var eventosDb = await query
            .Select(r => new { r.FechaReserva, r.UsuarioId })
            .ToListAsync(cancellationToken);

        var eventos = eventosDb.Select(e => (e.FechaReserva, e.UsuarioId)).ToList();

        var porHora = Enumerable.Range(8, 11)
            .Select(hora =>
            {
                var enHora = eventos.Where(e => e.FechaReserva.ToUniversalTime().Hour == hora).ToList();
                var ventasCount = enHora.Count;
                var clientes = enHora
                    .Select(e => e.UsuarioId)
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

        var query = _dbContext.Reservaciones
            .AsNoTracking()
            .Where(r => r.FechaReserva >= desde && r.FechaReserva <= hasta);

        if (modo == ModoVentas)
        {
            query = query.Where(r => r.EstadoPago == "pagado");
        }
        else if (modo == ModoReservaciones)
        {
            query = query.Where(r => r.EstadoPago != "pagado");
        }

        var reservaciones = await query
            .Select(r => new
            {
                r.StripeIntentId,
                Monto = r.MontoTotal
            })
            .ToListAsync(cancellationToken);

        var movimientos = reservaciones.Select(r => new {
            Metodo = string.IsNullOrEmpty(r.StripeIntentId) ? "Efectivo" : "Tarjeta",
            r.Monto
        }).ToList();

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

        var query = _dbContext.DetallesReservacion
            .AsNoTracking()
            .Include(d => d.Producto)
            .Include(d => d.Reservacion)
            .Where(d => d.Reservacion!.FechaReserva >= desde && d.Reservacion.FechaReserva <= hasta);

        if (modo == ModoVentas)
        {
            query = query.Where(d => d.Reservacion!.EstadoPago == "pagado");
        }
        else if (modo == ModoReservaciones)
        {
            query = query.Where(d => d.Reservacion!.EstadoPago != "pagado");
        }

        var list = await query
            .Select(d => new {
                Nombre = d.Producto != null ? d.Producto.Nombre : "Producto eliminado",
                d.Cantidad,
                d.Subtotal
            })
            .ToListAsync(cancellationToken);

        return list.Select(d => new LineaProducto(d.Nombre, d.Cantidad, d.Subtotal)).ToList();
    }

    private async Task<List<TransaccionEmpleado>> ObtenerTransaccionesEmpleadoAsync(
        ReportesFiltroDto filtro,
        CancellationToken cancellationToken)
    {
        var (desde, hasta) = NormalizarRango(filtro);
        var modo = NormalizarModo(filtro.Modo);
        var transacciones = new List<TransaccionEmpleado>();

        var query = _dbContext.Reservaciones
            .AsNoTracking()
            .Where(r => r.FechaReserva >= desde && r.FechaReserva <= hasta);

        if (modo == ModoVentas)
        {
            query = query.Where(r => r.EstadoPago == "pagado");
        }
        else if (modo == ModoReservaciones)
        {
            query = query.Where(r => r.EstadoPago != "pagado");
        }

        var reservaciones = await query
            .Select(r => new { r.MontoTotal })
            .ToListAsync(cancellationToken);

        transacciones.AddRange(reservaciones.Select(r => new TransaccionEmpleado(
            EmpleadoReservacionWeb,
            r.MontoTotal)));

        return transacciones;
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

    private sealed record LineaProducto(string NombreProducto, int Cantidad, decimal Subtotal);

    private sealed record TransaccionEmpleado(string Empleado, decimal Monto);
}

