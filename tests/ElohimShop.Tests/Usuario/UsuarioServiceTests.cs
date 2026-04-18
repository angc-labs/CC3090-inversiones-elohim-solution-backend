using ElohimShop.Application.Usuario;
using ElohimShop.Domain.Entities;
using ElohimShop.Infrastructure.Persistence;
using ElohimShop.Infrastructure.Security;
using ElohimShop.Infrastructure.User;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace ElohimShop.Tests.Usuario;

public class UsuarioServiceTests
{
    private readonly ElohimShopDbContext _dbContext;
    private readonly IPasswordHashing _passwordHashing;
    private readonly UsuarioService _service;

    public UsuarioServiceTests()
    {
        var options = new DbContextOptionsBuilder<ElohimShopDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContext = new ElohimShopDbContext(options);
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
        var usuario = Domain.Entities.Usuario.CrearCliente("test@test.com", "Juan", "hashed", "minorista", "Perez", "123456", "Zona 10");
        _dbContext.Usuarios.Add(usuario);
        await _dbContext.SaveChangesAsync();

        var result = await _service.ObtenerPerfilAsync(usuario.Id, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal("test@test.com", result.Correo);
        Assert.Equal("Juan", result.Nombre);
        Assert.Equal("cliente", result.TipoUsuario);
        Assert.Equal("minorista", result.TipoCliente);
    }

    [Fact]
    public async Task ObtenerPerfilAsync_Administrador_ReturnsPerfil()
    {
        var usuario = Domain.Entities.Usuario.CrearAdministrador("admin@test.com", "Admin", "hashed", "cajero");
        _dbContext.Usuarios.Add(usuario);
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
        var usuario1 = Domain.Entities.Usuario.CrearCliente("test1@test.com", "User1", "hashed", "minorista");
        var usuario2 = Domain.Entities.Usuario.CrearCliente("test2@test.com", "User2", "hashed", "minorista");
        _dbContext.Usuarios.AddRange(usuario1, usuario2);
        await _dbContext.SaveChangesAsync();

        var dto = new ActualizarPerfilDto { Correo = "test2@test.com" };

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.ActualizarPerfilAsync(usuario1.Id, dto, CancellationToken.None));
    }

    [Fact]
    public async Task ActualizarPerfilAsync_DatosValidos_ActualizaPerfil()
    {
        var usuario = Domain.Entities.Usuario.CrearCliente("test@test.com", "Juan", "hashed", "minorista");
        _dbContext.Usuarios.Add(usuario);
        await _dbContext.SaveChangesAsync();

        var dto = new ActualizarPerfilDto
        {
            Nombre = "Juan Actualizado",
            Apellido = "Perez",
            Telefono = "55512345"
        };

        var result = await _service.ActualizarPerfilAsync(usuario.Id, dto, CancellationToken.None);

        Assert.Equal("Juan Actualizado", result.Nombre);
        Assert.Equal("Perez", result.Apellido);
    }

    [Fact]
    public async Task ActualizarPerfilAsync_CambioContrasena_ActualizaContrasena()
    {
        var usuario = Domain.Entities.Usuario.CrearCliente("test@test.com", "Juan", "hashed", "minorista");
        _dbContext.Usuarios.Add(usuario);
        await _dbContext.SaveChangesAsync();

        var dto = new ActualizarPerfilDto { Contrasena = "NewPassword123!" };

        await _service.ActualizarPerfilAsync(usuario.Id, dto, CancellationToken.None);

        var usuarioActualizado = await _dbContext.Usuarios.FirstAsync(u => u.Id == usuario.Id);
        Assert.NotEqual("hashed", usuarioActualizado.Contrasena);
    }

    [Fact]
    public async Task ActualizarPerfilAsync_Direccion_ActualizaDireccion()
    {
        var usuario = Domain.Entities.Usuario.CrearCliente("test@test.com", "Juan", "hashed", "minorista", null, null, "Zona 1");
        _dbContext.Usuarios.Add(usuario);
        await _dbContext.SaveChangesAsync();

        var dto = new ActualizarPerfilDto { Direccion = "Zona 10" };

        var result = await _service.ActualizarPerfilAsync(usuario.Id, dto, CancellationToken.None);

        Assert.NotNull(result);
    }
}