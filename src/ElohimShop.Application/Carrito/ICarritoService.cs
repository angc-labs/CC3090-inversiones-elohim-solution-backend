using ElohimShop.Application.Carrito;

namespace ElohimShop.Application.Carrito;

public interface ICarritoService
{
    Task<CarritoDto?> ObtenerCarritoActivoAsync(string clienteId, CancellationToken ct = default);
    Task<ArticuloCarritoDto?> AgregarArticuloAsync(string clienteId, AgregarArticuloCarritoDto dto, CancellationToken ct = default);
    Task<ArticuloCarritoDto?> ActualizarCantidadArticuloAsync(string clienteId, string articuloId, ActualizarCantidadArticuloDto dto, CancellationToken ct = default);
    Task<bool> EliminarArticuloAsync(string clienteId, string articuloId, CancellationToken ct = default);
}