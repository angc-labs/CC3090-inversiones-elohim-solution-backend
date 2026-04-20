using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using ElohimShop.Application.Reservacion;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ElohimShop.API.Controllers;

[ApiController]
[Route("api/reservacion")]
[Authorize]
public class ReservacionController : ControllerBase
{
    private readonly IReservacionService _reservacionService;

    public ReservacionController(IReservacionService reservacionService)
    {
        _reservacionService = reservacionService;
    }

    [HttpPost]
    [ProducesResponseType(typeof(ReservacionDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CrearReservacion([FromBody] CrearReservacionDto dto, CancellationToken cancellationToken)
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

        if (string.IsNullOrWhiteSpace(dto.MetodoPagoId))
        {
            return BadRequest(new { error = "El método de pago es requerido." });
        }

        try
        {
            var reservacion = await _reservacionService.CrearReservacionAsync(clienteId, dto, cancellationToken);
            return StatusCode(201, reservacion);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<ReservacionListadoDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ObtenerReservaciones(CancellationToken cancellationToken)
    {
        var tipoUsuario = User.FindFirstValue("tipo_usuario");
        var esAdministrador = tipoUsuario == "administrador";
        
        var clienteId = esAdministrador
            ? null
            : User.FindFirstValue(JwtRegisteredClaimNames.Sub)
                ?? User.FindFirstValue(ClaimTypes.NameIdentifier);

        var reservaciones = await _reservacionService.ObtenerReservacionesAsync(clienteId, esAdministrador, cancellationToken);
        return Ok(reservaciones);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ReservacionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObtenerReservacionPorId(string id, CancellationToken cancellationToken)
    {
        var tipoUsuario = User.FindFirstValue("tipo_usuario");
        var esAdministrador = tipoUsuario == "administrador";
        
        var clienteId = User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier);

        try
        {
            var reservacion = await _reservacionService.ObtenerReservacionPorIdAsync(id, clienteId, esAdministrador, cancellationToken);
            
            if (reservacion is null)
            {
                return NotFound(new { error = "Reservación no encontrada." });
            }

            return Ok(reservacion);
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, new { error = ex.Message });
        }
    }
}