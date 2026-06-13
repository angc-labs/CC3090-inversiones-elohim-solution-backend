using ElohimShop.Application.Platform;
using Microsoft.AspNetCore.Mvc;

namespace ElohimShop.API.Controllers;

[ApiController]
[Route("api/v1/inventarios")]
public class InventariosController : V1ControllerBase
{
    private readonly IPlatformService _platformService;

    public InventariosController(IPlatformService platformService)
    {
        _platformService = platformService;
    }

    [HttpGet("sucursal/{sucursalId}")]
    public async Task<IActionResult> ObtenerPorSucursal(string sucursalId, CancellationToken cancellationToken)
    {
        if (GetTenantId() is null)
        {
            return BadRequest(new { error = "Se requiere el header X-Tenant-ID." });
        }

        var inventario = await _platformService.ObtenerInventarioSucursalAsync(sucursalId, cancellationToken);
        return Ok(inventario);
    }

    [HttpPut("ajuste")]
    public async Task<IActionResult> Ajustar([FromBody] AjustarInventarioRequest request, CancellationToken cancellationToken)
    {
        if (GetTenantId() is null)
        {
            return BadRequest(new { error = "Se requiere el header X-Tenant-ID." });
        }

        var inventario = await _platformService.AjustarInventarioAsync(request, cancellationToken);
        return Ok(inventario);
    }
}