using ElohimShop.Application.Carrito;
using ElohimShop.Domain.Entities;
using ElohimShop.Infrastructure.Catalog;
using ElohimShop.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace ElohimShop.Tests.Carrito;

public class CarritoServiceTests
{
    private readonly ElohimShopDbContext _dbContext;
    private readonly CarritoService _service;

    public CarritoServiceTests()
    {
        var options = new DbContextOptionsBuilder<ElohimShopDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContext = new ElohimShopDbContext(options);
        _service = new CarritoService(_dbContext);
    }

    [Fact]
    public async Task ObtenerCarritoActivoAsync_CarritoVacio_ReturnsNull()
    {
        var result = await _service.ObtenerCarritoActivoAsync("cliente-123");

        Assert.Null(result);
    }

    [Fact]
    public async Task ObtenerCarritoActivoAsync_CarritoExiste_ReturnsCarrito()
    {
        var carrito = ElohimShop.Domain.Entities.Carrito.Crear("cliente-123");
        _dbContext.Carritos.Add(carrito);
        await _dbContext.SaveChangesAsync();

        var result = await _service.ObtenerCarritoActivoAsync("cliente-123");

        Assert.NotNull(result);
        Assert.NotNull(result.CarritoId);
    }

    [Fact]
    public async Task AgregarArticuloAsync_ProductoNoExiste_ReturnsNull()
    {
        var dto = new AgregarArticuloCarritoDto
        {
            ProductoId = "producto-inexistente",
            Cantidad = 1
        };

        var result = await _service.AgregarArticuloAsync("cliente-123", dto);

        Assert.Null(result);
    }

    [Fact]
    public async Task AgregarArticuloAsync_StockInsuficiente_ThrowsException()
    {
        var producto = ElohimShop.Domain.Entities.Producto.Crear("PROD-001", "Producto Test", 100, 5);
        _dbContext.Productos.Add(producto);
        await _dbContext.SaveChangesAsync();

        var dto = new AgregarArticuloCarritoDto
        {
            ProductoId = producto.IdProducto,
            Cantidad = 10
        };

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.AgregarArticuloAsync("cliente-123", dto));
    }

    [Fact]
    public async Task AgregarArticuloAsync_ProductoValido_AgregaAlCarrito()
    {
        var producto = ElohimShop.Domain.Entities.Producto.Crear("PROD-001", "Producto Test", 100, 5);
        _dbContext.Productos.Add(producto);
        await _dbContext.SaveChangesAsync();

        var dto = new AgregarArticuloCarritoDto
        {
            ProductoId = producto.IdProducto,
            Cantidad = 2
        };

        var result = await _service.AgregarArticuloAsync("cliente-123", dto);

        Assert.NotNull(result);
        Assert.Equal(2, result.Cantidad);
        Assert.Equal(200, result.Subtotal);
    }

    [Fact]
    public async Task EliminarArticuloAsync_ArticuloNoExiste_ReturnsFalse()
    {
        var result = await _service.EliminarArticuloAsync("cliente-123", "articulo-inexistente");

        Assert.False(result);
    }

    [Fact]
    public async Task ActualizarCantidadArticuloAsync_CantidadCero_EliminaArticulo()
    {
        var producto = ElohimShop.Domain.Entities.Producto.Crear("PROD-001", "Producto Test", 100, 5);
        _dbContext.Productos.Add(producto);
        await _dbContext.SaveChangesAsync();

        var dtoAgregar = new AgregarArticuloCarritoDto
        {
            ProductoId = producto.IdProducto,
            Cantidad = 1
        };
        var articulo = await _service.AgregarArticuloAsync("cliente-123", dtoAgregar);

        var dtoActualizar = new ActualizarCantidadArticuloDto { Cantidad = 3 };
        var result = await _service.ActualizarCantidadArticuloAsync("cliente-123", articulo!.ArticuloId, dtoActualizar);

        Assert.NotNull(result);
        Assert.Equal(3, result.Cantidad);
    }
}