using ElohimShop.Application.Platform;
using Microsoft.AspNetCore.Mvc;

namespace ElohimShop.API.Controllers;

[ApiController]
[Route("api/v1/sucursales")]
public class SucursalesV1Controller : V1ControllerBase
{
    private readonly IPlatformService _platformService;

    public SucursalesV1Controller(IPlatformService platformService)
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

        var sucursales = await _platformService.ListarSucursalesAsync(cancellationToken);
        return Ok(sucursales);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Obtener(string id, CancellationToken cancellationToken)
    {
        if (GetTenantId() is null)
        {
            return BadRequest(new { error = "Se requiere el header X-Tenant-ID o estar autenticado." });
        }

        var sucursal = await _platformService.ObtenerSucursalAsync(id, cancellationToken);
        return sucursal is null ? NotFound(new { error = "Sucursal no encontrada." }) : Ok(sucursal);
    }

    [HttpPost]
    public async Task<IActionResult> Crear([FromBody] CrearSucursalRequest request, CancellationToken cancellationToken)
    {
        if (GetTenantId() is null)
        {
            return BadRequest(new { error = "Se requiere el header X-Tenant-ID o estar autenticado." });
        }

        if (!EsAdministrador())
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { error = "No tienes permisos para crear sucursales." });
        }

        var sucursal = await _platformService.CrearSucursalAsync(request, cancellationToken);
        return StatusCode(StatusCodes.Status201Created, sucursal);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Actualizar(string id, [FromBody] ActualizarSucursalRequest request, CancellationToken cancellationToken)
    {
        if (GetTenantId() is null)
        {
            return BadRequest(new { error = "Se requiere el header X-Tenant-ID o estar autenticado." });
        }

        if (!EsAdministrador())
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { error = "No tienes permisos para modificar sucursales." });
        }

        var sucursal = await _platformService.ActualizarSucursalAsync(id, request, cancellationToken);
        return sucursal is null ? NotFound(new { error = "Sucursal no encontrada." }) : Ok(sucursal);
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
            return StatusCode(StatusCodes.Status403Forbidden, new { error = "No tienes permisos para eliminar sucursales." });
        }

        var eliminado = await _platformService.EliminarSucursalAsync(id, cancellationToken);
        return eliminado ? NoContent() : NotFound(new { error = "Sucursal no encontrada." });
    }
}
