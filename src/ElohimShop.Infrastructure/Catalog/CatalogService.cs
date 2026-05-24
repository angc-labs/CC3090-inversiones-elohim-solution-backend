using ElohimShop.Application.Catalog;
using ElohimShop.Domain.Entities;
using ElohimShop.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

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
            FechaActualizacion = producto.FechaActualizacion,
            EnOferta = producto.EnOferta,
            PrecioOferta = producto.PrecioOferta,
            FechaFinOferta = producto.FechaFinOferta
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

    public async Task<SeedCatalogoResultadoDto> SeedCatalogoAsync(int cantidadProductos, CancellationToken cancellationToken)
    {
        if (cantidadProductos <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(cantidadProductos), "La cantidad de productos debe ser mayor a 0.");
        }

        var categoriasObjetivo = CategoriasSeed.Select(c => c.Nombre).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var marcasObjetivo = MarcasSeed.Select(m => m.Nombre).ToHashSet(StringComparer.OrdinalIgnoreCase);

        var categoriasExistentes = await _dbContext.Categorias
            .Where(c => categoriasObjetivo.Contains(c.NombreCategoria))
            .ToListAsync(cancellationToken);

        var marcasExistentes = await _dbContext.Marcas
            .Where(m => marcasObjetivo.Contains(m.NombreMarca))
            .ToListAsync(cancellationToken);

        var categoriasNuevas = CategoriasSeed
            .Where(seed => categoriasExistentes.All(c => !string.Equals(c.NombreCategoria, seed.Nombre, StringComparison.OrdinalIgnoreCase)))
            .Select(seed => Categoria.Crear(seed.Nombre, seed.Descripcion))
            .ToList();

        var marcasNuevas = MarcasSeed
            .Where(seed => marcasExistentes.All(m => !string.Equals(m.NombreMarca, seed.Nombre, StringComparison.OrdinalIgnoreCase)))
            .Select(seed => Marca.Crear(seed.Nombre, seed.Descripcion))
            .ToList();

        if (categoriasNuevas.Count > 0)
        {
            _dbContext.Categorias.AddRange(categoriasNuevas);
        }

        if (marcasNuevas.Count > 0)
        {
            _dbContext.Marcas.AddRange(marcasNuevas);
        }

        if (categoriasNuevas.Count > 0 || marcasNuevas.Count > 0)
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        var categorias = await _dbContext.Categorias
            .Where(c => categoriasObjetivo.Contains(c.NombreCategoria))
            .ToListAsync(cancellationToken);

        var marcas = await _dbContext.Marcas
            .Where(m => marcasObjetivo.Contains(m.NombreMarca))
            .ToListAsync(cancellationToken);

        var codigosObjetivo = Enumerable.Range(1, cantidadProductos)
            .Select(i => $"SEED-PROD-{i:000}")
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var codigosExistentes = await _dbContext.Productos
            .Where(p => codigosObjetivo.Contains(p.CodigoProducto))
            .Select(p => p.CodigoProducto)
            .ToListAsync(cancellationToken);

        var codigosExistentesSet = codigosExistentes.ToHashSet(StringComparer.OrdinalIgnoreCase);
        var productosNuevos = new List<Producto>(cantidadProductos);

        for (var i = 1; i <= cantidadProductos; i++)
        {
            var codigo = $"SEED-PROD-{i:000}";
            if (codigosExistentesSet.Contains(codigo))
            {
                continue;
            }

            var categoria = categorias[(i - 1) % categorias.Count];
            var marca = marcas[(i - 1) % marcas.Count];
            var imagenPrincipal = ImagenesPrincipalSeed[(i - 1) % ImagenesPrincipalSeed.Length];

            var producto = Producto.Crear(
                codigo,
                $"Producto Seed {i:000}",
                50 + (i * 10),
                10 + (i % 25),
                descripcion: $"Producto autogenerado para seed {i:000}.",
                idMarca: marca.Id,
                categoriaId: categoria.Id,
                fechaVencimiento: DateTime.UtcNow.AddMonths(12 + (i % 6)),
                imagenPrincipal: imagenPrincipal);

            productosNuevos.Add(producto);
        }

        if (productosNuevos.Count > 0)
        {
            _dbContext.Productos.AddRange(productosNuevos);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        return new SeedCatalogoResultadoDto(
            cantidadProductos,
            categoriasNuevas.Count,
            marcasNuevas.Count,
            productosNuevos.Count,
            cantidadProductos - productosNuevos.Count);
    }
}
