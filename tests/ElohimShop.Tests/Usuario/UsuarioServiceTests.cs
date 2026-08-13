using ElohimShop.Application.Platform;
using ElohimShop.Domain.Platform;
using ElohimShop.Infrastructure.Persistence;
using ElohimShop.Infrastructure.Platform;
using ElohimShop.Infrastructure.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;
using PlatformUser = ElohimShop.Domain.Platform.User;

namespace ElohimShop.Tests.Usuario;

public class UsuarioServiceTests
{
    private const string TestTenantId = "tienda-test";

    private readonly PlatformDbContext _dbContext;
    private readonly PlatformService _service;

    public UsuarioServiceTests()
    {
        var tenantProvider = new Mock<ITenantProvider>();
        tenantProvider.Setup(x => x.GetTenantId()).Returns(TestTenantId);

        var options = new DbContextOptionsBuilder<PlatformDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _dbContext = new PlatformDbContext(options, tenantProvider.Object);
        _service = new PlatformService(
            _dbContext,
            tenantProvider.Object,
            new Mock<IHttpContextAccessor>().Object);
    }

    [Fact]
    public async Task ObtenerUsuarioAsync_UsuarioNoExiste_ReturnsNull()
    {
        var result = await _service.ObtenerUsuarioAsync(
            "usuario-inexistente",
            CancellationToken.None);

        Assert.Null(result);
    }

    [Fact]
    public async Task ObtenerUsuarioAsync_UsuarioExiste_ReturnsUsuario()
    {
        var usuario = CrearUsuario("test@test.com", "Juan Perez");
        _dbContext.Users.Add(usuario);
        await _dbContext.SaveChangesAsync();

        var result = await _service.ObtenerUsuarioAsync(usuario.Id, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal("test@test.com", result.Email);
        Assert.Equal("Juan Perez", result.Name);
        Assert.Equal("cliente", result.TipoUsuario);
    }

    [Fact]
    public async Task ListarUsuariosAsync_FiltraUsuariosPorTienda()
    {
        _dbContext.Users.AddRange(
            CrearUsuario("actual@test.com", "Usuario Actual"),
            CrearUsuario("otro@test.com", "Usuario Otra Tienda", "otra-tienda"));
        await _dbContext.SaveChangesAsync();

        var result = await _service.ListarUsuariosAsync(CancellationToken.None);

        var usuario = Assert.Single(result);
        Assert.Equal("actual@test.com", usuario.Email);
    }

    [Fact]
    public async Task InvitarUsuarioAsync_CorreoExistente_ThrowsException()
    {
        _dbContext.Users.Add(CrearUsuario("test@test.com", "Usuario Existente"));
        await _dbContext.SaveChangesAsync();
        var request = new InvitarPlatformUsuarioRequest(
            "TEST@test.com",
            "Nuevo Usuario",
            "staff",
            "cajero",
            "Password123!");

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.InvitarUsuarioAsync(request, CancellationToken.None));
    }

    [Fact]
    public async Task InvitarUsuarioAsync_DatosValidos_CreaUsuarioYCuenta()
    {
        var request = new InvitarPlatformUsuarioRequest(
            "  NUEVO@test.com ",
            "  Nuevo Usuario ",
            "staff",
            "cajero",
            "Password123!");

        var result = await _service.InvitarUsuarioAsync(request, CancellationToken.None);

        Assert.Equal("nuevo@test.com", result.Email);
        Assert.Equal("Nuevo Usuario", result.Name);
        Assert.Equal("staff", result.TipoUsuario);
        Assert.Equal("cajero", result.RolStaff);

        var account = await _dbContext.Accounts.SingleAsync(x => x.UserId == result.Id);
        Assert.NotNull(account.Password);
        Assert.True(PasswordHashing.Verify("Password123!", account.Password));
    }

    [Fact]
    public async Task CambiarRolUsuarioAsync_UsuarioNoExiste_ReturnsNull()
    {
        var request = new CambiarRolPlatformUsuarioRequest("staff", "administrador");

        var result = await _service.CambiarRolUsuarioAsync(
            "usuario-inexistente",
            request,
            CancellationToken.None);

        Assert.Null(result);
    }

    [Fact]
    public async Task CambiarRolUsuarioAsync_UsuarioExiste_ActualizaRol()
    {
        var usuario = CrearUsuario("test@test.com", "Juan Perez");
        _dbContext.Users.Add(usuario);
        await _dbContext.SaveChangesAsync();

        var result = await _service.CambiarRolUsuarioAsync(
            usuario.Id,
            new CambiarRolPlatformUsuarioRequest("staff", "administrador"),
            CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal("staff", result.TipoUsuario);
        Assert.Equal("administrador", result.RolStaff);
    }

    [Fact]
    public async Task CambiarEstadoUsuarioAsync_UsuarioExiste_ActualizaEstado()
    {
        var usuario = CrearUsuario("test@test.com", "Juan Perez");
        _dbContext.Users.Add(usuario);
        await _dbContext.SaveChangesAsync();

        var result = await _service.CambiarEstadoUsuarioAsync(
            usuario.Id,
            false,
            CancellationToken.None);

        Assert.NotNull(result);
        Assert.False(result.Estado);
    }

    private static PlatformUser CrearUsuario(
        string correo,
        string nombre,
        string tenantId = TestTenantId)
    {
        return new PlatformUser
        {
            Id = Guid.NewGuid().ToString(),
            Name = nombre,
            Email = correo,
            TipoUsuario = "cliente",
            TiendaId = tenantId,
            Estado = true
        };
    }
}
