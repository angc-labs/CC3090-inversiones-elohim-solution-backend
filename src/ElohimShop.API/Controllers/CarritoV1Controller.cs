using ElohimShop.Application.Platform;
using Microsoft.AspNetCore.Mvc;

namespace ElohimShop.API.Controllers;

[ApiController]
[Route("api/v1/carrito")]
public class CarritoV1Controller : V1ControllerBase
{
    private readonly IPlatformService _platformService;

    public CarritoV1Controller(IPlatformService platformService)
    {
        _platformService = platformService;
    }

    [HttpGet]
    public async Task<IActionResult> Obtener(CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (string.IsNullOrWhiteSpace(userId) || GetTenantId() is null)
        {
            return Unauthorized(new { error = "Se requiere autenticación y tenant." });
        }

        return Ok(await _platformService.ObtenerCarritoAsync(userId, cancellationToken));
    }

    [HttpPost("articulos")]
    public async Task<IActionResult> Agregar([FromBody] AgregarCarritoRequest request, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (string.IsNullOrWhiteSpace(userId) || GetTenantId() is null)
        {
            return Unauthorized(new { error = "Se requiere autenticación y tenant." });
        }

        var item = await _platformService.AgregarArticuloAsync(userId, request, cancellationToken);
        return StatusCode(StatusCodes.Status201Created, item);
    }

    [HttpPut("articulos/{id}")]
    public async Task<IActionResult> Actualizar(string id, [FromBody] ActualizarCarritoRequest request, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (string.IsNullOrWhiteSpace(userId) || GetTenantId() is null)
        {
            return Unauthorized(new { error = "Se requiere autenticación y tenant." });
        }

        var item = await _platformService.ActualizarArticuloAsync(userId, id, request, cancellationToken);
        return item is null ? NotFound(new { error = "Artículo no encontrado." }) : Ok(item);
    }

    [HttpDelete("articulos/{id}")]
    public async Task<IActionResult> Eliminar(string id, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (string.IsNullOrWhiteSpace(userId) || GetTenantId() is null)
        {
            return Unauthorized(new { error = "Se requiere autenticación y tenant." });
        }

        var eliminado = await _platformService.EliminarArticuloAsync(userId, id, cancellationToken);
        return eliminado ? NoContent() : NotFound(new { error = "Artículo no encontrado." });
    }
}