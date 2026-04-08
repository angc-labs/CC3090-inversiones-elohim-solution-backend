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

    public async Task<BulkCreateProductsResultDto> CreateManyAsync(
        IReadOnlyCollection<CreateProductRequestDto> requests,
        CancellationToken cancellationToken)
    {
        if (requests.Count == 0)
        {
            return new BulkCreateProductsResultDto(0, 0, 0, Array.Empty<ProductResponseDto>(), Array.Empty<BulkCreateProductErrorDto>());
        }

        var errores = new List<BulkCreateProductErrorDto>();
        var validRequests = new List<CreateProductRequestDto>();
        var codigosEnLote = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var request in requests)
        {
            var codigo = request.CodigoProducto?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(codigo) || string.IsNullOrWhiteSpace(request.NombreProducto))
            {
                errores.Add(new BulkCreateProductErrorDto(codigo, "CodigoProducto y NombreProducto son requeridos."));
                continue;
            }

            if (request.Precio < 0 || request.StockActual < 0)
            {
                errores.Add(new BulkCreateProductErrorDto(codigo, "Precio y StockActual no pueden ser negativos."));
                continue;
            }

            if (!codigosEnLote.Add(codigo))
            {
                errores.Add(new BulkCreateProductErrorDto(codigo, "CodigoProducto repetido dentro del mismo lote."));
                continue;
            }

            validRequests.Add(request with { CodigoProducto = codigo });
        }

        var marcaIds = validRequests
            .Where(request => !string.IsNullOrWhiteSpace(request.IdMarca))
            .Select(request => request.IdMarca!)
            .Distinct()
            .ToList();

        var categoriaIds = validRequests
            .Where(request => !string.IsNullOrWhiteSpace(request.CategoriaId))
            .Select(request => request.CategoriaId!)
            .Distinct()
            .ToList();

        var marcasExistentes = await _dbContext.Marcas
            .AsNoTracking()
            .Where(marca => marcaIds.Contains(marca.Id))
            .Select(marca => marca.Id)
            .ToListAsync(cancellationToken);

        var categoriasExistentes = await _dbContext.Categorias
            .AsNoTracking()
            .Where(categoria => categoriaIds.Contains(categoria.Id))
            .Select(categoria => categoria.Id)
            .ToListAsync(cancellationToken);

        var marcasExistentesSet = marcasExistentes.ToHashSet();
        var categoriasExistentesSet = categoriasExistentes.ToHashSet();

        var validWithRelations = new List<CreateProductRequestDto>();
        foreach (var request in validRequests)
        {
            var codigo = request.CodigoProducto;

            if (!string.IsNullOrWhiteSpace(request.IdMarca) && !marcasExistentesSet.Contains(request.IdMarca))
            {
                errores.Add(new BulkCreateProductErrorDto(codigo, "IdMarca no existe."));
                continue;
            }

            if (!string.IsNullOrWhiteSpace(request.CategoriaId) && !categoriasExistentesSet.Contains(request.CategoriaId))
            {
                errores.Add(new BulkCreateProductErrorDto(codigo, "CategoriaId no existe."));
                continue;
            }

            validWithRelations.Add(request);
        }

        var codigos = validWithRelations.Select(request => request.CodigoProducto).ToList();
        var codigosExistentesDb = await _dbContext.Productos
            .AsNoTracking()
            .Where(producto => codigos.Contains(producto.CodigoProducto))
            .Select(producto => producto.CodigoProducto)
            .ToListAsync(cancellationToken);

        var codigosExistentesSet = codigosExistentesDb.ToHashSet(StringComparer.OrdinalIgnoreCase);

        var productosNuevos = new List<Producto>();
        foreach (var request in validWithRelations)
        {
            if (codigosExistentesSet.Contains(request.CodigoProducto))
            {
                errores.Add(new BulkCreateProductErrorDto(request.CodigoProducto, "Ya existe un producto con ese codigo."));
                continue;
            }

            var producto = Producto.Crear(
                request.CodigoProducto,
                request.NombreProducto,
                request.Precio,
                request.StockActual,
                request.Descripcion,
                request.IdMarca,
                request.CategoriaId,
                request.FechaVencimiento,
                request.ImagenPrincipal);

            productosNuevos.Add(producto);
        }

        if (productosNuevos.Count > 0)
        {
            _dbContext.Productos.AddRange(productosNuevos);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        var creados = productosNuevos
            .Select(MapToResponse)
            .ToList();

        return new BulkCreateProductsResultDto(
            requests.Count,
            creados.Count,
            errores.Count,
            creados,
            errores);
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
