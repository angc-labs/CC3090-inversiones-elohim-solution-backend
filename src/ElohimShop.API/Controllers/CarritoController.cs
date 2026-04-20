using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using ElohimShop.Application.Carrito;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ElohimShop.API.Controllers;

[ApiController]
[Route("api/carrito")]
[Authorize]
public class CarritoController : ControllerBase
{
    private readonly ICarritoService _carritoService;

    public CarritoController(ICarritoService carritoService)
    {
        _carritoService = carritoService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(CarritoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ObtenerCarrito(CancellationToken cancellationToken)
    {
        var tipoUsuario = User.FindFirstValue("tipo_usuario");
        if (tipoUsuario != "cliente")
        {
            return Forbid();
        }

        var clienteId = User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(clienteId))
        {
            return Unauthorized(new { error = "Token inválido." });
        }

        var carrito = await _carritoService.ObtenerCarritoActivoAsync(clienteId, cancellationToken);
        
        if (carrito is null)
        {
            return Ok(new CarritoDto
            {
                CarritoId = string.Empty,
                Items = new List<ArticuloCarritoDto>(),
                Total = 0
            });
        }

        return Ok(carrito);
    }

    [HttpPost("articulos")]
    [ProducesResponseType(typeof(ArticuloCarritoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> AgregarArticulo([FromBody] AgregarArticuloCarritoDto dto, CancellationToken cancellationToken)
    {
        var tipoUsuario = User.FindFirstValue("tipo_usuario");
        if (tipoUsuario != "cliente")
        {
            return Forbid();
        }

        var clienteId = User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(clienteId))
        {
            return Unauthorized(new { error = "Token inválido." });
        }

        if (string.IsNullOrWhiteSpace(dto.ProductoId))
        {
            return BadRequest(new { error = "El productoId es requerido." });
        }

        if (dto.Cantidad <= 0)
        {
            return BadRequest(new { error = "La cantidad debe ser mayor a 0." });
        }

        try
        {
            var articulo = await _carritoService.AgregarArticuloAsync(clienteId, dto, cancellationToken);
            
            if (articulo is null)
            {
                return BadRequest(new { error = "Producto no encontrado." });
            }

            return Ok(articulo);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPut("articulos/{articuloId}")]
    [ProducesResponseType(typeof(ArticuloCarritoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ActualizarCantidadArticulo(string articuloId, [FromBody] ActualizarCantidadArticuloDto dto, CancellationToken cancellationToken)
    {
        var tipoUsuario = User.FindFirstValue("tipo_usuario");
        if (tipoUsuario != "cliente")
        {
            return Forbid();
        }

        var clienteId = User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(clienteId))
        {
            return Unauthorized(new { error = "Token inválido." });
        }

        var articulo = await _carritoService.ActualizarCantidadArticuloAsync(clienteId, articuloId, dto, cancellationToken);
        
        if (articulo is null)
        {
            return NotFound(new { error = "Artículo no encontrado en el carrito." });
        }

        return Ok(articulo);
    }

    [HttpDelete("articulos/{articuloId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> EliminarArticulo(string articuloId, CancellationToken cancellationToken)
    {
        var tipoUsuario = User.FindFirstValue("tipo_usuario");
        if (tipoUsuario != "cliente")
        {
            return Forbid();
        }

        var clienteId = User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(clienteId))
        {
            return Unauthorized(new { error = "Token inválido." });
        }

        var eliminado = await _carritoService.EliminarArticuloAsync(clienteId, articuloId, cancellationToken);
        
        if (!eliminado)
        {
            return NotFound(new { error = "Artículo no encontrado en el carrito." });
        }

        return NoContent();
    }
}