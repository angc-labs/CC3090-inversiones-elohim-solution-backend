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

    [HttpPost("register")]
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

    [Authorize]
    [HttpPost("logout")]
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