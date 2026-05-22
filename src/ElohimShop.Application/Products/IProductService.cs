namespace ElohimShop.Application.Products;

public interface IProductService
{
    Task<ProductResponseDto> CreateAsync(CreateProductRequestDto request, CancellationToken cancellationToken);
    Task<BulkCreateProductsResultDto> CreateManyAsync(IReadOnlyCollection<CreateProductRequestDto> requests, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<ProductResponseDto>> GetAllAsync(CancellationToken cancellationToken);
    Task<ProductResponseDto?> UpdateAsync(string id, UpdateProductRequestDto request, CancellationToken cancellationToken);
    Task<ProductResponseDto?> UpdateStockAsync(string id, UpdateStockRequestDto request, CancellationToken cancellationToken);
    Task<ProductResponseDto?> CreateOfferAsync(string id, CreateProductOfferRequestDto request, CancellationToken cancellationToken);
    Task<bool> DeleteOfferAsync(string id, CancellationToken cancellationToken);
}
