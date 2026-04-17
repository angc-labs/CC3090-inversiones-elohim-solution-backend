namespace ElohimShop.Application.Catalog;

public class BusquedaProductosDto
{
    public string Query { get; init; } = string.Empty;
    public IReadOnlyList<ProductoBusquedaDto> Resultados { get; init; } = [];
}
