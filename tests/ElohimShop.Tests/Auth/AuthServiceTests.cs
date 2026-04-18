using ElohimShop.Application.Auth;
using ElohimShop.Domain.Entities;
using ElohimShop.Infrastructure.Auth;
using ElohimShop.Infrastructure.Persistence;
using ElohimShop.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;

namespace ElohimShop.Tests.Auth;

public class AuthServiceTests
{
    private readonly ElohimShopDbContext _dbContext;
    private readonly Mock<ITokenRevocationService> _tokenRevocationMock;
    private readonly Mock<IConfiguration> _configMock;
    private readonly IPasswordHashing _passwordHashing;
    private readonly AuthService _service;

    public AuthServiceTests()
    {
        var options = new DbContextOptionsBuilder<ElohimShopDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContext = new ElohimShopDbContext(options);
        _tokenRevocationMock = new Mock<ITokenRevocationService>();
        
        _configMock = new Mock<IConfiguration>();
        _configMock.Setup(c => c["JWT_KEY"]).Returns("test-key-123456789012345678901234567890123456789012345678901234567890");
        _configMock.Setup(c => c["Jwt:Key"]).Returns("test-key-123456789012345678901234567890123456789012345678901234567890");
        _configMock.Setup(c => c["Jwt:Issuer"]).Returns("TestIssuer");
        _configMock.Setup(c => c["Jwt:Audience"]).Returns("TestAudience");
        
        var jwtSection = new Mock<IConfigurationSection>();
        jwtSection.Setup(s => s["Key"]).Returns("test-key-123456789012345678901234567890123456789012345678901234567890");
        jwtSection.Setup(s => s["Issuer"]).Returns("TestIssuer");
        jwtSection.Setup(s => s["Audience"]).Returns("TestAudience");
        _configMock.Setup(c => c.GetSection("Jwt")).Returns(jwtSection.Object);

        _passwordHashing = new PasswordHashingService();
        
        _service = new AuthService(_dbContext, _tokenRevocationMock.Object, _configMock.Object);
    }

    [Fact]
    public async Task RegisterAsync_CorreoExistente_ThrowsException()
    {
        var existingUser = Domain.Entities.Usuario.CrearCliente("test@test.com", "Test", "hashed", "minorista");
        _dbContext.Usuarios.Add(existingUser);
        await _dbContext.SaveChangesAsync();

        var request = new RegisterRequestDto
        {
            Correo = "test@test.com",
            Nombre = "New User",
            Contrasena = "Password123",
            TipoUsuario = "cliente",
            TipoCliente = "minorista"
        };

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.RegisterAsync(request, CancellationToken.None));
    }

    [Fact]
    public async Task RegisterAsync_ClienteSinTipoCliente_ThrowsException()
    {
        var request = new RegisterRequestDto
        {
            Correo = "test@test.com",
            Nombre = "Test",
            Contrasena = "Password123",
            TipoUsuario = "cliente"
        };

        await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.RegisterAsync(request, CancellationToken.None));
    }

    [Fact]
    public async Task RegisterAsync_RegistroClienteValido_RetornaToken()
    {
        var request = new RegisterRequestDto
        {
            Correo = "cliente@test.com",
            Nombre = "Juan",
            Contrasena = "Password123!",
            TipoUsuario = "cliente",
            TipoCliente = "minorista",
            Apellido = "Perez"
        };

        var result = await _service.RegisterAsync(request, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal("cliente@test.com", result.Correo);
        Assert.Equal("Juan", result.Nombre);
        Assert.Equal("cliente", result.TipoUsuario);
        Assert.Equal("minorista", result.TipoCliente);
        Assert.NotNull(result.Token);
    }

    [Fact]
    public async Task RegisterAsync_TipoUsuarioAdministrador_ThrowsException()
    {
        var request = new RegisterRequestDto
        {
            Correo = "admin@test.com",
            Nombre = "Admin",
            Contrasena = "Password123!",
            TipoUsuario = "administrador"
        };

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.RegisterAsync(request, CancellationToken.None));
    }

    [Fact]
    public async Task RegisterAdminAsync_AdminValido_RetornaToken()
    {
        var request = new RegisterRequestDto
        {
            Correo = "admin@test.com",
            Nombre = "Admin",
            Contrasena = "Password123!",
            TipoUsuario = "administrador",
            Rol = "administrador"
        };

        var result = await _service.RegisterAdminAsync(request, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal("admin@test.com", result.Correo);
        Assert.Equal("administrador", result.TipoUsuario);
        Assert.Equal("administrador", result.Rol);
    }

    [Fact]
    public async Task RegisterAdminAsync_SinRol_ThrowsException()
    {
        var request = new RegisterRequestDto
        {
            Correo = "admin@test.com",
            Nombre = "Admin",
            Contrasena = "Password123!",
            TipoUsuario = "administrador"
        };

        await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.RegisterAdminAsync(request, CancellationToken.None));
    }

    [Fact]
    public async Task LoginAsync_UsuarioNoExiste_ThrowsException()
    {
        var request = new LoginRequestDto("noexiste@test.com", "password");

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _service.LoginAsync(request, CancellationToken.None));
    }

    [Fact]
    public async Task LoginAsync_CredencialesValidas_RetornaToken()
    {
        var hashedPassword = _passwordHashing.HashPassword("Password123!");
        var usuario = Domain.Entities.Usuario.CrearCliente("test@test.com", "Test", hashedPassword, "minorista");
        _dbContext.Usuarios.Add(usuario);
        await _dbContext.SaveChangesAsync();

        var request = new LoginRequestDto("test@test.com", "Password123!");

        var result = await _service.LoginAsync(request, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal("test@test.com", result.Correo);
        Assert.Equal("cliente", result.TipoUsuario);
        Assert.NotNull(result.Token);
    }

    [Fact]
    public async Task LoginAsync_PasswordIncorrecto_ThrowsException()
    {
        var hashedPassword = _passwordHashing.HashPassword("Password123!");
        var usuario = Domain.Entities.Usuario.CrearCliente("test@test.com", "Test", hashedPassword, "minorista");
        _dbContext.Usuarios.Add(usuario);
        await _dbContext.SaveChangesAsync();

        var request = new LoginRequestDto("test@test.com", "WrongPassword");

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _service.LoginAsync(request, CancellationToken.None));
    }
}