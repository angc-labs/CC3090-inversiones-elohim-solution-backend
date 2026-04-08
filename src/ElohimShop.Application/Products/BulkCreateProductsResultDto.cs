namespace ElohimShop.Application.Products;

public record BulkCreateProductsResultDto(
    int TotalRecibidos,
    int TotalCreados,
    int TotalFallidos,
    IReadOnlyCollection<ProductResponseDto> Creados,
    IReadOnlyCollection<BulkCreateProductErrorDto> Errores);
