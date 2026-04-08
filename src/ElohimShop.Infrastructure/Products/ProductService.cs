using ElohimShop.Application.Products;
using ElohimShop.Domain.Entities;
using ElohimShop.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ElohimShop.Infrastructure.Products;

public class ProductService : IProductService
{
    private readonly ElohimShopDbContext _dbContext;

    public ProductService(ElohimShopDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ProductResponseDto> CreateAsync(CreateProductRequestDto request, CancellationToken cancellationToken)
    {
        var codigoProducto = request.CodigoProducto.Trim();

        var exists = await _dbContext.Productos
            .AnyAsync(producto => producto.CodigoProducto == codigoProducto, cancellationToken);

        if (exists)
        {
            throw new InvalidOperationException("Ya existe un producto con ese codigo.");
        }

        var producto = Producto.Crear(
            codigoProducto,
            request.NombreProducto,
            request.Precio,
            request.StockActual,
            request.Descripcion,
            request.IdMarca,
            request.CategoriaId,
            request.FechaVencimiento,
            request.ImagenPrincipal);

        _dbContext.Productos.Add(producto);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return MapToResponse(producto);
    }

    public async Task<IReadOnlyCollection<ProductResponseDto>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await _dbContext.Productos
            .AsNoTracking()
            .OrderBy(producto => producto.FechaCreacion)
            .Select(producto => new ProductResponseDto(
                producto.IdProducto,
                producto.CodigoProducto,
                producto.NombreProducto,
                producto.Precio,
                producto.StockActual,
                producto.Descripcion,
                producto.IdMarca,
                producto.CategoriaId,
                producto.FechaVencimiento,
                producto.ImagenPrincipal,
                producto.FechaCreacion,
                producto.FechaActualizacion))
            .ToListAsync(cancellationToken);
    }

    private static ProductResponseDto MapToResponse(Producto producto)
    {
        return new ProductResponseDto(
            producto.IdProducto,
            producto.CodigoProducto,
            producto.NombreProducto,
            producto.Precio,
            producto.StockActual,
            producto.Descripcion,
            producto.IdMarca,
            producto.CategoriaId,
            producto.FechaVencimiento,
            producto.ImagenPrincipal,
            producto.FechaCreacion,
            producto.FechaActualizacion);
    }
}
