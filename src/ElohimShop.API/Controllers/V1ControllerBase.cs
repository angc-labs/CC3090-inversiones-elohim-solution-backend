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

        if (HttpContext.Items.TryGetValue("ResolvedTenantId", out var resolvedTenantIdObj) &&
            resolvedTenantIdObj is string resolvedTenantId &&
            !string.IsNullOrWhiteSpace(resolvedTenantId))
        {
            return resolvedTenantId;
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
        if (string.Equals(tipoUsuario, "administrador", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(tipoUsuario, "admin", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        var rol = User.FindFirstValue("rol") ?? User.FindFirstValue("rol_staff");
        return rol is "administrador" or "admin" or "cajero" or "logistica" or "superadmin";
    }

    protected bool EsAdministrador()
    {
        var tipoUsuario = User.FindFirstValue("tipo_usuario") ?? User.FindFirstValue("tipoUsuario");
        if (string.Equals(tipoUsuario, "administrador", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(tipoUsuario, "admin", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        var rol = User.FindFirstValue("rol") ?? User.FindFirstValue("rol_staff");
        return string.Equals(rol, "administrador", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(rol, "admin", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(rol, "superadmin", StringComparison.OrdinalIgnoreCase);
    }
}