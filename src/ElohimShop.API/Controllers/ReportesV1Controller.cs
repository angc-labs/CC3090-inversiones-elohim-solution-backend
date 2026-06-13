using ElohimShop.Application.Platform;
using Microsoft.AspNetCore.Mvc;

namespace ElohimShop.API.Controllers;

[ApiController]
[Route("api/v1/reportes")]
public class ReportesV1Controller : V1ControllerBase
{
    private readonly IPlatformService _platformService;

    public ReportesV1Controller(IPlatformService platformService)
    {
        _platformService = platformService;
    }

    [HttpPost("ejecutar-raw")]
    public async Task<IActionResult> EjecutarRaw([FromBody] EjecutarRawReporteRequest request, CancellationToken cancellationToken)
    {
        if (GetTenantId() is null || !EsStaff())
        {
            return Forbid();
        }

        return Ok(await _platformService.EjecutarRawReporteAsync(request, cancellationToken));
    }

    [HttpPost("guardar")]
    public async Task<IActionResult> Guardar([FromBody] GuardarReporteRequest request, CancellationToken cancellationToken)
    {
        if (GetTenantId() is null || !EsStaff())
        {
            return Forbid();
        }

        var reporte = await _platformService.GuardarReporteAsync(request, GetUserId(), cancellationToken);
        return StatusCode(StatusCodes.Status201Created, reporte);
    }

    [HttpGet]
    public async Task<IActionResult> Listar(CancellationToken cancellationToken)
    {
        if (GetTenantId() is null || !EsStaff())
        {
            return Forbid();
        }

        return Ok(await _platformService.ListarReportesAsync(cancellationToken));
    }

    [HttpGet("{id}/correr")]
    public async Task<IActionResult> Correr(string id, CancellationToken cancellationToken)
    {
        if (GetTenantId() is null || !EsStaff())
        {
            return Forbid();
        }

        return Ok(await _platformService.CorrerReporteAsync(id, cancellationToken));
    }
}