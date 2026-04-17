using ElohimShop.Application.Auth;
using ElohimShop.Application.Catalog;
using ElohimShop.Application.Products;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ElohimShop.API.Controllers;

[ApiController]
[Route("api")]
public class CatalogController : ControllerBase
{
    private readonly ICatalogService _catalogService;
    private readonly IProductService _productService;

    public CatalogController(ICatalogService catalogService, IProductService productService)
    {
        _catalogService = catalogService;
        _productService = productService;
    }

    [HttpGet("marcas")]
    [ProducesResponseType(typeof(IReadOnlyList<MarcaDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ObtenerMarcas(CancellationToken cancellationToken)
    {
        var marcas = await _catalogService.ObtenerMarcasAsync(cancellationToken);
        return Ok(marcas);
    }

    [HttpGet("categorias")]
    [ProducesResponseType(typeof(IReadOnlyList<CategoriaDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ObtenerCategorias(CancellationToken cancellationToken)
    {
        var categorias = await _catalogService.ObtenerCategoriasAsync(cancellationToken);
        return Ok(categorias);
    }

    [HttpGet("productos")]
    [ProducesResponseType(typeof(ProductoPaginacionDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> ObtenerProductos(
        [FromQuery] string? category,
        [FromQuery] string? brand,
        [FromQuery] int page = 1,
        [FromQuery] int limit = 20,
        CancellationToken cancellationToken = default)
    {
        if (page < 1) page = 1;
        if (limit < 1) limit = 1;
        if (limit > 100) limit = 100;

        var resultado = await _catalogService.ObtenerProductosAsync(category, brand, page, limit, cancellationToken);
        return Ok(resultado);
    }

    [HttpGet("productos/buscar")]
    [ProducesResponseType(typeof(BusquedaProductosDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> BuscarProductos(
        [FromQuery] string? q,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(q))
        {
            return BadRequest(new { error = "El parámetro 'q' es requerido." });
        }

        var resultado = await _catalogService.BuscarProductosAsync(q, cancellationToken);
        return Ok(resultado);
    }

    [HttpGet("productos/{id}")]
    [ProducesResponseType(typeof(ProductoDetalleDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObtenerProductoPorId(string id, CancellationToken cancellationToken)
    {
        var producto = await _catalogService.ObtenerProductoPorIdAsync(id, cancellationToken);
        if (producto is null)
        {
            return NotFound(new { error = "Producto no encontrado." });
        }

        return Ok(producto);
    }

    [Authorize(Roles = "administrador")]
    [HttpPost("productos")]
    [ProducesResponseType(typeof(ProductResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CrearProducto(
        [FromBody] CreateProductRequestDto request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.CodigoProducto) || string.IsNullOrWhiteSpace(request.NombreProducto))
        {
            return BadRequest(new { error = "Datos inválidos.", detalles = new[] { "CodigoProducto y NombreProducto son requeridos." } });
        }

        if (request.Precio <= 0)
        {
            return BadRequest(new { error = "Datos inválidos.", detalles = new[] { "El precio debe ser mayor a 0." } });
        }

        try
        {
            var resultado = await _productService.CreateAsync(request, cancellationToken);
            return StatusCode(201, resultado);
        }
        catch (InvalidOperationException)
        {
            return Conflict(new { error = "El código de producto ya existe." });
        }
    }

    [Authorize(Roles = "administrador")]
    [HttpPost("productos/bulk")]
    [ProducesResponseType(typeof(BulkCreateProductsResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CrearProductosBulk(
        [FromBody] IReadOnlyCollection<CreateProductRequestDto> requests,
        CancellationToken cancellationToken)
    {
        if (requests is null || requests.Count == 0)
        {
            return BadRequest(new { error = "Debes enviar al menos un producto." });
        }

        var resultado = await _productService.CreateManyAsync(requests, cancellationToken);
        return Ok(resultado);
    }
}
