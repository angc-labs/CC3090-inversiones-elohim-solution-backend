using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace ElohimShop.API.Controllers;

public abstract class V1ControllerBase : ControllerBase
{
    protected ElohimShop.Application.Common.TenantContext? GetTenantContext()
    {
        if (HttpContext.Items.TryGetValue("TenantContext", out var contextObj) &&
            contextObj is ElohimShop.Application.Common.TenantContext tenantContext)
        {
            return tenantContext;
        }

        return null;
    }

    protected string? GetTenantId()
    {
        var tenantContext = GetTenantContext();
        if (tenantContext is not null)
        {
            return tenantContext.TenantId;
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
        if (HttpContext.Items.TryGetValue("EffectiveUserId", out var effIdObj) &&
            effIdObj is string effId &&
            !string.IsNullOrWhiteSpace(effId))
        {
            return effId;
        }

        return User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
    }

    protected string? GetEffectiveRole()
    {
        if (HttpContext.Items.TryGetValue("EffectiveUserRole", out var effRoleObj) &&
            effRoleObj is string effRole &&
            !string.IsNullOrWhiteSpace(effRole))
        {
            return effRole;
        }

        return User.FindFirstValue("rol") ?? User.FindFirstValue("rol_staff");
    }

    protected bool EsStaff()
    {
        var tipoUsuario = User.FindFirstValue("tipo_usuario") ?? User.FindFirstValue("tipoUsuario");
        if (string.Equals(tipoUsuario, "administrador", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(tipoUsuario, "admin", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        var rol = GetEffectiveRole();
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

        var rol = GetEffectiveRole();
        return string.Equals(rol, "administrador", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(rol, "admin", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(rol, "superadmin", StringComparison.OrdinalIgnoreCase);
    }
}
