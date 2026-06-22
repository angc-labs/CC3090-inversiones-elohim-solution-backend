using System.Security.Claims;
using ElohimShop.Application.Reportes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ElohimShop.API.Controllers;

[ApiController]
[Route("api/admin/reportes")]
[Authorize]
public class ReportesController : ControllerBase
{
    private readonly IReportesService _reportesService;

    public ReportesController(IReportesService reportesService)
    {
        _reportesService = reportesService;
    }

    [HttpGet("productos")]
    [ProducesResponseType(typeof(ReporteProductosDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Productos(
        [FromQuery] DateTime? desde,
        [FromQuery] DateTime? hasta,
        [FromQuery] string? modo,
        CancellationToken cancellationToken)
    {
        if (!EsPanelAdmin())
        {
            return Forbid();
        }

        var resultado = await _reportesService.ObtenerProductosAsync(
            new ReportesFiltroDto(desde, hasta, modo ?? "todos"),
            cancellationToken);

        return Ok(resultado);
    }

    [HttpGet("empleados")]
    [ProducesResponseType(typeof(ReporteEmpleadosDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Empleados(
        [FromQuery] DateTime? desde,
        [FromQuery] DateTime? hasta,
        [FromQuery] string? modo,
        CancellationToken cancellationToken)
    {
        if (!EsPanelAdmin())
        {
            return Forbid();
        }

        var resultado = await _reportesService.ObtenerEmpleadosAsync(
            new ReportesFiltroDto(desde, hasta, modo ?? "todos"),
            cancellationToken);

        return Ok(resultado);
    }

    [HttpGet("stock-critico")]
    [ProducesResponseType(typeof(ReporteStockCriticoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> StockCritico(CancellationToken cancellationToken)
    {
        if (!EsPanelAdmin())
        {
            return Forbid();
        }

        var resultado = await _reportesService.ObtenerStockCriticoAsync(cancellationToken);
        return Ok(resultado);
    }

    [HttpGet("demanda")]
    [ProducesResponseType(typeof(ReporteDemandaDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Demanda(
        [FromQuery] DateTime? desde,
        [FromQuery] DateTime? hasta,
        [FromQuery] string? modo,
        CancellationToken cancellationToken)
    {
        if (!EsPanelAdmin())
        {
            return Forbid();
        }

        var resultado = await _reportesService.ObtenerDemandaAsync(
            new ReportesFiltroDto(desde, hasta, modo ?? "todos"),
            cancellationToken);

        return Ok(resultado);
    }

    [HttpGet("metodos-pago")]
    [ProducesResponseType(typeof(ReporteMetodosPagoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> MetodosPago(
        [FromQuery] DateTime? desde,
        [FromQuery] DateTime? hasta,
        [FromQuery] string? modo,
        CancellationToken cancellationToken)
    {
        if (!EsPanelAdmin())
        {
            return Forbid();
        }

        var resultado = await _reportesService.ObtenerMetodosPagoAsync(
            new ReportesFiltroDto(desde, hasta, modo ?? "todos"),
            cancellationToken);

        return Ok(resultado);
    }

    private bool EsPanelAdmin()
    {
        var tipoUsuario = User.FindFirstValue("tipo_usuario") ?? User.FindFirstValue("tipoUsuario");
        if (string.Equals(tipoUsuario, "administrador", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(tipoUsuario, "admin", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        var rol = User.FindFirstValue("rol") ?? User.FindFirstValue("rol_staff");
        return rol is "administrador" or "admin" or "cajero" or "superadmin";
    }
}
