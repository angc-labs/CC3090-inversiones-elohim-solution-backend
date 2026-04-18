using ElohimShop.Application.Reservacion;

namespace ElohimShop.Application.Reservacion;

public interface IReservacionService
{
    Task<ReservacionDto> CrearReservacionAsync(string clienteId, CrearReservacionDto dto, CancellationToken ct = default);
    Task<IReadOnlyList<ReservacionListadoDto>> ObtenerReservacionesAsync(string? clienteId, bool esAdministrador, CancellationToken ct = default);
    Task<ReservacionDto?> ObtenerReservacionPorIdAsync(string id, string? clienteId, bool esAdministrador, CancellationToken ct = default);
}