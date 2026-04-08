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

public class ClientAuthService : IClientAuthService
{
    private const int TokenLifetimeMonths = 1;
    private readonly ElohimShopDbContext _dbContext;
    private readonly ITokenRevocationService _tokenRevocationService;
    private readonly IConfiguration _configuration;

    public ClientAuthService(
        ElohimShopDbContext dbContext,
        ITokenRevocationService tokenRevocationService,
        IConfiguration configuration)
    {
        _dbContext = dbContext;
        _tokenRevocationService = tokenRevocationService;
        _configuration = configuration;
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterClientRequestDto request, CancellationToken cancellationToken)
    {
        var correo = request.Correo.Trim();

        var correoExistente = await _dbContext.Clientes
            .AnyAsync(cliente => cliente.Correo == correo, cancellationToken);

        if (correoExistente)
        {
            throw new InvalidOperationException("Ya existe un cliente con ese correo.");
        }

        var hashedPassword = PasswordHashing.Hash(request.Contrasena);

        var cliente = Cliente.Registrar(
            correo,
            request.Nombre,
            hashedPassword,
            request.Apellido,
            request.Telefono,
            request.Direccion
            );

        _dbContext.Clientes.Add(cliente);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var token = GenerateJwt(cliente);

        return new AuthResponseDto(
            cliente.IdCliente,
            cliente.Correo,
            cliente.Nombre,
            token.Token,
            token.ExpiresAt);
    }

    public async Task<AuthResponseDto> LoginAsync(LoginClientRequestDto request, CancellationToken cancellationToken)
    {
        var correo = request.Correo.Trim();

        var cliente = await _dbContext.Clientes
            .FirstOrDefaultAsync(cliente => cliente.Correo == correo, cancellationToken);

        if (cliente is null || !cliente.EstadoCuenta || !PasswordHashing.Verify(request.Contrasena, cliente.Contrasena))
        {
            throw new UnauthorizedAccessException("Credenciales inválidas.");
        }

        var token = GenerateJwt(cliente);

        return new AuthResponseDto(
            cliente.IdCliente,
            cliente.Correo,
            cliente.Nombre,
            token.Token,
            token.ExpiresAt);
    }

    public Task LogoutAsync(string jti, string clienteId, DateTime expiresAt, CancellationToken cancellationToken)
    {
        return _tokenRevocationService.RevokeTokenAsync(jti, clienteId, expiresAt, cancellationToken);
    }

    private (string Token, DateTime ExpiresAt) GenerateJwt(Cliente cliente)
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
            new(JwtRegisteredClaimNames.Sub, cliente.IdCliente),
            new(JwtRegisteredClaimNames.Jti, jti),
            new(JwtRegisteredClaimNames.Email, cliente.Correo),
            new(ClaimTypes.Name, cliente.Nombre),
            new(ClaimTypes.NameIdentifier, cliente.IdCliente),
            new(ClaimTypes.Role, "Cliente")
        };

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