namespace ElohimShop.Application.Admin;

public interface IAdminVentasService
{
    Task<VentasAdminListadoDto> ObtenerListadoAsync(
        string? busqueda,
        DateOnly? fecha,
        string? filtroPrecio,
        string? filtroMetodoPago,
        CancellationToken cancellationToken);
}
