using ElohimShop.Application.Catalog;
using ElohimShop.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace ElohimShop.Infrastructure.Catalog;

public class CatalogService : ICatalogService
{
    private static readonly IReadOnlyList<(string Nombre, string Descripcion)> CategoriasSeed =
    [
        ("CatSeed01", "Categoria seed tecnologia"),
        ("CatSeed02", "Categoria seed hogar"),
        ("CatSeed03", "Categoria seed deporte"),
        ("CatSeed04", "Categoria seed oficina"),
        ("CatSeed05", "Categoria seed salud")
    ];

    private static readonly IReadOnlyList<(string Nombre, string Descripcion)> MarcasSeed =
    [
        ("MarcaSeed01", "Marca seed uno"),
        ("MarcaSeed02", "Marca seed dos"),
        ("MarcaSeed03", "Marca seed tres"),
        ("MarcaSeed04", "Marca seed cuatro"),
        ("MarcaSeed05", "Marca seed cinco")
    ];

    private static readonly string[] ImagenesPrincipalSeed =
    [
        "https://www.elcampofoods.com/cdn/shop/files/nutrileche1.png",
        "https://cdn.kemik.gt/2024/03/D1017-1200x1200-02.jpg",
        "https://www.smartnfinal.com.mx/wp-content/uploads/2016/08/MANZNA-ROJA.jpg",
        "https://pamsdailydish.com/wp-content/uploads/2015/04/Bunch-Bananas-1.jpg",
        "https://www.ammarket.com/wp-content/uploads/2021/12/NARANJA_MESA_AMMARKET.COM_2.jpg",
        "https://www.gastronomiavasca.net/uploads/image/file/3415/pi_a.jpg",
        "https://walmartgt.vtexassets.com/arquivos/ids/519015/Fresa-Clamshell-1-Libra-1-31924.jpg",
        "https://www.gastronomiavasca.net/uploads/image/file/3395/mora.jpg",
        "https://walmartgt.vtexassets.com/arquivos/ids/583581/14893_01.jpg?v=638602812471630000",
        "https://img.pacifiko.com/PROD/resize/0/1000x1000/105029.jpg",
        "https://walmartgt.vtexassets.com/arquivos/ids/800195-800-450?v=638809755231730000&width=800&height=450&aspect=true",
        "https://latorremx.vtexassets.com/arquivos/ids/190348/61832-frontal.jpg?v=638494042144500000",
        "https://superexitus.com/wp-content/uploads/2024/11/carton_de_huevos_superexitus.jpg",
        "https://walmartgt.vtexassets.com/arquivos/ids/772830/7487_01.jpg?v=638763948695500000",
        "https://walmartgt.vtexassets.com/arquivos/ids/653805/7709_01.jpg?v=638658209672500000"
    ];

    private readonly PlatformDbContext _dbContext;
    private readonly ITenantProvider _tenantProvider;

    public CatalogService(PlatformDbContext dbContext, ITenantProvider tenantProvider)
    {
        _dbContext = dbContext;
        _tenantProvider = tenantProvider;
    }

    public Task<IReadOnlyList<MarcaDto>> ObtenerMarcasAsync(CancellationToken cancellationToken)
    {
        // El nuevo modelo PlatformDbContext no tiene la tabla Marcas. Retornamos marcas simuladas.
        IReadOnlyList<MarcaDto> marcas = MarcasSeed.Select(m => new MarcaDto
        {
            Id = m.Nombre.ToLowerInvariant(),
            NombreMarca = m.Nombre,
            Descripcion = m.Descripcion
        }).ToList();

        return Task.FromResult(marcas);
    }

