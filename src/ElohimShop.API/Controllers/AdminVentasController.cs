using System.Security.Claims;
using ElohimShop.Application.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ElohimShop.API.Controllers;

[ApiController]
[Route("api/admin/ventas")]
[Authorize]
public class AdminVentasController : ControllerBase
{
    private readonly IAdminVentasService _adminVentasService;

    public AdminVentasController(IAdminVentasService adminVentasService)
    {
        _adminVentasService = adminVentasService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(VentasAdminListadoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ObtenerListado(
        [FromQuery] string? busqueda,
        [FromQuery] DateOnly? fecha,
        [FromQuery] string? filtroPrecio,
        [FromQuery] string? filtroMetodoPago,
        CancellationToken cancellationToken)
    {
        if (!EsPanelAdmin())
        {
            return Forbid();
        }

        var resultado = await _adminVentasService.ObtenerListadoAsync(
            busqueda,
            fecha,
            filtroPrecio,
            filtroMetodoPago,
            cancellationToken);

        return Ok(resultado);
    }

    private static bool EsPanelAdmin(ClaimsPrincipal user)
    {
        var tipoUsuario = user.FindFirstValue("tipo_usuario") ?? user.FindFirstValue("tipoUsuario");
        if (string.Equals(tipoUsuario, "administrador", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        var rol = user.FindFirstValue("rol");
        return rol is "administrador" or "cajero" or "admin";
    }

    private bool EsPanelAdmin() => EsPanelAdmin(User);
}
