using ElohimShop.Application.Platform;
using Microsoft.AspNetCore.Mvc;

namespace ElohimShop.API.Controllers;

[ApiController]
[Route("api/v1/tiendas")]
public class TiendasController : V1ControllerBase
{
    private readonly IPlatformService _platformService;

    public TiendasController(IPlatformService platformService)
    {
        _platformService = platformService;
    }

    [HttpPost]
    public async Task<IActionResult> Crear([FromBody] CrearTiendaRequest request, CancellationToken cancellationToken)
    {
        var tienda = await _platformService.CrearTiendaAsync(request, cancellationToken);
        return CreatedAtAction(nameof(ValidarSlug), new { slug = tienda.Slug }, tienda);
    }

    [HttpGet("valida-slug/{slug}")]
    public async Task<IActionResult> ValidarSlug(string slug, CancellationToken cancellationToken)
    {
        var disponible = await _platformService.SlugDisponibleAsync(slug, cancellationToken);
        return Ok(new { slug, disponible });
    }

    [HttpPut("configuracion-visual")]
    public async Task<IActionResult> ActualizarConfiguracionVisual([FromBody] ActualizarConfiguracionVisualRequest request, CancellationToken cancellationToken)
    {
        if (GetTenantId() is null)
        {
            return BadRequest(new { error = "Se requiere el header X-Tenant-ID." });
        }

        var tienda = await _platformService.ActualizarConfiguracionVisualAsync(request, cancellationToken);
        return Ok(tienda);
    }

    [HttpPost("integraciones")]
    public async Task<IActionResult> GuardarIntegraciones([FromBody] GuardarIntegracionesRequest request, CancellationToken cancellationToken)
    {
        if (GetTenantId() is null)
        {
            return BadRequest(new { error = "Se requiere el header X-Tenant-ID." });
        }

        var tienda = await _platformService.GuardarIntegracionesAsync(request, cancellationToken);
        return Ok(tienda);
    }
}