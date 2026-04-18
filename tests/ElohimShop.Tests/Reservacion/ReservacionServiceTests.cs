using ElohimShop.Application.Reservacion;
using ElohimShop.Domain.Entities;
using ElohimShop.Infrastructure.Catalog;
using ElohimShop.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace ElohimShop.Tests.Reservacion;

public class ReservacionServiceTests
{
    private readonly ElohimShopDbContext _dbContext;
    private readonly ReservacionService _service;

    public ReservacionServiceTests()
    {
        var options = new DbContextOptionsBuilder<ElohimShopDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContext = new ElohimShopDbContext(options);
        _service = new ReservacionService(_dbContext);
    }

    [Fact]
    public async Task CrearReservacionAsync_CarritoVacio_ThrowsException()
    {
        var dto = new CrearReservacionDto { MetodoPagoId = "metodo-123" };

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.CrearReservacionAsync("cliente-123", dto));
    }

    [Fact]
    public async Task CrearReservacionAsync_MetodoPagoInvalido_ThrowsException()
    {
        var producto = ElohimShop.Domain.Entities.Producto.Crear("PROD-001", "Producto Test", 100, 5);
        _dbContext.Productos.Add(producto);
        
        var metodoPago = ElohimShop.Domain.Entities.MetodoPago.Crear("otro-usuario", "Efectivo");
        _dbContext.MetodosPago.Add(metodoPago);
        await _dbContext.SaveChangesAsync();

        var carrito = ElohimShop.Domain.Entities.Carrito.Crear("cliente-123");
        var articulo = ElohimShop.Domain.Entities.ArticuloCarrito.Crear(carrito.IdCarrito, producto.IdProducto, producto.NombreProducto, 2, producto.Precio);
        _dbContext.Carritos.Add(carrito);
        _dbContext.ArticulosCarrito.Add(articulo);
        await _dbContext.SaveChangesAsync();

        var dto = new CrearReservacionDto { MetodoPagoId = "metodo-inexistente" };

        await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.CrearReservacionAsync("cliente-123", dto));
    }

    [Fact]
    public async Task CrearReservacionAsync_DatosValidos_CreaReservacion()
    {
        var producto = ElohimShop.Domain.Entities.Producto.Crear("PROD-001", "Producto Test", 100, 5);
        _dbContext.Productos.Add(producto);

        var metodoPago = ElohimShop.Domain.Entities.MetodoPago.Crear("cliente-123", "Efectivo");
        _dbContext.MetodosPago.Add(metodoPago);
        await _dbContext.SaveChangesAsync();

        var carrito = ElohimShop.Domain.Entities.Carrito.Crear("cliente-123");
        var articulo = ElohimShop.Domain.Entities.ArticuloCarrito.Crear(carrito.IdCarrito, producto.IdProducto, producto.NombreProducto, 1, producto.Precio);
        _dbContext.Carritos.Add(carrito);
        _dbContext.ArticulosCarrito.Add(articulo);
        await _dbContext.SaveChangesAsync();

        var dto = new CrearReservacionDto { MetodoPagoId = metodoPago.IdMetodoPago };

        var result = await _service.CrearReservacionAsync("cliente-123", dto);

        Assert.NotNull(result);
        Assert.Equal("pendiente", result.Estado);
        Assert.Equal(100, result.TotalReservacion);
        Assert.Single(result.Items);
    }

    [Fact]
    public async Task ObtenerReservacionesAsync_Cliente_ReturnsSoloSusReservaciones()
    {
        var reservacion1 = ElohimShop.Domain.Entities.Reservacion.Crear("cliente-123", null);
        reservacion1.CalcularTotal();
        var reservacion2 = ElohimShop.Domain.Entities.Reservacion.Crear("cliente-456", null);
        reservacion2.CalcularTotal();
        
        _dbContext.Reservaciones.AddRange(reservacion1, reservacion2);
        await _dbContext.SaveChangesAsync();

        var result = await _service.ObtenerReservacionesAsync("cliente-123", esAdministrador: false);

        Assert.Single(result);
        Assert.Equal("cliente-123", result[0].ClienteId);
    }

    [Fact]
    public async Task ObtenerReservacionesAsync_Administrador_ReturnsTodas()
    {
        var reservacion1 = ElohimShop.Domain.Entities.Reservacion.Crear("cliente-123", null);
        reservacion1.CalcularTotal();
        var reservacion2 = ElohimShop.Domain.Entities.Reservacion.Crear("cliente-456", null);
        reservacion2.CalcularTotal();

        _dbContext.Reservaciones.AddRange(reservacion1, reservacion2);
        await _dbContext.SaveChangesAsync();

        var result = await _service.ObtenerReservacionesAsync(null, esAdministrador: true);

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task ObtenerReservacionPorIdAsync_ReservacionNoExiste_ReturnsNull()
    {
        var result = await _service.ObtenerReservacionPorIdAsync("reservacion-inexistente", "cliente-123", false);

        Assert.Null(result);
    }

    [Fact]
    public async Task ObtenerReservacionPorIdAsync_ClienteAccedeOtra_ThrowsException()
    {
        var reservacion = ElohimShop.Domain.Entities.Reservacion.Crear("cliente-456", null);
        reservacion.CalcularTotal();
        _dbContext.Reservaciones.Add(reservacion);
        await _dbContext.SaveChangesAsync();

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _service.ObtenerReservacionPorIdAsync(reservacion.IdReservacion, "cliente-123", esAdministrador: false));
    }
}