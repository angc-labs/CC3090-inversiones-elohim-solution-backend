using Microsoft.AspNetCore.Http;

namespace ElohimShop.Infrastructure.Persistence;

public class TenantProvider : ITenantProvider
{
    private const string TenantHeaderName = "X-Tenant-ID";
    private readonly IHttpContextAccessor _httpContextAccessor;

    public TenantProvider(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string GetTenantId()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext is null)
        {
            return string.Empty;
        }

        if (httpContext.Request.Headers.TryGetValue(TenantHeaderName, out var tenantId) && !string.IsNullOrWhiteSpace(tenantId))
        {
            return tenantId.ToString();
        }

        var claimTenantId = httpContext.User.FindFirst("tienda_id")?.Value;
        if (!string.IsNullOrWhiteSpace(claimTenantId))
        {
            return claimTenantId;
        }

        return string.Empty;
    }
}