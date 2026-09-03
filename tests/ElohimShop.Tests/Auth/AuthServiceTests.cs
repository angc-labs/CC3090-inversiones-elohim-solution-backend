using ElohimShop.Application.Auth;
using ElohimShop.Domain.Platform;
using ElohimShop.Infrastructure.Auth;
using ElohimShop.Infrastructure.Persistence;
using ElohimShop.Infrastructure.Security;
using Microsoft.AspNetCore.Http;
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
    private readonly Mock<IGoogleTokenValidator> _googleTokenValidatorMock;
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

        var httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        var httpContext = new DefaultHttpContext();
        httpContext.User = new System.Security.Claims.ClaimsPrincipal(new System.Security.Claims.ClaimsIdentity("TestAuth"));
        httpContextAccessorMock.Setup(h => h.HttpContext).Returns(httpContext);

        _googleTokenValidatorMock = new Mock<IGoogleTokenValidator>();
        _service = new AuthService(
            _dbContext,
            _configMock.Object,
            _tenantProviderMock.Object,
            httpContextAccessorMock.Object,
            _googleTokenValidatorMock.Object);
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
            TipoUsuario = "administrador"
        };

        await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.RegisterAdminAsync(request, CancellationToken.None));
    }

    [Fact]
    public async Task RegisterAdminAsync_AnonymousStoreCreator_AssignsAdministradorRole()
    {
        // Arrange
        var httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        var httpContext = new DefaultHttpContext();
        // User identity is not authenticated (anonymous)
        httpContext.User = new System.Security.Claims.ClaimsPrincipal(new System.Security.Claims.ClaimsIdentity());
        httpContextAccessorMock.Setup(h => h.HttpContext).Returns(httpContext);

        var service = new AuthService(
            _dbContext,
            _configMock.Object,
            _tenantProviderMock.Object,
            httpContextAccessorMock.Object,
            _googleTokenValidatorMock.Object);

        var request = new RegisterRequestDto
        {
            Correo = "newstoreadmin@test.com",
            Nombre = "Store",
            Apellido = "Owner",
            Contrasena = "Password123!",
            TipoUsuario = "administrador",
            Rol = "administrador"
        };

        // Act
        var result = await service.RegisterAdminAsync(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("newstoreadmin@test.com", result.Correo);
        Assert.Equal("administrador", result.TipoUsuario);
        Assert.Equal("administrador", result.Rol);
        Assert.False(result.EsSuperAdmin);

        // Verify the user in db has RolStaff == "administrador"
        var dbUser = await _dbContext.Users.IgnoreQueryFilters().FirstOrDefaultAsync(u => u.Email == "newstoreadmin@test.com");
        Assert.NotNull(dbUser);
        Assert.Equal("administrador", dbUser.RolStaff);
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

    [Fact]
    public async Task LoginWithGoogleAsync_CuentaNueva_CreaUsuarioYAccount()
    {
        SetupGooglePayload("google-subject-new", "nuevo@test.com", "Nuevo Cliente");

        var result = await _service.LoginWithGoogleAsync(
            new GoogleAuthRequestDto { IdToken = "valid-token", TipoUsuario = "cliente", TiendaId = TestTenantId },
            CancellationToken.None);

        Assert.Equal("nuevo@test.com", result.Correo);
        Assert.Equal("cliente", result.TipoUsuario);
        Assert.NotEmpty(result.Token);
        var user = await _dbContext.Users.IgnoreQueryFilters().SingleAsync(u => u.Email == "nuevo@test.com");
        Assert.True(user.EmailVerified);
        Assert.Equal(TestTenantId, user.TiendaId);
        Assert.True(await _dbContext.Accounts.IgnoreQueryFilters().AnyAsync(a =>
            a.UserId == user.Id && a.ProviderId == "google" && a.AccountId == "google-subject-new"));
    }

    [Fact]
    public async Task LoginWithGoogleAsync_CuentaConPassword_VinculaSinDuplicarUsuario()
    {
        var user = new PlatformUser
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Cliente Existente",
            Email = "existente@test.com",
            TiendaId = TestTenantId,
            TipoUsuario = "cliente"
        };
        _dbContext.Users.Add(user);
        _dbContext.Accounts.Add(new Account
        {
            Id = Guid.NewGuid().ToString(),
            UserId = user.Id,
            ProviderId = "credential",
            AccountId = user.Email,
            Password = PasswordHashing.Hash("Password123!")
        });
        await _dbContext.SaveChangesAsync();
        SetupGooglePayload("google-subject-existing", user.Email, user.Name);

        await _service.LoginWithGoogleAsync(
            new GoogleAuthRequestDto { IdToken = "valid-token", TipoUsuario = "cliente" },
            CancellationToken.None);

        Assert.Equal(1, await _dbContext.Users.IgnoreQueryFilters().CountAsync(u => u.Email == user.Email));
        Assert.Equal(2, await _dbContext.Accounts.IgnoreQueryFilters().CountAsync(a => a.UserId == user.Id));
    }

    [Fact]
    public async Task LoginWithGoogleAsync_CuentaVinculada_IniciaSesionSinDuplicarAccount()
    {
        SetupGooglePayload("google-subject-linked", "linked@test.com", "Linked User");
        var request = new GoogleAuthRequestDto { IdToken = "valid-token", TipoUsuario = "cliente" };

        var first = await _service.LoginWithGoogleAsync(request, CancellationToken.None);
        var second = await _service.LoginWithGoogleAsync(request, CancellationToken.None);

        Assert.Equal(first.UsuarioId, second.UsuarioId);
        Assert.Equal(1, await _dbContext.Accounts.IgnoreQueryFilters().CountAsync(a =>
            a.ProviderId == "google" && a.AccountId == "google-subject-linked"));
        Assert.Equal(2, await _dbContext.Sessions.IgnoreQueryFilters().CountAsync(s => s.UserId == first.UsuarioId));
    }

    private void SetupGooglePayload(string subject, string email, string name)
    {
        _googleTokenValidatorMock
            .Setup(v => v.ValidateAsync("valid-token", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GoogleTokenPayload(subject, email, name, true, null));
    }
}