    public async Task<IReadOnlyList<CategoriaDto>> ObtenerCategoriasAsync(CancellationToken cancellationToken)
    {
        // Consultar categorías de la tienda activa inyectada por ITenantProvider
        return await _dbContext.Categorias
            .AsNoTracking()
            .OrderBy(c => c.Nombre)
            .Select(c => new CategoriaDto
            {
                Id = c.Id,
                NombreCategoria = c.Nombre,
                Descripcion = c.Descripcion,
                FechaCreacion = DateTime.UtcNow // Valor mock
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

        var total = await query.CountAsync(cancellationToken);

        var productos = await query
            .OrderByDescending(p => p.FechaCreacion)
            .Skip((pagina - 1) * limite)
            .Take(limite)
            .Select(p => new ProductoListadoDto
            {
                IdProducto = p.Id,
                CodigoProducto = p.Sku ?? string.Empty,
                NombreProducto = p.Nombre,
                Descripcion = p.Descripcion,
                Precio = (int)p.PrecioDetalle,
                StockActual = p.Inventarios.Sum(i => i.Stock),
                IdMarca = "marca-demo",
                CategoriaId = p.CategoriaId,
                ImagenPrincipal = p.ImagenUrl,
                FechaVencimiento = DateTime.UtcNow.AddYears(1)
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
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

        if (producto is null)
        {
            return null;
        }

        var stockActual = await _dbContext.Inventarios
            .Where(i => i.ProductoId == producto.Id)
            .SumAsync(i => i.Stock, cancellationToken);

        return new ProductoDetalleDto
        {
            IdProducto = producto.Id,
            CodigoProducto = producto.Sku ?? string.Empty,
            NombreProducto = producto.Nombre,
            Descripcion = producto.Descripcion,
            Precio = (int)producto.PrecioDetalle,
            StockActual = stockActual,
            IdMarca = "marca-demo",
            CategoriaId = producto.CategoriaId,
            ImagenPrincipal = producto.ImagenUrl,
            FechaVencimiento = DateTime.UtcNow.AddYears(1),
            FechaCreacion = producto.FechaCreacion,
            FechaActualizacion = producto.FechaCreacion
        };
    }

    public async Task<BusquedaProductosDto> BuscarProductosAsync(string query, CancellationToken cancellationToken)
    {
        var normalizedQuery = query.Trim().ToLower();

        var resultados = await _dbContext.Productos
            .AsNoTracking()
            .Where(p => p.Nombre.ToLower().Contains(normalizedQuery))
            .OrderBy(p => p.Nombre)
            .Take(20)
            .Select(p => new ProductoBusquedaDto
            {
                IdProducto = p.Id,
                NombreProducto = p.Nombre,
                Precio = (int)p.PrecioDetalle,
                ImagenPrincipal = p.ImagenUrl
            })
            .ToListAsync(cancellationToken);

        return new BusquedaProductosDto
        {
            Query = query.Trim(),
            Resultados = resultados
        };
    }

    public async Task<SeedCatalogoResultadoDto> SeedCatalogoAsync(int cantidadProductos, CancellationToken cancellationToken)
    {
        if (cantidadProductos <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(cantidadProductos), "La cantidad de productos debe ser mayor a 0.");
        }

        var tenantId = _tenantProvider.GetTenantId();
        if (string.IsNullOrWhiteSpace(tenantId))
        {
            var primeraTienda = await _dbContext.Tiendas.FirstOrDefaultAsync(cancellationToken);
            tenantId = primeraTienda?.Id ?? "seed-tienda-default-id";
        }

        var categoriasObjetivo = CategoriasSeed.Select(c => c.Nombre).ToHashSet(StringComparer.OrdinalIgnoreCase);

        var categoriasExistentes = await _dbContext.Categorias
            .Where(c => categoriasObjetivo.Contains(c.Nombre))
            .ToListAsync(cancellationToken);

        var categoriasNuevas = CategoriasSeed
            .Where(seed => categoriasExistentes.All(c => !string.Equals(c.Nombre, seed.Nombre, StringComparison.OrdinalIgnoreCase)))
            .Select(seed => new ElohimShop.Domain.Platform.Categoria
            {
                Id = Guid.NewGuid().ToString(),
                TiendaId = tenantId,
                Nombre = seed.Nombre,
                Descripcion = seed.Descripcion,
                Slug = seed.Nombre.ToLowerInvariant(),
                ImagenUrl = string.Empty
            })
            .ToList();

        if (categoriasNuevas.Count > 0)
        {
            _dbContext.Categorias.AddRange(categoriasNuevas);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        var categorias = await _dbContext.Categorias
            .Where(c => categoriasObjetivo.Contains(c.Nombre))
            .ToListAsync(cancellationToken);

        var codigosObjetivo = Enumerable.Range(1, cantidadProductos)
            .Select(i => $"SEED-PROD-{i:000}")
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var codigosExistentes = await _dbContext.Productos
            .Where(p => codigosObjetivo.Contains(p.Sku))
            .Select(p => p.Sku)
            .ToListAsync(cancellationToken);

        var codigosExistentesSet = codigosExistentes.ToHashSet(StringComparer.OrdinalIgnoreCase);
        var productosNuevos = new List<ElohimShop.Domain.Platform.Producto>(cantidadProductos);

        var sucursal = await _dbContext.Sucursales.FirstOrDefaultAsync(s => s.TiendaId == tenantId, cancellationToken);

        for (var i = 1; i <= cantidadProductos; i++)
        {
            var codigo = $"SEED-PROD-{i:000}";
            if (codigosExistentesSet.Contains(codigo))
            {
                continue;
            }

            var categoria = categorias[(i - 1) % categorias.Count];
            var imagenPrincipal = ImagenesPrincipalSeed[(i - 1) % ImagenesPrincipalSeed.Length];

            var producto = new ElohimShop.Domain.Platform.Producto
            {
                Id = Guid.NewGuid().ToString(),
                TiendaId = tenantId,
                CategoriaId = categoria.Id,
                Nombre = $"Producto Seed {i:000}",
                Descripcion = $"Producto autogenerado para seed {i:000}.",
                Sku = codigo,
                PrecioMayoreo = 50 + (i * 10),
                PrecioDetalle = 50 + (i * 10),
                ImagenUrl = imagenPrincipal,
                Publicado = true,
                FechaCreacion = DateTime.UtcNow,
                Eliminado = false
            };

            productosNuevos.Add(producto);

            if (sucursal is not null)
            {
                var inventario = new ElohimShop.Domain.Platform.Inventario
                {
                    Id = Guid.NewGuid().ToString(),
                    TiendaId = tenantId,
                    SucursalId = sucursal.Id,
                    ProductoId = producto.Id,
                    Stock = 10 + (i % 25)
                };
                _dbContext.Inventarios.Add(inventario);
            }
        }

        if (productosNuevos.Count > 0)
        {
            _dbContext.Productos.AddRange(productosNuevos);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        return new SeedCatalogoResultadoDto(
            cantidadProductos,
            categoriasNuevas.Count,
            0, // marcasNuevas
            productosNuevos.Count,
            cantidadProductos - productosNuevos.Count);
    }
}
