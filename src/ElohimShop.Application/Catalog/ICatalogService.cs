namespace ElohimShop.Application.Catalog;

public interface ICatalogService
{
    Task<IReadOnlyList<MarcaDto>> ObtenerMarcasAsync(CancellationToken cancellationToken);
    Task<IReadOnlyList<CategoriaDto>> ObtenerCategoriasAsync(CancellationToken cancellationToken);
    Task<ProductoPaginacionDto> ObtenerProductosAsync(string? categoriaId, string? marcaId, int pagina, int limite, CancellationToken cancellationToken);
    Task<ProductoDetalleDto?> ObtenerProductoPorIdAsync(string id, CancellationToken cancellationToken);
    Task<BusquedaProductosDto> BuscarProductosAsync(string query, CancellationToken cancellationToken);
}
