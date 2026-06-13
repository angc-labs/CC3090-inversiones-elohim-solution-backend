using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using ElohimShop.Infrastructure.Persistence;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace ElohimShop.Infrastructure.Auth;

public class BetterAuthSessionMiddleware
{
    private readonly RequestDelegate _next;

    public BetterAuthSessionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, PlatformDbContext dbContext)
    {
        string? token = null;

        // 1. Try to get token from Authorization header (Bearer token)
        var authHeader = context.Request.Headers["Authorization"].ToString();
        if (!string.IsNullOrWhiteSpace(authHeader) && authHeader.StartsWith("Bearer ", System.StringComparison.OrdinalIgnoreCase))
        {
            token = authHeader.Substring("Bearer ".Length).Trim();
        }

        // 2. Try to get token from Cookies if not found in headers
        if (string.IsNullOrWhiteSpace(token))
        {
            if (context.Request.Cookies.TryGetValue("better-auth.session_token", out var cookieToken))
            {
                token = cookieToken;
            }
            else if (context.Request.Cookies.TryGetValue("__secure-better-auth.session_token", out var secureCookieToken))
            {
                token = secureCookieToken;
            }
        }

        if (!string.IsNullOrWhiteSpace(token))
        {
            // Query DB ignoring filters to find the session and its user
            var session = await dbContext.Sessions
                .IgnoreQueryFilters()
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.Token == token);

            if (session != null && session.ExpiresAt > DateTime.UtcNow && session.User != null)
            {
                var user = session.User;
                var claims = new List<Claim>
                {
                    new(ClaimTypes.NameIdentifier, user.Id),
                    new(JwtRegisteredClaimNames.Sub, user.Id),
                    new(ClaimTypes.Email, user.Email),
                    new(ClaimTypes.Name, user.Name),
                    new("tienda_id", user.TiendaId),
                    new("tipo_usuario", user.TipoUsuario),
                    new("tipoUsuario", user.TipoUsuario)
                };

                if (user.TipoUsuario == "staff")
                {
                    var rol = user.RolStaff ?? "cajero";
                    claims.Add(new Claim("rol", rol));
                    claims.Add(new Claim("rol_staff", rol));
                    claims.Add(new Claim(ClaimTypes.Role, rol));
                }
                else if (user.TipoUsuario == "cliente")
                {
                    claims.Add(new Claim("rol", "cliente"));
                    claims.Add(new Claim(ClaimTypes.Role, "cliente"));
                }

                var identity = new ClaimsIdentity(claims, "BetterAuthSession");
                context.User = new ClaimsPrincipal(identity);
            }
        }

        await _next(context);
    }
}
