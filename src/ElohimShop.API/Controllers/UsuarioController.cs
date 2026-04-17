using System.Security.Claims;
using ElohimShop.Application.Usuario;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ElohimShop.API.Controllers;

[ApiController]
[Route("api/usuario")]
[Authorize]
public class UsuarioController : ControllerBase
{
    private readonly IUsuarioService _usuarioService;

    public UsuarioController(IUsuarioService usuarioService)
    {
        _usuarioService = usuarioService;
    }

    [HttpGet("me")]
    [ProducesResponseType(typeof(UsuarioPerfilDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ObtenerPerfil(CancellationToken cancellationToken)
    {
        var usuarioId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(usuarioId))
        {
            return Unauthorized(new { error = "Token inválido." });
        }

        var perfil = await _usuarioService.ObtenerPerfilAsync(usuarioId, cancellationToken);
        if (perfil is null)
        {
            return Unauthorized(new { error = "Usuario no encontrado." });
        }

        return Ok(perfil);
    }

    [HttpPut("me")]
    [ProducesResponseType(typeof(PerfilActualizadoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> ActualizarPerfil(
        [FromBody] ActualizarPerfilDto dto,
        CancellationToken cancellationToken)
    {
        var usuarioId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(usuarioId))
        {
            return Unauthorized(new { error = "Token inválido." });
        }

        try
        {
            var resultado = await _usuarioService.ActualizarPerfilAsync(usuarioId, dto, cancellationToken);
            return Ok(resultado);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("correo"))
        {
            return Conflict(new { error = ex.Message });
        }
    }
}
