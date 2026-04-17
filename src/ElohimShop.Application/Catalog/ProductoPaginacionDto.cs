namespace ElohimShop.Application.Catalog;

public class ProductoPaginacionDto
{
    public int Total { get; init; }
    public int Pagina { get; init; }
    public int Limite { get; init; }
    public IReadOnlyList<ProductoListadoDto> Productos { get; init; } = [];
}
