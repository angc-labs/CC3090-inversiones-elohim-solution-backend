using ElohimShop.Application.Auth;
using ElohimShop.Domain.Platform;
using ElohimShop.Infrastructure.Auth;
using ElohimShop.Infrastructure.Persistence;
using ElohimShop.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;
using PlatformUser = ElohimShop.Domain.Platform.User;

namespace ElohimShop.Tests.Auth;

public class AuthServiceTests
{
    private readonly PlatformDbContext _dbContext;
    private readonly Mock<ITenantProvider> _tenantProviderMock;
    private readonly Mock<IConfiguration> _configMock;
    private readonly AuthService _service;
    private const string TestTenantId = "test-tenant-123";

    public AuthServiceTests()
    {
        _tenantProviderMock = new Mock<ITenantProvider>();
        _tenantProviderMock.Setup(t => t.GetTenantId()).Returns(TestTenantId);

        var options = new DbContextOptionsBuilder<PlatformDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContext = new PlatformDbContext(options, _tenantProviderMock.Object);
        
        _configMock = new Mock<IConfiguration>();
        _configMock.Setup(c => c["SuperAdmin:Email"]).Returns("superadmin@test.com");

        _service = new AuthService(_dbContext, _configMock.Object, _tenantProviderMock.Object);
    }

    [Fact]
    public async Task RegisterAsync_CorreoExistente_ThrowsException()
    {
        var existingUser = new PlatformUser
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Existing User",
            Email = "test@test.com",
            TiendaId = TestTenantId,
            TipoUsuario = "cliente"
        };
        _dbContext.Users.Add(existingUser);
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
        Assert.Equal("Juan Perez", result.Nombre);
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
        Assert.NotNull(result.Token);
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
        var email = "test@test.com";
        var password = "Password123!";
        var hashedPassword = PasswordHashing.Hash(password);

        var usuario = new PlatformUser
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Test Client",
            Email = email,
            TiendaId = TestTenantId,
            TipoUsuario = "cliente"
        };
        _dbContext.Users.Add(usuario);

        var account = new Account
        {
            Id = Guid.NewGuid().ToString(),
            UserId = usuario.Id,
            ProviderId = "credential",
            AccountId = email,
            Password = hashedPassword
        };
        _dbContext.Accounts.Add(account);
        await _dbContext.SaveChangesAsync();

        var request = new LoginRequestDto(email, password);

        var result = await _service.LoginAsync(request, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(email, result.Correo);
        Assert.Equal("cliente", result.TipoUsuario);
        Assert.NotNull(result.Token);
    }

    [Fact]
    public async Task LoginAsync_PasswordIncorrecto_ThrowsException()
    {
        var email = "test@test.com";
        var password = "Password123!";
        var hashedPassword = PasswordHashing.Hash(password);

        var usuario = new PlatformUser
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Test Client",
            Email = email,
            TiendaId = TestTenantId,
            TipoUsuario = "cliente"
        };
        _dbContext.Users.Add(usuario);

        var account = new Account
        {
            Id = Guid.NewGuid().ToString(),
            UserId = usuario.Id,
            ProviderId = "credential",
            AccountId = email,
            Password = hashedPassword
        };
        _dbContext.Accounts.Add(account);
        await _dbContext.SaveChangesAsync();

        var request = new LoginRequestDto(email, "WrongPassword");

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _service.LoginAsync(request, CancellationToken.None));
    }
}