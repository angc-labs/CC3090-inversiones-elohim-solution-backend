namespace ElohimShop.Application.Products;

public record BulkCreateProductErrorDto(
    string CodigoProducto,
    string Error);
