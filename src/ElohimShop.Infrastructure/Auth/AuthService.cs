using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using ElohimShop.Application.Admin;
using ElohimShop.Application.Auth;
using ElohimShop.Domain.Platform;
using ElohimShop.Infrastructure.Persistence;
using ElohimShop.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using PlatformUser = ElohimShop.Domain.Platform.User;

namespace ElohimShop.Infrastructure.Auth;

public class AuthService : IAuthService
{
    private const int TokenLifetimeMonths = 1;
    private readonly PlatformDbContext _dbContext;
    private readonly IConfiguration _configuration;
    private readonly ITenantProvider _tenantProvider;

    public AuthService(
        PlatformDbContext dbContext,
        IConfiguration configuration,
        ITenantProvider tenantProvider)
    {
        _dbContext = dbContext;
        _configuration = configuration;
        _tenantProvider = tenantProvider;
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request, CancellationToken cancellationToken)
    {
        if (request.TipoUsuario == "administrador" || request.TipoUsuario == "staff")
        {
            throw new InvalidOperationException("No tenés permisos para registrar personal de la tienda.");
        }

        var tenantId = _tenantProvider.GetTenantId();
        if (string.IsNullOrWhiteSpace(tenantId))
        {
            throw new InvalidOperationException("Se requiere el tenant (tienda_id) para registrar un cliente.");
        }

        var email = request.Correo.Trim().ToLowerInvariant();
        var correoExistente = await _dbContext.Users
            .IgnoreQueryFilters()
            .AnyAsync(u => u.Email == email && u.TiendaId == tenantId, cancellationToken);

        if (correoExistente)
        {
            throw new InvalidOperationException("El correo ya está registrado en esta tienda.");
        }

        var hashedPassword = PasswordHashing.Hash(request.Contrasena);

        var usuario = new PlatformUser
        {
            Id = Guid.NewGuid().ToString(),
            Name = $"{request.Nombre} {request.Apellido}".Trim(),
            Email = email,
            EmailVerified = false,
            TiendaId = tenantId,
            TipoUsuario = "cliente",
            Telefono = request.Telefono,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _dbContext.Users.Add(usuario);

        var account = new Account
        {
            Id = Guid.NewGuid().ToString(),
            UserId = usuario.Id,
            ProviderId = "credential",
            AccountId = email,
            Password = hashedPassword,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _dbContext.Accounts.Add(account);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var session = await CreateSessionAsync(usuario.Id, cancellationToken);

        var esSuperAdmin = SuperAdminHelper.IsSuperAdminEmail(
            usuario.Email,
            _configuration["SuperAdmin:Email"]);

        return new AuthResponseDto(
            usuario.Id,
            usuario.Email,
            usuario.Name,
            "cliente",
            null,
            request.TipoCliente ?? "particular",
            session.Token,
            session.ExpiresAt,
            esSuperAdmin);
    }

    public async Task<AuthResponseDto> RegisterAdminAsync(RegisterRequestDto request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Rol))
        {
            throw new ArgumentException("El campo rol es requerido para personal de la tienda.");
        }

        var tenantId = _tenantProvider.GetTenantId();
        if (string.IsNullOrWhiteSpace(tenantId))
        {
            throw new InvalidOperationException("Se requiere el tenant (tienda_id) para registrar personal.");
        }

        var email = request.Correo.Trim().ToLowerInvariant();
        var correoExistente = await _dbContext.Users
            .IgnoreQueryFilters()
            .AnyAsync(u => u.Email == email && u.TiendaId == tenantId, cancellationToken);

        if (correoExistente)
        {
            throw new InvalidOperationException("El correo ya está registrado en esta tienda.");
        }

        var hashedPassword = PasswordHashing.Hash(request.Contrasena);

        var usuario = new PlatformUser
        {
            Id = Guid.NewGuid().ToString(),
            Name = $"{request.Nombre} {request.Apellido}".Trim(),
            Email = email,
            EmailVerified = false,
            TiendaId = tenantId,
            TipoUsuario = "staff",
            RolStaff = request.Rol.Trim().ToLowerInvariant(),
            Telefono = request.Telefono,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _dbContext.Users.Add(usuario);

        var account = new Account
        {
            Id = Guid.NewGuid().ToString(),
            UserId = usuario.Id,
            ProviderId = "credential",
            AccountId = email,
            Password = hashedPassword,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _dbContext.Accounts.Add(account);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var session = await CreateSessionAsync(usuario.Id, cancellationToken);

        var esSuperAdmin = SuperAdminHelper.IsSuperAdminEmail(
            usuario.Email,
            _configuration["SuperAdmin:Email"]);

        return new AuthResponseDto(
            usuario.Id,
            usuario.Email,
            usuario.Name,
            "administrador",
            usuario.RolStaff,
            null,
            session.Token,
            session.ExpiresAt,
            esSuperAdmin);
    }

    public async Task<AuthResponseDto> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken)
    {
        var email = request.Correo.Trim().ToLowerInvariant();

        // Query across all stores/tenants if tenant is not specified, or scope to current tenant
        var query = _dbContext.Users.IgnoreQueryFilters();
        
        var tenantId = _tenantProvider.GetTenantId();
        if (!string.IsNullOrWhiteSpace(tenantId))
        {
            query = query.Where(u => u.TiendaId == tenantId);
        }

        var usuario = await query.FirstOrDefaultAsync(u => u.Email == email, cancellationToken);

        if (usuario is null)
        {
            throw new UnauthorizedAccessException("Credenciales inválidas.");
        }

        var account = await _dbContext.Accounts
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(a => a.UserId == usuario.Id && a.ProviderId == "credential", cancellationToken);

        if (account is null || string.IsNullOrWhiteSpace(account.Password) || !PasswordHashing.Verify(request.Contrasena, account.Password))
        {
            throw new UnauthorizedAccessException("Credenciales inválidas.");
        }

        var session = await CreateSessionAsync(usuario.Id, cancellationToken);

        var esSuperAdmin = SuperAdminHelper.IsSuperAdminEmail(
            usuario.Email,
            _configuration["SuperAdmin:Email"]);

        var mappedTipoUsuario = usuario.TipoUsuario == "staff" ? "administrador" : "cliente";

        return new AuthResponseDto(
            usuario.Id,
            usuario.Email,
            usuario.Name,
            mappedTipoUsuario,
            usuario.RolStaff,
            usuario.TipoUsuario == "cliente" ? "particular" : null,
            session.Token,
            session.ExpiresAt,
            esSuperAdmin);
    }

    public async Task LogoutAsync(string token, string usuarioId, DateTime expiresAt, CancellationToken cancellationToken)
    {
        var session = await _dbContext.Sessions
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(s => s.Token == token, cancellationToken);

        if (session != null)
        {
            _dbContext.Sessions.Remove(session);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }

    public Task ForgotPasswordAsync(ForgotPasswordRequestDto request, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    private async Task<Session> CreateSessionAsync(string userId, CancellationToken cancellationToken)
    {
        var sessionToken = Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString("N");
        var session = new Session
        {
            Id = Guid.NewGuid().ToString(),
            Token = sessionToken,
            UserId = userId,
            ExpiresAt = DateTime.UtcNow.AddMonths(TokenLifetimeMonths),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _dbContext.Sessions.Add(session);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return session;
    }
}
