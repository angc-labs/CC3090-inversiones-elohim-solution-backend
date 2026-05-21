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

    public AdminUsuariosController(IAdminUsuarioService adminUsuarioService)
    {
        _adminUsuarioService = adminUsuarioService;
    }

    /// <summary>
    /// Lista todos los usuarios. Soporta filtros opcionales por búsqueda, tipo y estado.
    /// </summary>
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

    /// <summary>
    /// Cambia el estado (activo/inactivo) de un usuario.
    /// </summary>
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
        var rol = User.FindFirstValue("rol");
        var tipoUsuario = User.FindFirstValue("tipoUsuario");
        return tipoUsuario == "administrador" || rol == "admin" || rol == "administrador";
    }
}

public class CambiarEstadoRequestDto
{
    public bool Estado { get; init; }
}
