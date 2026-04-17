namespace ElohimShop.Application.Catalog;

public class CategoriaDto
{
    public string Id { get; init; } = string.Empty;
    public string NombreCategoria { get; init; } = string.Empty;
    public string? Descripcion { get; init; }
    public DateTime? FechaCreacion { get; init; }
}
