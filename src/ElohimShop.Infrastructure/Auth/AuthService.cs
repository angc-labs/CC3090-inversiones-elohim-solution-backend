using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ElohimShop.Application.Auth;
using ElohimShop.Domain.Entities;
using ElohimShop.Infrastructure.Persistence;
using ElohimShop.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace ElohimShop.Infrastructure.Auth;

public class AuthService : IAuthService
{
    private const int TokenLifetimeMonths = 1;
    private readonly ElohimShopDbContext _dbContext;
    private readonly ITokenRevocationService _tokenRevocationService;
    private readonly IConfiguration _configuration;

    public AuthService(
        ElohimShopDbContext dbContext,
        ITokenRevocationService tokenRevocationService,
        IConfiguration configuration)
    {
        _dbContext = dbContext;
        _tokenRevocationService = tokenRevocationService;
        _configuration = configuration;
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request, CancellationToken cancellationToken)
    {
        if (request.TipoUsuario == "administrador")
        {
            throw new InvalidOperationException("No tenés permisos para registrar administradores.");
        }

        if (request.TipoUsuario == "cliente" && string.IsNullOrWhiteSpace(request.TipoCliente))
        {
            throw new ArgumentException("El campo tipoCliente es requerido para usuarios de tipo cliente.");
        }

        var correo = request.Correo.Trim();
        var correoExistente = await _dbContext.Usuarios
            .AnyAsync(u => u.Correo == correo, cancellationToken);

        if (correoExistente)
        {
            throw new InvalidOperationException("El correo ya está registrado.");
        }

        var hashedPassword = PasswordHashing.Hash(request.Contrasena);

        var usuario = Usuario.CrearCliente(
            correo,
            request.Nombre,
            hashedPassword,
            request.TipoCliente!,
            request.Apellido,
            request.Telefono,
            request.Direccion);

        _dbContext.Usuarios.Add(usuario);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var token = GenerateJwt(usuario);

        return new AuthResponseDto(
            usuario.Id,
            usuario.Correo,
            usuario.Nombre,
            usuario.TipoUsuario,
            null,
            usuario.ClientePerfil?.TipoCliente,
            token.Token,
            token.ExpiresAt);
    }

    public async Task<AuthResponseDto> RegisterAdminAsync(RegisterRequestDto request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Rol))
        {
            throw new ArgumentException("El campo rol es requerido para administradores.");
        }

        var correo = request.Correo.Trim();
        var correoExistente = await _dbContext.Usuarios
            .AnyAsync(u => u.Correo == correo, cancellationToken);

        if (correoExistente)
        {
            throw new InvalidOperationException("El correo ya está registrado.");
        }

        var hashedPassword = PasswordHashing.Hash(request.Contrasena);

        var usuario = Usuario.CrearAdministrador(
            correo,
            request.Nombre,
            hashedPassword,
            request.Rol,
            request.Apellido,
            request.Telefono);

        _dbContext.Usuarios.Add(usuario);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var token = GenerateJwt(usuario);

        return new AuthResponseDto(
            usuario.Id,
            usuario.Correo,
            usuario.Nombre,
            usuario.TipoUsuario,
            usuario.AdministradorPerfil?.Rol,
            null,
            token.Token,
            token.ExpiresAt);
    }

    public async Task<AuthResponseDto> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken)
    {
        var correo = request.Correo.Trim();

        var usuario = await _dbContext.Usuarios
            .Include(u => u.ClientePerfil)
            .Include(u => u.AdministradorPerfil)
            .FirstOrDefaultAsync(u => u.Correo == correo, cancellationToken);

        if (usuario is null || !usuario.Estado || !PasswordHashing.Verify(request.Contrasena, usuario.Contrasena))
        {
            throw new UnauthorizedAccessException("Credenciales inválidas o cuenta inactiva.");
        }

        var token = GenerateJwt(usuario);

        return new AuthResponseDto(
            usuario.Id,
            usuario.Correo,
            usuario.Nombre,
            usuario.TipoUsuario,
            usuario.AdministradorPerfil?.Rol,
            usuario.ClientePerfil?.TipoCliente,
            token.Token,
            token.ExpiresAt);
    }

    public Task LogoutAsync(string jti, string usuarioId, DateTime expiresAt, CancellationToken cancellationToken)
    {
        return _tokenRevocationService.RevokeTokenAsync(jti, usuarioId, expiresAt, cancellationToken);
    }

    public Task ForgotPasswordAsync(ForgotPasswordRequestDto request, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    private (string Token, DateTime ExpiresAt) GenerateJwt(Usuario usuario)
    {
        var jwtSection = _configuration.GetSection("Jwt");
        var key = _configuration["JWT_KEY"]
            ?? jwtSection["Key"]
            ?? throw new InvalidOperationException("JWT key is required. Configure JWT_KEY or Jwt:Key.");
        var issuer = jwtSection["Issuer"] ?? throw new InvalidOperationException("Jwt:Issuer is required.");
        var audience = jwtSection["Audience"] ?? throw new InvalidOperationException("Jwt:Audience is required.");

        var expiresAt = DateTime.UtcNow.AddMonths(TokenLifetimeMonths);
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
        var jti = Guid.NewGuid().ToString();

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, usuario.Id),
            new(JwtRegisteredClaimNames.Jti, jti),
            new(JwtRegisteredClaimNames.Email, usuario.Correo),
            new(ClaimTypes.Name, usuario.Nombre),
            new(ClaimTypes.NameIdentifier, usuario.Id),
            new("tipo_usuario", usuario.TipoUsuario)
        };

        if (usuario.ClientePerfil != null)
        {
            claims.Add(new Claim("tipo_cliente", usuario.ClientePerfil.TipoCliente));
            claims.Add(new Claim(ClaimTypes.Role, "Cliente"));
        }

        if (usuario.AdministradorPerfil != null)
        {
            claims.Add(new Claim("rol", usuario.AdministradorPerfil.Rol));
            claims.Add(new Claim(ClaimTypes.Role, usuario.AdministradorPerfil.Rol));
        }

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = expiresAt,
            Issuer = issuer,
            Audience = audience,
            SigningCredentials = credentials
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);

        return (tokenHandler.WriteToken(token), expiresAt);
    }
}
