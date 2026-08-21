using ElohimShop.Application.Platform;
using ElohimShop.Domain.Platform;
using ElohimShop.Infrastructure.Persistence;
using ElohimShop.Infrastructure.Platform;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

// ============================================================================
// ENDPOINT CUBIERTO: CarritoV1Controller -> /api/v1/carrito
// CAPA TESTEADA: Servicio (PlatformService), NO el controlador HTTP directamente.
//   GET    /api/v1/carrito            -> ObtenerCarritoAsync()
//   POST   /api/v1/carrito            -> AgregarArticuloAsync()
//   PUT    /api/v1/carrito/{itemId}   -> ActualizarArticuloAsync()
//   DELETE /api/v1/carrito/{itemId}   -> EliminarArticuloAsync()
//
// Referencia: PLAN_ACCIONES_TESTS.md / SWAGGER_TESTS_MAPPING.md marcan este
// archivo como "ADAPTER" pendiente: falta crear tests de controlador que
// validen códigos HTTP (200/404/401/403) y autenticación/autorización.
// ============================================================================

// test class for CarritoService
namespace ElohimShop.Tests.Carrito;

public class CarritoServiceTests
{
    private const string TestTenantId = "tienda-test";
    private const string TestUsuarioId = "cliente-123";

    private readonly PlatformDbContext _dbContext;
    private readonly PlatformService _service;

    //Test del endpoint de carrito, se crean pruebas unitarias para cada metodo del servicio de carrito
    // Setup: usa EF Core InMemory Database (aislada por Guid en cada test) y
    // mockea ITenantProvider/IHttpContextAccessor para simular el contexto
    // multi-tenant sin dependencias externas reales.
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
// Test methods for CarritoService
    [Fact]
    // GET /api/v1/carrito | caso: usuario sin artículos.
    // Verifica que un carrito nuevo/vacío devuelva una colección vacía
    // (sin excepciones ni nulls inesperados).
    public async Task ObtenerCarritoAsync_CarritoVacio_ReturnsEmpty()
    {
        var result = await _service.ObtenerCarritoAsync(TestUsuarioId, CancellationToken.None);

        Assert.Empty(result);
    }
// Enpoint test for ObtenerCarritoAsync method, it filters by user and store
    [Fact]
    // GET /api/v1/carrito | caso: aislamiento multi-tenant + por usuario.
    // Crea artículos de OTRO cliente y de OTRA tienda a propósito, y verifica
    // que ObtenerCarritoAsync solo devuelva el artículo que pertenece al
    // usuario Y tienda actuales. Es el test más crítico del archivo porque
    // valida que no haya fuga de datos entre tenants (multi-tienda SaaS).
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
// 
    // POST /api/v1/carrito | caso: producto válido, agregarlo por primera vez.
    // Verifica que se calculen correctamente cantidad, precio unitario
    // y subtotal (precio * cantidad) al insertar un nuevo artículo.
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
    // POST /api/v1/carrito | caso: producto repetido en el mismo carrito.
    // Verifica que agregar dos veces el mismo producto ACUMULE la cantidad
    // en la fila existente, en vez de crear un registro duplicado.
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
    // PUT /api/v1/carrito/{itemId} | caso: artículo inexistente.
    // Verifica que actualizar un id que no existe no lance excepción,
    // sino que retorne null (permitiendo al controlador mapearlo a 404).
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
    // PUT /api/v1/carrito/{itemId} | caso: artículo válido.
    // Verifica que la cantidad se actualice correctamente sobre un
    // artículo previamente agregado al carrito.
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
    // DELETE /api/v1/carrito/{itemId} | caso: artículo válido.
    // Verifica que eliminar un artículo lo borre físicamente de la base
    // de datos (no soft-delete) y que el método retorne true.
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

    // Helper: crea un Producto de prueba con precio configurable.
    // PrecioMayoreo se calcula como 90% del precio de detalle.
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

    // Helper: crea un CarritoElemento de prueba, permitiendo variar
    // usuarioId y tiendaId para los escenarios de aislamiento multi-tenant.
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

// ============================================================================
// CONCLUSIONES
// ----------------------------------------------------------------------------
// 1. Cobertura de lógica de negocio: BUENA. Los 4 métodos del service tienen
//    al menos un happy path, más el caso crítico de aislamiento multi-tenant.
//
// 2. Cobertura de casos límite: PARCIAL. Falta cubrir:
//    - AgregarArticuloAsync con producto inexistente o sin stock suficiente.
//    - EliminarArticuloAsync con artículo inexistente (¿retorna false?).
//    - ActualizarArticuloAsync con cantidad 0 o negativa.
//
// 3. Brecha principal: son tests de SERVICIO, no de CONTROLADOR. No validan
//    códigos HTTP, autenticación (JWT) ni autorización por rol. Pendiente
//    crear Carrito/CarritoControllerTests.cs para completar el punto 2 del
//    plan de acción.
// ============================================================================