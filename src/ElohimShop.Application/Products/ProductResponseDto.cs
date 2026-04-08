namespace ElohimShop.Application.Products;

public record ProductResponseDto(
    string IdProducto,
    string CodigoProducto,
    string NombreProducto,
    int Precio,
    int StockActual,
    string? Descripcion,
    string? IdMarca,
    string? CategoriaId,
    DateTime FechaVencimiento,
    string? ImagenPrincipal,
    DateTime FechaCreacion,
    DateTime FechaActualizacion);
