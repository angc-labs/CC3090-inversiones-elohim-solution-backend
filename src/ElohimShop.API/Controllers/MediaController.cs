using ElohimShop.Application.Platform;
using Microsoft.AspNetCore.Mvc;

namespace ElohimShop.API.Controllers;

[ApiController]
[Route("api/v1/media")]
public class MediaController : V1ControllerBase
{
    private readonly IPlatformService _platformService;

    public MediaController(IPlatformService platformService)
    {
        _platformService = platformService;
    }

    [HttpGet("cloudinary-signature")]
    public async Task<IActionResult> ObtenerFirma([FromQuery] string publicId, [FromQuery] long? timestamp, [FromQuery] string? folder, CancellationToken cancellationToken)
    {
        if (GetTenantId() is null)
        {
            return BadRequest(new { error = "Se requiere el header X-Tenant-ID." });
        }

        if (!EsStaff())
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { error = "No tienes permisos para generar firmas de carga de medios." });
        }

        var firma = await _platformService.GenerarFirmaMediaAsync(new MediaSignatureRequest(publicId, timestamp, folder), cancellationToken);
        return Ok(firma);
    }
}
