namespace ElohimShop.Application.Catalog;

public class ProductoBusquedaDto
{
    public string IdProducto { get; init; } = string.Empty;
    public string NombreProducto { get; init; } = string.Empty;
    public int Precio { get; init; }
    public string? ImagenPrincipal { get; init; }
}
