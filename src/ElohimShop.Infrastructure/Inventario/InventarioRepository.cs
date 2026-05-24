using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ElohimShop.Application.Inventario;
using ElohimShop.Application.Inventario.Dtos;
using ElohimShop.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ElohimShop.Infrastructure.Inventario;

public class InventarioRepository : IInventarioRepository
{
    private readonly ElohimShopDbContext _dbContext;

    public InventarioRepository(ElohimShopDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<InventarioResponseDto> GetInventarioAsync(InventarioQuery query, CancellationToken cancellationToken)
    {
        var page = query.Page <= 0 ? 1 : query.Page;
        var limit = query.Limit <= 0 ? 20 : query.Limit;

        var baseQuery = BuildInventarioQuery(query);

        var total = await baseQuery.CountAsync(cancellationToken);

        var productos = await baseQuery
            .Skip((page - 1) * limit)
            .Take(limit)
            .Select(p => new InventarioProductoDto
            {
                IdProducto = p.IdProducto,
                CodigoProducto = p.CodigoProducto,
                NombreProducto = p.NombreProducto,
                Categoria = p.Categoria == null ? null : new InventarioCategoriaDto { Id = p.Categoria.Id, Nombre = p.Categoria.NombreCategoria },
                Marca = p.Marca == null ? null : new InventarioMarcaDto { Id = p.Marca.Id, Nombre = p.Marca.NombreMarca },
                Precio = p.Precio,
                StockActual = p.StockActual,
                StockMinimo = p.StockMinimo,
                Estado = p.StockActual == 0 ? "agotado" : (p.StockActual <= p.StockMinimo ? "critico" : "normal"),
                ValorStock = (long)p.Precio * p.StockActual,
                DescuentoActivo = p.DescuentoPorcentaje.HasValue && p.DescuentoPorcentaje > 0 && p.OfertaHasta.HasValue && p.OfertaHasta > DateTime.UtcNow,
                FechaFinOferta = p.OfertaHasta,
                FechaVencimiento = p.FechaVencimiento,
                ImagenPrincipal = p.ImagenPrincipal
            })
            .ToListAsync(cancellationToken);

        var resumen = await BuildResumenAsync(cancellationToken);

        return new InventarioResponseDto
        {
            Resumen = resumen,
            Productos = productos,
            Total = total,
            Pagina = page,
            Limite = limit
        };
    }

    public async Task<IReadOnlyList<InventarioProductoDto>> GetInventarioProductosAsync(InventarioQuery query, CancellationToken cancellationToken)
    {
        var baseQuery = BuildInventarioQuery(query);

        return await baseQuery
            .Select(p => new InventarioProductoDto
            {
                IdProducto = p.IdProducto,
                CodigoProducto = p.CodigoProducto,
                NombreProducto = p.NombreProducto,
                Categoria = p.Categoria == null ? null : new InventarioCategoriaDto { Id = p.Categoria.Id, Nombre = p.Categoria.NombreCategoria },
                Marca = p.Marca == null ? null : new InventarioMarcaDto { Id = p.Marca.Id, Nombre = p.Marca.NombreMarca },
                Precio = p.Precio,
                StockActual = p.StockActual,
                StockMinimo = p.StockMinimo,
                Estado = p.StockActual == 0 ? "agotado" : (p.StockActual <= p.StockMinimo ? "critico" : "normal"),
                ValorStock = (long)p.Precio * p.StockActual,
                DescuentoActivo = p.DescuentoPorcentaje.HasValue && p.DescuentoPorcentaje > 0 && p.OfertaHasta.HasValue && p.OfertaHasta > DateTime.UtcNow,
                FechaFinOferta = p.OfertaHasta,
                FechaVencimiento = p.FechaVencimiento,
                ImagenPrincipal = p.ImagenPrincipal
            })
            .ToListAsync(cancellationToken);
    }

    private IQueryable<Domain.Entities.Producto> BuildInventarioQuery(InventarioQuery query)
    {
        var q = query.Q?.Trim();
        var baseQuery = _dbContext.Productos.AsNoTracking()
            .Include(p => p.Categoria)
            .Include(p => p.Marca)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(q))
        {
            var normalized = q.ToLower();
            baseQuery = baseQuery.Where(p => p.NombreProducto.ToLower().Contains(normalized) || p.CodigoProducto.ToLower().Contains(normalized));
        }

        if (!string.IsNullOrWhiteSpace(query.CategoriaId))
        {
            baseQuery = baseQuery.Where(p => p.CategoriaId == query.CategoriaId);
        }

        if (!string.IsNullOrWhiteSpace(query.Estado))
        {
            var estadoFilter = query.Estado!.ToLower();
            if (estadoFilter == "agotado")
            {
                baseQuery = baseQuery.Where(p => p.StockActual == 0);
            }
            else if (estadoFilter == "critico")
            {
                baseQuery = baseQuery.Where(p => p.StockActual > 0 && p.StockActual <= p.StockMinimo);
            }
            else if (estadoFilter == "normal")
            {
                baseQuery = baseQuery.Where(p => p.StockActual > p.StockMinimo);
            }
        }

        var orderBy = query.OrderBy?.ToLower();
        var order = query.Order?.ToLower() == "desc" ? "desc" : "asc";

        return orderBy switch
        {
            "nombre" => order == "asc" ? baseQuery.OrderBy(p => p.NombreProducto) : baseQuery.OrderByDescending(p => p.NombreProducto),
            "stockactual" => order == "asc" ? baseQuery.OrderBy(p => p.StockActual) : baseQuery.OrderByDescending(p => p.StockActual),
            "precio" => order == "asc" ? baseQuery.OrderBy(p => p.Precio) : baseQuery.OrderByDescending(p => p.Precio),
            "fechavencimiento" => order == "asc" ? baseQuery.OrderBy(p => p.FechaVencimiento) : baseQuery.OrderByDescending(p => p.FechaVencimiento),
            _ => baseQuery.OrderByDescending(p => p.FechaCreacion)
        };
    }

    private async Task<InventarioResumenDto> BuildResumenAsync(CancellationToken cancellationToken)
    {
        var totalProductos = await _dbContext.Productos.AsNoTracking().CountAsync(cancellationToken);
        var stockCritico = await _dbContext.Productos.AsNoTracking().CountAsync(p => p.StockActual > 0 && p.StockActual <= p.StockMinimo, cancellationToken);
        var stockNormal = await _dbContext.Productos.AsNoTracking().CountAsync(p => p.StockActual > p.StockMinimo, cancellationToken);
        var valorInventario = await _dbContext.Productos.AsNoTracking().SumAsync(p => (long)p.Precio * p.StockActual, cancellationToken);

        return new InventarioResumenDto
        {
            TotalProductos = totalProductos,
            StockNormal = stockNormal,
            StockCritico = stockCritico,
            ValorInventario = valorInventario
        };
    }
}
