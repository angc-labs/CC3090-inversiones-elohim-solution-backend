using ElohimShop.Application.Usuario;
using ElohimShop.Infrastructure.Persistence;
using ElohimShop.Infrastructure.Security;
using ElohimShop.Infrastructure.User;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;
using PlatformUser = ElohimShop.Domain.Platform.User;

namespace ElohimShop.Tests.Usuario;

public class UsuarioServiceTests
{
    private readonly PlatformDbContext _dbContext;
    private readonly IPasswordHashing _passwordHashing;
    private readonly UsuarioService _service;
    private readonly Mock<ITenantProvider> _tenantProviderMock;

    public UsuarioServiceTests()
    {
        _tenantProviderMock = new Mock<ITenantProvider>();
        _tenantProviderMock.Setup(x => x.GetTenantId()).Returns("tienda-test");

        var options = new DbContextOptionsBuilder<PlatformDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContext = new PlatformDbContext(options, _tenantProviderMock.Object);
        _passwordHashing = new PasswordHashingService();
        
        _service = new UsuarioService(_dbContext, _passwordHashing);
    }

    [Fact]
    public async Task ObtenerPerfilAsync_UsuarioNoExiste_ReturnsNull()
    {
        var result = await _service.ObtenerPerfilAsync("usuario-inexistente", CancellationToken.None);

        Assert.Null(result);
    }

    [Fact]
    public async Task ObtenerPerfilAsync_Cliente_ReturnsPerfil()
    {
        var usuario = new PlatformUser
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Juan Perez",
            Email = "test@test.com",
            TipoUsuario = "cliente",
            Telefono = "123456",
            TiendaId = "tienda-test"
        };
        _dbContext.Users.Add(usuario);
        await _dbContext.SaveChangesAsync();

        var result = await _service.ObtenerPerfilAsync(usuario.Id, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal("test@test.com", result.Correo);
        Assert.Equal("Juan", result.Nombre);
        Assert.Equal("Perez", result.Apellido);
        Assert.Equal("cliente", result.TipoUsuario);
    }

    [Fact]
    public async Task ObtenerPerfilAsync_Administrador_ReturnsPerfil()
    {
        var usuario = new PlatformUser
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Admin",
            Email = "admin@test.com",
            TipoUsuario = "staff",
            RolStaff = "cajero",
            TiendaId = "tienda-test"
        };
        _dbContext.Users.Add(usuario);
        await _dbContext.SaveChangesAsync();

        var result = await _service.ObtenerPerfilAsync(usuario.Id, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal("admin@test.com", result.Correo);
        Assert.Equal("administrador", result.TipoUsuario);
        Assert.Equal("cajero", result.Rol);
    }

    [Fact]
    public async Task ActualizarPerfilAsync_UsuarioNoExiste_ThrowsException()
    {
        var dto = new ActualizarPerfilDto { Nombre = "Nuevo" };

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.ActualizarPerfilAsync("usuario-inexistente", dto, CancellationToken.None));
    }

    [Fact]
    public async Task ActualizarPerfilAsync_CorreoEnUso_ThrowsException()
    {
        var usuario1 = new PlatformUser
        {
            Id = Guid.NewGuid().ToString(),
            Name = "User 1",
            Email = "test1@test.com",
            TipoUsuario = "cliente",
            TiendaId = "tienda-test"
        };
        var usuario2 = new PlatformUser
        {
            Id = Guid.NewGuid().ToString(),
            Name = "User 2",
            Email = "test2@test.com",
            TipoUsuario = "cliente",
            TiendaId = "tienda-test"
        };
        _dbContext.Users.AddRange(usuario1, usuario2);
        await _dbContext.SaveChangesAsync();

        var dto = new ActualizarPerfilDto { Correo = "test2@test.com" };

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.ActualizarPerfilAsync(usuario1.Id, dto, CancellationToken.None));
    }

    [Fact]
    public async Task ActualizarPerfilAsync_DatosValidos_ActualizaPerfil()
    {
        var usuario = new PlatformUser
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Juan",
            Email = "test@test.com",
            TipoUsuario = "cliente",
            TiendaId = "tienda-test"
        };
        _dbContext.Users.Add(usuario);
        await _dbContext.SaveChangesAsync();

        var dto = new ActualizarPerfilDto
        {
            Nombre = "Juan Actualizado",
            Apellido = "Perez",
            Telefono = "55512345"
        };

        var result = await _service.ActualizarPerfilAsync(usuario.Id, dto, CancellationToken.None);

        Assert.Equal("Juan Actualizado Perez", $"{result.Nombre} {result.Apellido}");
        Assert.Equal("55512345", result.Telefono);
    }

    [Fact]
    public async Task ActualizarPerfilAsync_CambioContrasena_ActualizaContrasena()
    {
        var usuario = new PlatformUser
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Juan",
            Email = "test@test.com",
            TipoUsuario = "cliente",
            TiendaId = "tienda-test"
        };
        _dbContext.Users.Add(usuario);
        await _dbContext.SaveChangesAsync();

        var dto = new ActualizarPerfilDto { Contrasena = "NewPassword123!" };

        await _service.ActualizarPerfilAsync(usuario.Id, dto, CancellationToken.None);

        var account = await _dbContext.Accounts.FirstOrDefaultAsync(a => a.UserId == usuario.Id);
        Assert.NotNull(account);
        Assert.True(PasswordHashing.Verify("NewPassword123!", account.Password));
    }
}