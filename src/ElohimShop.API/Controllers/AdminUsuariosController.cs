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

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(UsuarioAdminDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObtenerPorId(string id, CancellationToken cancellationToken)
    {
        if (!EsAdministrador()) return Forbid();
        try
        {
            var usuario = await _adminUsuarioService.ObtenerPorIdAsync(id, cancellationToken);
            return Ok(usuario);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    [HttpPost]
    [ProducesResponseType(typeof(UsuarioAdminDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(object), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Crear([FromBody] CrearUsuarioAdminDto dto, CancellationToken cancellationToken)
    {
        if (!EsAdministrador()) return Forbid();
        try
        {
            var usuario = await _adminUsuarioService.CrearAsync(dto, cancellationToken);
            return CreatedAtAction(nameof(ObtenerPorId), new { id = usuario.Id }, usuario);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { error = ex.Message });
        }
    }

    [HttpPut("{id}")]
    [ProducesResponseType(typeof(UsuarioAdminDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(object), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Actualizar(string id, [FromBody] ActualizarUsuarioAdminDto dto, CancellationToken cancellationToken)
    {
        if (!EsAdministrador()) return Forbid();
        try
        {
            var usuario = await _adminUsuarioService.ActualizarAsync(id, dto, cancellationToken);
            return Ok(usuario);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("no encontrado"))
        {
            return NotFound(new { error = ex.Message });
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("en uso"))
        {
            return Conflict(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Eliminar(string id, CancellationToken cancellationToken)
    {
        if (!EsAdministrador()) return Forbid();
        try
        {
            await _adminUsuarioService.EliminarAsync(id, cancellationToken);
            return NoContent();
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("no encontrado"))
        {
            return NotFound(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
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
