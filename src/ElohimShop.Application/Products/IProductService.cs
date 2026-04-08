namespace ElohimShop.Application.Products;

public interface IProductService
{
    Task<ProductResponseDto> CreateAsync(CreateProductRequestDto request, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<ProductResponseDto>> GetAllAsync(CancellationToken cancellationToken);
}
