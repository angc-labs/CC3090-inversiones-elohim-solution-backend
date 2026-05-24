using System.Security.Claims;
using ElohimShop.Application.Products;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ElohimShop.API.Controllers;

[ApiController]
[Route("api/productos")]
[Authorize]
public class ProductosController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductosController(IProductService productService)
    {
        _productService = productService;
    }

    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ProductResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Actualizar(string id, [FromBody] UpdateProductRequestDto request, CancellationToken cancellationToken)
    {
        if (!EsAdministrador())
        {
            return Forbid();
        }

        try
        {
            var producto = await _productService.UpdateAsync(id, request, cancellationToken);
            return Ok(producto);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Eliminar(string id, CancellationToken cancellationToken)
    {
        if (!EsAdministrador())
        {
            return Forbid();
        }

        try
        {
            await _productService.DeleteAsync(id, cancellationToken);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    private bool EsAdministrador()
    {
        var tipoUsuario = User.FindFirstValue("tipo_usuario") ?? User.FindFirstValue("tipoUsuario");
        if (!string.Equals(tipoUsuario, "administrador", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        var rol = User.FindFirstValue("rol");
        return rol is null or "administrador" or "admin";
    }
}
