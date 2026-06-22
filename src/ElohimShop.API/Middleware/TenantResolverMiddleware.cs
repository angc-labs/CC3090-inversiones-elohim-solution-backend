using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using ElohimShop.Infrastructure.Persistence;

namespace ElohimShop.API.Middleware;

public class TenantResolverMiddleware
{
    private readonly RequestDelegate _next;

    public TenantResolverMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, PlatformDbContext dbContext)
    {
        string? resolvedTenantId = null;

        // 1. Si ya nos envían el Guid directamente por X-Tenant-ID
        if (context.Request.Headers.TryGetValue("X-Tenant-ID", out var tenantIdHeader) && !string.IsNullOrWhiteSpace(tenantIdHeader))
        {
            resolvedTenantId = tenantIdHeader.ToString();
        }
        // 2. Si nos envían el Slug de la tienda por cabecera
        else if (context.Request.Headers.TryGetValue("X-Tenant-Slug", out var slugHeader) && !string.IsNullOrWhiteSpace(slugHeader))
        {
            var slug = slugHeader.ToString().Trim().ToLowerInvariant();
            
            var tienda = await dbContext.Tiendas
                .IgnoreQueryFilters()
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Slug == slug);
            
            if (tienda != null)
            {
                resolvedTenantId = tienda.Id;
            }
        }
        // 3. Opcional: Resolver a partir del subdominio del host (ej. tiendaadmin3.lvh.me o tiendaadmin3.localhost)
        else
        {
            var host = context.Request.Host.Host;
            var parts = host.Split('.');
            
            // Caso: tiendaadmin3.lvh.me (parts.Length = 3)
            if (parts.Length > 2)
            {
                var subdomain = parts[0];
                if (subdomain != "www" && subdomain != "api" && subdomain != "admin" && subdomain != "localhost")
                {
                    var tienda = await dbContext.Tiendas
                        .IgnoreQueryFilters()
                        .AsNoTracking()
                        .FirstOrDefaultAsync(t => t.Slug == subdomain.ToLowerInvariant());
                    if (tienda != null)
                    {
                        resolvedTenantId = tienda.Id;
                    }
                }
            }
            // Caso: tiendaadmin3.localhost (parts.Length = 2)
            else if (parts.Length == 2 && parts[1] == "localhost")
            {
                var subdomain = parts[0];
                if (subdomain != "www" && subdomain != "api" && subdomain != "admin")
                {
                    var tienda = await dbContext.Tiendas
                        .IgnoreQueryFilters()
                        .AsNoTracking()
                        .FirstOrDefaultAsync(t => t.Slug == subdomain.ToLowerInvariant());
                    if (tienda != null)
                    {
                        resolvedTenantId = tienda.Id;
                    }
                }
            }
        }

        // Si logramos resolver el ID del Tenant, lo guardamos en HttpContext.Items
        if (!string.IsNullOrEmpty(resolvedTenantId))
        {
            context.Items["ResolvedTenantId"] = resolvedTenantId;
        }

        await _next(context);
    }
}
