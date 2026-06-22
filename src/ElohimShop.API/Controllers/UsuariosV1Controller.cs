using ElohimShop.Application.Platform;
using Microsoft.AspNetCore.Mvc;

namespace ElohimShop.API.Controllers;

[ApiController]
[Route("api/v1/usuarios")]
public class UsuariosV1Controller : V1ControllerBase
{
    private readonly IPlatformService _platformService;

    public UsuariosV1Controller(IPlatformService platformService)
    {
        _platformService = platformService;
    }

    [HttpGet]
    public async Task<IActionResult> Listar(CancellationToken cancellationToken)
    {
        if (GetTenantId() is null)
        {
            return BadRequest(new { error = "Se requiere el header X-Tenant-ID o estar autenticado." });
        }

        if (!EsAdministrador())
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { error = "No tienes permisos para ver los usuarios." });
        }

        var usuarios = await _platformService.ListarUsuariosAsync(cancellationToken);
        return Ok(usuarios);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Obtener(string id, CancellationToken cancellationToken)
    {
        if (GetTenantId() is null)
        {
            return BadRequest(new { error = "Se requiere el header X-Tenant-ID o estar autenticado." });
        }

        if (!EsAdministrador())
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { error = "No tienes permisos para ver este usuario." });
        }

        var usuario = await _platformService.ObtenerUsuarioAsync(id, cancellationToken);
        return usuario is null ? NotFound(new { error = "Usuario no encontrado." }) : Ok(usuario);
    }

    [HttpPost("invitar")]
    public async Task<IActionResult> Invitar([FromBody] InvitarPlatformUsuarioRequest request, CancellationToken cancellationToken)
    {
        if (GetTenantId() is null)
        {
            return BadRequest(new { error = "Se requiere el header X-Tenant-ID o estar autenticado." });
        }

        if (!EsAdministrador())
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { error = "No tienes permisos para invitar colaboradores." });
        }

        try
        {
            var usuario = await _platformService.InvitarUsuarioAsync(request, cancellationToken);
            return StatusCode(StatusCodes.Status201Created, usuario);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { error = ex.Message });
        }
    }

    [HttpPut("{id}/rol")]
    public async Task<IActionResult> CambiarRol(string id, [FromBody] CambiarRolPlatformUsuarioRequest request, CancellationToken cancellationToken)
    {
        if (GetTenantId() is null)
        {
            return BadRequest(new { error = "Se requiere el header X-Tenant-ID o estar autenticado." });
        }

        if (!EsAdministrador())
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { error = "No tienes permisos para cambiar roles." });
        }

        var usuario = await _platformService.CambiarRolUsuarioAsync(id, request, cancellationToken);
        return usuario is null ? NotFound(new { error = "Usuario no encontrado." }) : Ok(usuario);
    }

    [HttpPut("{id}/estado")]
    public async Task<IActionResult> CambiarEstado(string id, [FromBody] bool activo, CancellationToken cancellationToken)
    {
        if (GetTenantId() is null)
        {
            return BadRequest(new { error = "Se requiere el header X-Tenant-ID o estar autenticado." });
        }

        if (!EsAdministrador())
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { error = "No tienes permisos para cambiar el estado del usuario." });
        }

        var usuario = await _platformService.CambiarEstadoUsuarioAsync(id, activo, cancellationToken);
        return usuario is null ? NotFound(new { error = "Usuario no encontrado." }) : Ok(usuario);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Eliminar(string id, CancellationToken cancellationToken)
    {
        if (GetTenantId() is null)
        {
            return BadRequest(new { error = "Se requiere el header X-Tenant-ID o estar autenticado." });
        }

        if (!EsAdministrador())
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { error = "No tienes permisos para eliminar colaboradores." });
        }

        var eliminado = await _platformService.EliminarUsuarioAsync(id, cancellationToken);
        return eliminado ? NoContent() : NotFound(new { error = "Usuario no encontrado." });
    }
}
