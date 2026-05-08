using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using ElohimShop.Application.Auth;
using ElohimShop.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ElohimShop.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ElohimShopDbContext _dbContext;

    public AuthController(IAuthService authService, ElohimShopDbContext dbContext)
    {
        _authService = authService;
        _dbContext = dbContext;
    }

    [HttpPost("register")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(object), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto request, CancellationToken cancellationToken)
    {
        try
        {
            AuthResponseDto result;

            if (request.TipoUsuario == "administrador")
            {
                var callerRole = User.FindFirstValue("rol");
                var existeAdministrador = await _dbContext.Usuarios
                    .AsNoTracking()
                    .AnyAsync(u => u.TipoUsuario == "administrador", cancellationToken);

                // Bootstrap: si no existe ningun admin, permitimos crear el primero sin token admin.
                if (callerRole != "administrador" && existeAdministrador)
                {
                    return StatusCode(403, new { error = "No tenés permisos para registrar administradores." });
                }
                result = await _authService.RegisterAdminAsync(request, cancellationToken);
            }
            else
            {
                result = await _authService.RegisterAsync(request, cancellationToken);
            }

            return StatusCode(201, result);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { error = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = "Datos inválidos.", detalles = new[] { ex.Message } });
        }
    }

    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _authService.LoginAsync(request, cancellationToken);
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { error = ex.Message });
        }
    }

    [Authorize]
    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Logout(CancellationToken cancellationToken)
    {
        var jti = User.FindFirstValue(JwtRegisteredClaimNames.Jti);
        var usuarioId = User.FindFirstValue(JwtRegisteredClaimNames.Sub) ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        var expClaim = User.FindFirstValue(JwtRegisteredClaimNames.Exp);

        if (string.IsNullOrWhiteSpace(jti) || string.IsNullOrWhiteSpace(usuarioId) || string.IsNullOrWhiteSpace(expClaim))
        {
            return Unauthorized(new { error = "Token inválido, expirado o ya revocado." });
        }

        if (!long.TryParse(expClaim, out var expiresAtUnix))
        {
            return Unauthorized(new { error = "Token inválido, expirado o ya revocado." });
        }

        var expiresAt = DateTimeOffset.FromUnixTimeSeconds(expiresAtUnix).UtcDateTime;
        await _authService.LogoutAsync(jti, usuarioId, expiresAt, cancellationToken);

        return NoContent();
    }

    [HttpPost("forgot-password")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequestDto request, CancellationToken cancellationToken)
    {
        await _authService.ForgotPasswordAsync(request, cancellationToken);
        return Ok(new { mensaje = "Si el correo existe, recibirás un enlace de recuperación." });
    }
}
