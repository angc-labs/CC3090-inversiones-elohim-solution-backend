using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using ElohimShop.Application.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ElohimShop.API.Controllers;

[ApiController]
[Route("api/admin/usuarios")]
[Authorize]
public class AdminUsuariosController : ControllerBase
{
    private readonly IAdminUsuarioService _adminUsuarioService;
    private readonly IConfiguration _configuration;

    public AdminUsuariosController(
        IAdminUsuarioService adminUsuarioService,
        IConfiguration configuration)
    {
        _adminUsuarioService = adminUsuarioService;
        _configuration = configuration;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<UsuarioAdminDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ObtenerTodos(
        [FromQuery] string? busqueda,
        [FromQuery] string? tipoUsuario,
        [FromQuery] bool? estado,
        CancellationToken cancellationToken)
    {
        if (!EsAdministrador())
        {
            return Forbid();
        }

        var usuarios = await _adminUsuarioService.ObtenerTodosAsync(busqueda, tipoUsuario, estado, cancellationToken);
        return Ok(usuarios);
    }

    [HttpPost]
    [ProducesResponseType(typeof(UsuarioAdminDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Crear(
        [FromBody] CrearUsuarioAdminRequestDto request,
        CancellationToken cancellationToken)
    {
        if (!EsAdministrador())
        {
            return Forbid();
        }

        try
        {
            var usuario = await _adminUsuarioService.CrearAsync(request, cancellationToken);
            return StatusCode(StatusCodes.Status201Created, usuario);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { error = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPut("{id}/rol")]
    [ProducesResponseType(typeof(UsuarioAdminDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CambiarRol(
        string id,
        [FromBody] CambiarRolUsuarioRequestDto request,
        CancellationToken cancellationToken)
    {
        if (!EsSuperAdmin())
        {
            return Forbid();
        }

        try
        {
            var resultado = await _adminUsuarioService.CambiarRolAsync(id, request, cancellationToken);
            return Ok(resultado);
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

    [HttpPut("{id}/estado")]
    [ProducesResponseType(typeof(UsuarioAdminDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CambiarEstado(
        string id,
        [FromBody] CambiarEstadoRequestDto request,
        CancellationToken cancellationToken)
    {
        if (!EsAdministrador())
        {
            return Forbid();
        }

        try
        {
            var resultado = await _adminUsuarioService.CambiarEstadoAsync(id, request.Estado, cancellationToken);
            return Ok(resultado);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    private bool EsAdministrador()
    {
        var tipoUsuario = User.FindFirstValue("tipo_usuario") ?? User.FindFirstValue("tipoUsuario");
        if (string.Equals(tipoUsuario, "administrador", StringComparison.OrdinalIgnoreCase))
        {
            var rol = User.FindFirstValue("rol");
            return rol is null or "administrador" or "admin";
        }

        return false;
    }

    private bool EsSuperAdmin()
    {
        if (string.Equals(
                User.FindFirstValue("es_super_admin"),
                "true",
                StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        var email = User.FindFirstValue(JwtRegisteredClaimNames.Email)
            ?? User.FindFirstValue(ClaimTypes.Email);

        return SuperAdminHelper.IsSuperAdminEmail(email, _configuration["SuperAdmin:Email"]);
    }
}

public class CambiarEstadoRequestDto
{
    public bool Estado { get; init; }
}
