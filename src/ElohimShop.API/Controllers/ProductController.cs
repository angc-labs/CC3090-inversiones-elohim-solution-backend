using ElohimShop.Application.Products;
using Microsoft.AspNetCore.Mvc;

namespace ElohimShop.API.Controllers;

[ApiController]
[Route("api/product")]
public class ProductController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductController(IProductService productService)
    {
        _productService = productService;
    }

    /// <summary>
    /// Crea un producto nuevo.
    /// </summary>
    /// <param name="request">Datos del producto a crear.</param>
    /// <param name="cancellationToken">Token de cancelacion.</param>
    /// <returns>Producto creado.</returns>
    /// <response code="200">Producto creado correctamente.</response>
    /// <response code="400">Solicitud invalida.</response>
    /// <response code="409">El codigo del producto ya existe.</response>
    [HttpPost]
    [ProducesResponseType(typeof(ProductResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] CreateProductRequestDto request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.CodigoProducto) || string.IsNullOrWhiteSpace(request.NombreProducto))
        {
            return BadRequest(new { message = "CodigoProducto y NombreProducto son requeridos." });
        }

        if (request.Precio < 0 || request.StockActual < 0)
        {
            return BadRequest(new { message = "Precio y StockActual no pueden ser negativos." });
        }

        try
        {
            var result = await _productService.CreateAsync(request, cancellationToken);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Obtiene todos los productos registrados.
    /// </summary>
    /// <param name="cancellationToken">Token de cancelacion.</param>
    /// <returns>Lista de productos.</returns>
    /// <response code="200">Lista de productos obtenida correctamente.</response>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyCollection<ProductResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var products = await _productService.GetAllAsync(cancellationToken);
        return Ok(products);
    }
}
