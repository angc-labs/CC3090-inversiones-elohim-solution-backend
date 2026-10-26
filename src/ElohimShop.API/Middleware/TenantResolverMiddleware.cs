using System.Security.Claims;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ElohimShop.Application.Admin;
using ElohimShop.Application.Common;
using ElohimShop.Infrastructure.Persistence;
using ElohimShop.Infrastructure.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace ElohimShop.API.Middleware;

public class TenantResolverMiddleware
{
    private static readonly Regex TenantIdentifierRegex = new(@"^[a-zA-Z0-9_\-\.]{1,100}$", RegexOptions.Compiled);
    private readonly RequestDelegate _next;

    public TenantResolverMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, PlatformDbContext dbContext, IConfiguration configuration)
    {
        string? candidateTenantId = null;
        string? candidateSlug = null;

        // 1. Extraer y validar cabeceras de tenant
        if (context.Request.Headers.TryGetValue("X-Tenant-ID", out var tenantIdHeader))
        {
            var rawId = tenantIdHeader.ToString().Trim();
            if (string.IsNullOrWhiteSpace(rawId) || rawId.Length > 100 || !TenantIdentifierRegex.IsMatch(rawId))
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync("{\"error\": \"El header X-Tenant-ID contiene un formato o valor inválido.\"}");
                return;
            }
            candidateTenantId = rawId;
        }

        if (context.Request.Headers.TryGetValue("X-Tenant-Slug", out var slugHeader))
        {
            var rawSlug = slugHeader.ToString().Trim().ToLowerInvariant();
            if (string.IsNullOrWhiteSpace(rawSlug) || rawSlug.Length > 100 || !TenantIdentifierRegex.IsMatch(rawSlug))
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync("{\"error\": \"El header X-Tenant-Slug contiene un formato o valor inválido.\"}");
                return;
            }
            candidateSlug = rawSlug;
        }

        // Si no se proporcionaron cabeceras, extraer por subdominio
        if (candidateTenantId == null && candidateSlug == null)
        {
            var host = context.Request.Host.Host;
            var parts = host.Split('.');
            if (parts.Length > 2)
            {
                var subdomain = parts[0].ToLowerInvariant();
                if (subdomain is not ("www" or "api" or "admin" or "localhost") && TenantIdentifierRegex.IsMatch(subdomain))
                {
                    candidateSlug = subdomain;
                }
            }
            else if (parts.Length == 2 && parts[1] == "localhost")
            {
                var subdomain = parts[0].ToLowerInvariant();
                if (subdomain is not ("www" or "api" or "admin") && TenantIdentifierRegex.IsMatch(subdomain))
                {
                    candidateSlug = subdomain;
                }
            }
        }

        // 2. Resolver la entidad Tienda en PlatformDbContext
        ElohimShop.Domain.Platform.Tienda? tienda = null;

        if (!string.IsNullOrEmpty(candidateTenantId) && !string.IsNullOrEmpty(candidateSlug))
        {
            tienda = await dbContext.Tiendas
                .IgnoreQueryFilters()
                .AsNoTracking()
                .FirstOrDefaultAsync(t => (t.Id == candidateTenantId || t.Slug == candidateTenantId) &&
                                          (t.Slug == candidateSlug || t.Id == candidateSlug));

            if (tienda == null)
            {
                var idExists = await dbContext.Tiendas.IgnoreQueryFilters().AnyAsync(t => t.Id == candidateTenantId || t.Slug == candidateTenantId);
                var slugExists = await dbContext.Tiendas.IgnoreQueryFilters().AnyAsync(t => t.Slug == candidateSlug || t.Id == candidateSlug);
                if (idExists && slugExists)
                {
                    context.Response.StatusCode = StatusCodes.Status400BadRequest;
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsync("{\"error\": \"Los headers X-Tenant-ID y X-Tenant-Slug no corresponden a la misma tienda.\"}");
                    return;
                }
            }
        }
        else if (!string.IsNullOrEmpty(candidateTenantId))
        {
            tienda = await dbContext.Tiendas
                .IgnoreQueryFilters()
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == candidateTenantId || t.Slug == candidateTenantId);
        }
        else if (!string.IsNullOrEmpty(candidateSlug))
        {
            tienda = await dbContext.Tiendas
                .IgnoreQueryFilters()
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Slug == candidateSlug || t.Id == candidateSlug);
        }

        // Si se especificó una tienda en el request pero no se encuentra en BD o está inactiva
        if ((!string.IsNullOrEmpty(candidateTenantId) || !string.IsNullOrEmpty(candidateSlug)) && tienda is null)
        {
            context.Response.StatusCode = StatusCodes.Status404NotFound;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync("{\"error\": \"La tienda solicitada no existe o no está disponible.\"}");
            return;
        }

        if (tienda is not null && string.Equals(tienda.Estado, "inactivo", StringComparison.OrdinalIgnoreCase))
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync("{\"error\": \"La tienda solicitada se encuentra inactiva.\"}");
            return;
        }

        // 3. Cross-Tenant Guard (Verificación estricta de pertenencia al tenant para usuarios autenticados)
        if (tienda is not null && context.User.Identity?.IsAuthenticated == true)
        {
            var userTenantId = context.User.FindFirstValue("tienda_id");
            var email = context.User.FindFirstValue(ClaimTypes.Email) ?? context.User.FindFirstValue("email");
            var rol = context.User.FindFirstValue("rol") ?? context.User.FindFirstValue("rol_staff");
            var esSuperAdminClaim = context.User.FindFirstValue("es_super_admin");

            var isSuperAdmin = string.Equals(rol, "superadmin", StringComparison.OrdinalIgnoreCase) ||
                               string.Equals(esSuperAdminClaim, "true", StringComparison.OrdinalIgnoreCase) ||
                               SuperAdminHelper.IsSuperAdminEmail(email, configuration["SuperAdmin:Email"]);

            if (!isSuperAdmin)
            {
                bool belongsToTenant = false;
                string? effectiveUserId = null;
                string? effectiveRole = null;

                // 1) Si el token actual fue emitido para este tenant
                if (!string.IsNullOrEmpty(userTenantId) && string.Equals(userTenantId, tienda.Id, StringComparison.OrdinalIgnoreCase))
                {
                    belongsToTenant = true;
                }
                // 2) Soporte multi-tienda: el usuario puede pertenecer a múltiples tiendas con su misma cuenta/correo
                else if (!string.IsNullOrWhiteSpace(email))
                {
                    var storeUser = await dbContext.Users
                        .IgnoreQueryFilters()
                        .AsNoTracking()
                        .FirstOrDefaultAsync(u => u.Email == email.Trim().ToLowerInvariant()
                                               && u.TiendaId == tienda.Id
                                               && u.Estado);

                    if (storeUser != null)
                    {
                        belongsToTenant = true;
                        effectiveUserId = storeUser.Id;
                        effectiveRole = storeUser.RolStaff ?? storeUser.TipoUsuario;
                    }
                }

                if (!belongsToTenant)
                {
                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsync("{\"error\": \"Acceso denegado: el usuario no pertenece a la tienda solicitada.\"}");
                    return;
                }

                if (!string.IsNullOrEmpty(effectiveUserId))
                {
                    context.Items["EffectiveUserId"] = effectiveUserId;
                }
                if (!string.IsNullOrEmpty(effectiveRole))
                {
                    context.Items["EffectiveUserRole"] = effectiveRole;
                }
            }

            var tenantContext = new TenantContext
            {
                TenantId = tienda.Id,
                Slug = tienda.Slug,
                Nombre = tienda.Nombre,
                IsSuperAdminOverride = isSuperAdmin
            };
            context.Items["TenantContext"] = tenantContext;
            context.Items["ResolvedTenantId"] = tienda.Id;
        }
        else if (tienda is not null)
        {
            var tenantContext = new TenantContext
            {
                TenantId = tienda.Id,
                Slug = tienda.Slug,
                Nombre = tienda.Nombre,
                IsSuperAdminOverride = false
            };
            context.Items["TenantContext"] = tenantContext;
            context.Items["ResolvedTenantId"] = tienda.Id;
        }

        await _next(context);
    }
}
