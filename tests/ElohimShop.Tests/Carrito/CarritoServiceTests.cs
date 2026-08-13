using ElohimShop.Application.Platform;
using ElohimShop.Domain.Platform;
using ElohimShop.Infrastructure.Persistence;
using ElohimShop.Infrastructure.Platform;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace ElohimShop.Tests.Carrito;

public class CarritoServiceTests
{
    private const string TestTenantId = "tienda-test";
    private const string TestUsuarioId = "cliente-123";

    private readonly PlatformDbContext _dbContext;
    private readonly PlatformService _service;

    public CarritoServiceTests()
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
    public async Task ObtenerCarritoAsync_CarritoVacio_ReturnsEmpty()
    {
        var result = await _service.ObtenerCarritoAsync(TestUsuarioId, CancellationToken.None);

        Assert.Empty(result);
    }

    [Fact]
    public async Task ObtenerCarritoAsync_FiltraPorUsuarioYTienda()
    {
        var producto = CrearProducto();
        _dbContext.Productos.Add(producto);
        _dbContext.CarritoElementos.AddRange(
            CrearArticulo(producto.Id, TestUsuarioId, TestTenantId),
            CrearArticulo(producto.Id, "otro-cliente", TestTenantId),
            CrearArticulo(producto.Id, TestUsuarioId, "otra-tienda"));
        await _dbContext.SaveChangesAsync();

        var result = await _service.ObtenerCarritoAsync(TestUsuarioId, CancellationToken.None);

        var articulo = Assert.Single(result);
        Assert.Equal(TestTenantId, articulo.TiendaId);
        Assert.Equal(TestUsuarioId, articulo.UsuarioId);
    }

    [Fact]
    public async Task AgregarArticuloAsync_ProductoValido_AgregaAlCarrito()
    {
        var producto = CrearProducto(precio: 100m);
        _dbContext.Productos.Add(producto);
        await _dbContext.SaveChangesAsync();

        var result = await _service.AgregarArticuloAsync(
            TestUsuarioId,
            new AgregarCarritoRequest(producto.Id, 2),
            CancellationToken.None);

        Assert.Equal(2, result.Cantidad);
        Assert.Equal(100m, result.PrecioDetalle);
        Assert.Equal(200m, result.Subtotal);
    }

    [Fact]
    public async Task AgregarArticuloAsync_ProductoRepetido_AcumulaCantidad()
    {
        var producto = CrearProducto();
        _dbContext.Productos.Add(producto);
        await _dbContext.SaveChangesAsync();

        await _service.AgregarArticuloAsync(
            TestUsuarioId,
            new AgregarCarritoRequest(producto.Id, 2),
            CancellationToken.None);
        var result = await _service.AgregarArticuloAsync(
            TestUsuarioId,
            new AgregarCarritoRequest(producto.Id, 3),
            CancellationToken.None);

        Assert.Equal(5, result.Cantidad);
        Assert.Single(await _dbContext.CarritoElementos.ToListAsync());
    }

    [Fact]
    public async Task ActualizarArticuloAsync_ArticuloNoExiste_ReturnsNull()
    {
        var result = await _service.ActualizarArticuloAsync(
            TestUsuarioId,
            "articulo-inexistente",
            new ActualizarCarritoRequest(3),
            CancellationToken.None);

        Assert.Null(result);
    }

    [Fact]
    public async Task ActualizarArticuloAsync_ArticuloValido_ActualizaCantidad()
    {
        var producto = CrearProducto();
        _dbContext.Productos.Add(producto);
        await _dbContext.SaveChangesAsync();
        var articulo = await _service.AgregarArticuloAsync(
            TestUsuarioId,
            new AgregarCarritoRequest(producto.Id, 1),
            CancellationToken.None);

        var result = await _service.ActualizarArticuloAsync(
            TestUsuarioId,
            articulo.Id,
            new ActualizarCarritoRequest(3),
            CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(3, result.Cantidad);
    }

    [Fact]
    public async Task EliminarArticuloAsync_ArticuloValido_EliminaArticulo()
    {
        var producto = CrearProducto();
        _dbContext.Productos.Add(producto);
        await _dbContext.SaveChangesAsync();
        var articulo = await _service.AgregarArticuloAsync(
            TestUsuarioId,
            new AgregarCarritoRequest(producto.Id, 1),
            CancellationToken.None);

        var result = await _service.EliminarArticuloAsync(
            TestUsuarioId,
            articulo.Id,
            CancellationToken.None);

        Assert.True(result);
        Assert.Empty(await _dbContext.CarritoElementos.ToListAsync());
    }

    private static Producto CrearProducto(decimal precio = 50m)
    {
        return new Producto
        {
            TiendaId = TestTenantId,
            Nombre = "Producto Test",
            PrecioDetalle = precio,
            PrecioMayoreo = precio * 0.9m,
            StockActual = 10
        };
    }

    private static CarritoElemento CrearArticulo(string productoId, string usuarioId, string tiendaId)
    {
        return new CarritoElemento
        {
            TiendaId = tiendaId,
            UsuarioId = usuarioId,
            ProductoId = productoId,
            Cantidad = 1
        };
    }
}
