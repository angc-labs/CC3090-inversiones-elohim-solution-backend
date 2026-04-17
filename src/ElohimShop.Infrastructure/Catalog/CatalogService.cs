using ElohimShop.Application.Catalog;
using ElohimShop.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ElohimShop.Infrastructure.Catalog;

public class CatalogService : ICatalogService
{
    private readonly ElohimShopDbContext _dbContext;

    public CatalogService(ElohimShopDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<MarcaDto>> ObtenerMarcasAsync(CancellationToken cancellationToken)
    {
        return await _dbContext.Marcas
            .AsNoTracking()
            .OrderBy(m => m.NombreMarca)
            .Select(m => new MarcaDto
            {
                Id = m.Id,
                NombreMarca = m.NombreMarca,
                Descripcion = m.Descripcion
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<CategoriaDto>> ObtenerCategoriasAsync(CancellationToken cancellationToken)
    {
        return await _dbContext.Categorias
            .AsNoTracking()
            .OrderBy(c => c.NombreCategoria)
            .Select(c => new CategoriaDto
            {
                Id = c.Id,
                NombreCategoria = c.NombreCategoria,
                Descripcion = c.Descripcion,
                FechaCreacion = c.FechaCreacion
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<ProductoPaginacionDto> ObtenerProductosAsync(
        string? categoriaId,
        string? marcaId,
        int pagina,
        int limite,
        CancellationToken cancellationToken)
    {
        var query = _dbContext.Productos.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(categoriaId))
        {
            query = query.Where(p => p.CategoriaId == categoriaId);
        }

        if (!string.IsNullOrWhiteSpace(marcaId))
        {
            query = query.Where(p => p.IdMarca == marcaId);
        }

        var total = await query.CountAsync(cancellationToken);

        var productos = await query
            .OrderByDescending(p => p.FechaCreacion)
            .Skip((pagina - 1) * limite)
            .Take(limite)
            .Select(p => new ProductoListadoDto
            {
                IdProducto = p.IdProducto,
                CodigoProducto = p.CodigoProducto,
                NombreProducto = p.NombreProducto,
                Descripcion = p.Descripcion,
                Precio = p.Precio,
                StockActual = p.StockActual,
                IdMarca = p.IdMarca,
                CategoriaId = p.CategoriaId,
                ImagenPrincipal = p.ImagenPrincipal,
                FechaVencimiento = p.FechaVencimiento
            })
            .ToListAsync(cancellationToken);

        return new ProductoPaginacionDto
        {
            Total = total,
            Pagina = pagina,
            Limite = limite,
            Productos = productos
        };
    }

    public async Task<ProductoDetalleDto?> ObtenerProductoPorIdAsync(string id, CancellationToken cancellationToken)
    {
        var producto = await _dbContext.Productos
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.IdProducto == id, cancellationToken);

        if (producto is null)
        {
            return null;
        }

        return new ProductoDetalleDto
        {
            IdProducto = producto.IdProducto,
            CodigoProducto = producto.CodigoProducto,
            NombreProducto = producto.NombreProducto,
            Descripcion = producto.Descripcion,
            Precio = producto.Precio,
            StockActual = producto.StockActual,
            IdMarca = producto.IdMarca,
            CategoriaId = producto.CategoriaId,
            ImagenPrincipal = producto.ImagenPrincipal,
            FechaVencimiento = producto.FechaVencimiento,
            FechaCreacion = producto.FechaCreacion,
            FechaActualizacion = producto.FechaActualizacion
        };
    }

    public async Task<BusquedaProductosDto> BuscarProductosAsync(string query, CancellationToken cancellationToken)
    {
        var normalizedQuery = query.Trim().ToLower();

        var resultados = await _dbContext.Productos
            .AsNoTracking()
            .Where(p => p.NombreProducto.ToLower().Contains(normalizedQuery))
            .OrderBy(p => p.NombreProducto)
            .Take(20)
            .Select(p => new ProductoBusquedaDto
            {
                IdProducto = p.IdProducto,
                NombreProducto = p.NombreProducto,
                Precio = p.Precio,
                ImagenPrincipal = p.ImagenPrincipal
            })
            .ToListAsync(cancellationToken);

        return new BusquedaProductosDto
        {
            Query = query.Trim(),
            Resultados = resultados
        };
    }
}
