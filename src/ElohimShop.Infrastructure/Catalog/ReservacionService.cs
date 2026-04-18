using ElohimShop.Application.Reservacion;
using ElohimShop.Domain.Entities;
using ElohimShop.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ElohimShop.Infrastructure.Catalog;

public class ReservacionService : IReservacionService
{
    private readonly ElohimShopDbContext _dbContext;

    public ReservacionService(ElohimShopDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ReservacionDto> CrearReservacionAsync(string clienteId, CrearReservacionDto dto, CancellationToken ct = default)
    {
        var carrito = await _dbContext.Carritos
            .Include(c => c.Articulos)
            .FirstOrDefaultAsync(c => c.ClienteId == clienteId && c.Activo, ct);

        if (carrito is null || !carrito.Articulos.Any())
        {
            throw new InvalidOperationException("El carrito está vacío.");
        }

        var metodoPago = await _dbContext.MetodosPago
            .FirstOrDefaultAsync(mp => mp.IdMetodoPago == dto.MetodoPagoId && mp.UsuarioId == clienteId, ct);

        if (metodoPago is null)
        {
            throw new ArgumentException("Método de pago no válido.");
        }

        var reservacion = Reservacion.Crear(clienteId, dto.MetodoPagoId);

        decimal total = 0;
        foreach (var articulo in carrito.Articulos)
        {
            reservacion.AgregarDetalle(articulo.ProductoId, articulo.NombreProducto, articulo.Cantidad, articulo.Subtotal);
            total += articulo.Subtotal;
        }

        reservacion.CalcularTotal();

        _dbContext.Reservaciones.Add(reservacion);
        
        foreach (var articulo in carrito.Articulos)
        {
            _dbContext.ArticulosCarrito.Remove(articulo);
        }
        
        await _dbContext.SaveChangesAsync(ct);

        return MapToDto(reservacion);
    }

    public async Task<IReadOnlyList<ReservacionListadoDto>> ObtenerReservacionesAsync(string? clienteId, bool esAdministrador, CancellationToken ct = default)
    {
        var query = _dbContext.Reservaciones
            .AsNoTracking()
            .AsQueryable();

        if (!esAdministrador && !string.IsNullOrWhiteSpace(clienteId))
        {
            query = query.Where(r => r.ClienteId == clienteId);
        }

        var reservaciones = await query
            .OrderByDescending(r => r.FechaRenovacion)
            .ToListAsync(ct);

        return reservaciones.Select(r => new ReservacionListadoDto
        {
            IdReservacion = r.IdReservacion,
            CodigoReservacion = r.CodigoReservacion,
            ClienteId = r.ClienteId ?? string.Empty,
            Estado = r.EstadoRenovacion,
            TotalReservacion = r.TotalRenovacion ?? 0,
            Pagado = r.Pagado,
            FechaLimiteRetiro = r.FechaLimiteRetiro
        }).ToList();
    }

    public async Task<ReservacionDto?> ObtenerReservacionPorIdAsync(string id, string? clienteId, bool esAdministrador, CancellationToken ct = default)
    {
        var reservacion = await _dbContext.Reservaciones
            .AsNoTracking()
            .Include(r => r.Detalles)
            .FirstOrDefaultAsync(r => r.IdReservacion == id, ct);

        if (reservacion is null)
        {
            return null;
        }

        if (!esAdministrador && reservacion.ClienteId != clienteId)
        {
            throw new UnauthorizedAccessException("No tenés permisos para ver esta reservación.");
        }

        return MapToDto(reservacion);
    }

    private static ReservacionDto MapToDto(Reservacion reservacion)
    {
        return new ReservacionDto
        {
            IdReservacion = reservacion.IdReservacion,
            CodigoReservacion = reservacion.CodigoReservacion,
            ClienteId = reservacion.ClienteId ?? string.Empty,
            Estado = reservacion.EstadoRenovacion,
            TotalReservacion = reservacion.TotalRenovacion ?? 0,
            MetodoPagoId = reservacion.MetodoPagoId,
            Pagado = reservacion.Pagado,
            Observaciones = reservacion.Observaciones,
            FechaLimiteRetiro = reservacion.FechaLimiteRetiro,
            Items = reservacion.Detalles.Select(d => new DetalleReservacionDto
            {
                ProductoId = d.ProductoId ?? string.Empty,
                NombreProducto = d.NombreProducto,
                Cantidad = d.Cantidad,
                PrecioUnitario = (int)d.PrecioUnitario,
                Subtotal = d.Subtotal
            }).ToList()
        };
    }
}