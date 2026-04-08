using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using ElohimShop.Application.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ElohimShop.API.Controllers;

[ApiController]
[Route("api/client")]
public class ClientController : ControllerBase
{
    private readonly IClientAuthService _clientAuthService;

    public ClientController(IClientAuthService clientAuthService)
    {
        _clientAuthService = clientAuthService;
    }

    /// <summary>
    /// Registra un cliente nuevo y devuelve un JWT.
    /// </summary>
    /// <param name="request">Datos del cliente a registrar.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>Datos del cliente autenticado y su token.</returns>
    /// <response code="200">Cliente creado correctamente.</response>
    /// <response code="409">El correo ya está registrado.</response>
    [HttpPost("register")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register([FromBody] RegisterClientRequestDto request, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _clientAuthService.RegisterAsync(request, cancellationToken);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Inicia sesión de un cliente y devuelve un JWT.
    /// </summary>
    /// <param name="request">Correo y contraseña del cliente.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>Datos del cliente autenticado y su token.</returns>
    /// <response code="200">Inicio de sesión exitoso.</response>
    /// <response code="401">Credenciales inválidas o cuenta inactiva.</response>
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginClientRequestDto request, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _clientAuthService.LoginAsync(request, cancellationToken);
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Revoca el JWT actual.
    /// </summary>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>No content cuando el token fue revocado.</returns>
    /// <response code="204">Token revocado correctamente.</response>
    /// <response code="401">Token inválido, expirado o revocado.</response>
    [Authorize]
    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Logout(CancellationToken cancellationToken)
    {
        var jti = User.FindFirstValue(JwtRegisteredClaimNames.Jti);
        var clienteId = User.FindFirstValue(JwtRegisteredClaimNames.Sub) ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        var expClaim = User.FindFirstValue(JwtRegisteredClaimNames.Exp);

        if (string.IsNullOrWhiteSpace(jti) || string.IsNullOrWhiteSpace(clienteId) || string.IsNullOrWhiteSpace(expClaim))
        {
            return Unauthorized();
        }

        if (!long.TryParse(expClaim, out var expiresAtUnix))
        {
            return Unauthorized();
        }

        var expiresAt = DateTimeOffset.FromUnixTimeSeconds(expiresAtUnix).UtcDateTime;
        await _clientAuthService.LogoutAsync(jti, clienteId, expiresAt, cancellationToken);

        return NoContent();
    }
}