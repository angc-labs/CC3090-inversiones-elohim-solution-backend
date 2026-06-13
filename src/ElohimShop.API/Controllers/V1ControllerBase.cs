using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace ElohimShop.API.Controllers;

public abstract class V1ControllerBase : ControllerBase
{
    protected string? GetTenantId()
    {
        if (Request.Headers.TryGetValue("X-Tenant-ID", out var tenantId) && !string.IsNullOrWhiteSpace(tenantId))
        {
            return tenantId.ToString();
        }

        return User.FindFirstValue("tienda_id");
    }

    protected string? GetUserId()
    {
        return User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
    }

    protected bool EsStaff()
    {
        var tipoUsuario = User.FindFirstValue("tipo_usuario") ?? User.FindFirstValue("tipoUsuario");
        if (string.Equals(tipoUsuario, "administrador", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        var rol = User.FindFirstValue("rol") ?? User.FindFirstValue("rol_staff");
        return rol is "administrador" or "cajero" or "logistica";
    }
}