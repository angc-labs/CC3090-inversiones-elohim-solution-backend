namespace ElohimShop.Application.Products;

public record UpdateProductRequestDto(
    string NombreProducto,
    int Precio,
    int StockActual,
    string? Descripcion,
    string? IdMarca,
    string? CategoriaId,
    DateTime? FechaVencimiento,
    string? ImagenPrincipal);
