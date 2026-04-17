namespace ElohimShop.Application.Catalog;

public class ProductoListadoDto
{
    public string IdProducto { get; init; } = string.Empty;
    public string CodigoProducto { get; init; } = string.Empty;
    public string NombreProducto { get; init; } = string.Empty;
    public string? Descripcion { get; init; }
    public int Precio { get; init; }
    public int StockActual { get; init; }
    public string? IdMarca { get; init; }
    public string? CategoriaId { get; init; }
    public string? ImagenPrincipal { get; init; }
    public DateTime FechaVencimiento { get; init; }
}
