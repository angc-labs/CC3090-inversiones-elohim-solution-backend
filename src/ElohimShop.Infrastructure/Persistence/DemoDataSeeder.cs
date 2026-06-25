using System.Reflection;
using ElohimShop.Domain.Entities;
using ElohimShop.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ElohimShop.Infrastructure.Persistence;

public static class DemoDataSeeder
{
    private const string DemoMarkerCodigo = "DEMO-LECHE-DESL";
    private const string DemoPassword = "Demo123!";

    public static async Task SeedAsync(
        ElohimShopDbContext dbContext,
        bool enabled,
        ILogger logger,
        CancellationToken cancellationToken = default)
    {
        if (!enabled)
        {
            return;
        }

        var seedUserEmail = Environment.GetEnvironmentVariable("SEED_USER_EMAIL")?.Trim();
        var seedUserPassword = Environment.GetEnvironmentVariable("SEED_USER_PASSWORD")?.Trim();

        if (!string.IsNullOrEmpty(seedUserEmail))
        {
            var existeAdmin = await dbContext.Usuarios
                .AsNoTracking()
                .AnyAsync(u => u.Correo == seedUserEmail, cancellationToken);

            if (!existeAdmin)
            {
                logger.LogInformation("Creando usuario administrador seed ({Email}) en ElohimShopDbContext...", seedUserEmail);
                var adminUser = Usuario.CrearAdministrador(
                    seedUserEmail,
                    "Administrador Seed",
                    PasswordHashing.Hash(string.IsNullOrEmpty(seedUserPassword) ? DemoPassword : seedUserPassword),
                    "administrador");
                dbContext.Usuarios.Add(adminUser);
                await dbContext.SaveChangesAsync(cancellationToken);
            }
        }

        var existeDemo = await dbContext.Productos
            .AsNoTracking()
            .AnyAsync(p => p.CodigoProducto == DemoMarkerCodigo, cancellationToken);

        if (existeDemo)
        {
            return;
        }

        logger.LogInformation("Cargando datos de demostración para reportes...");

        var marca = Marca.Crear("DM Hub");
        var categoria = Categoria.Crear("Lácteos");
        dbContext.Marcas.Add(marca);
        dbContext.Categorias.Add(categoria);

        var productos = CrearProductosDemo(marca.Id, categoria.Id);
        dbContext.Productos.AddRange(productos);

        var cliente = Usuario.CrearCliente(
            "cliente.demo@dmhub.gt",
            "Cliente Demo",
            PasswordHashing.Hash(DemoPassword),
            "particular");
        dbContext.Usuarios.Add(cliente);

        var cajeros = new[]
        {
            Usuario.CrearAdministrador("carlos.demo@dmhub.gt", "Carlos", PasswordHashing.Hash(DemoPassword), "cajero", "Ruiz"),
            Usuario.CrearAdministrador("ana.demo@dmhub.gt", "Ana", PasswordHashing.Hash(DemoPassword), "cajero", "López"),
            Usuario.CrearAdministrador("maria.demo@dmhub.gt", "María", PasswordHashing.Hash(DemoPassword), "cajero", "Soto"),
            Usuario.CrearAdministrador("pedro.demo@dmhub.gt", "Pedro", PasswordHashing.Hash(DemoPassword), "cajero", "Martínez")
        };
        dbContext.Usuarios.AddRange(cajeros);

        var metodos = new[]
        {
            MetodoPago.Crear(null, "Efectivo"),
            MetodoPago.Crear(null, "Yape"),
            MetodoPago.Crear(null, "Tarjeta"),
            MetodoPago.Crear(null, "Plin")
        };
        dbContext.MetodosPago.AddRange(metodos);

        await dbContext.SaveChangesAsync(cancellationToken);

        var mapaProductos = productos.ToDictionary(p => p.CodigoProducto);
        var reservaciones = GenerarReservacionesDemo(cliente.Id, metodos, mapaProductos);
        dbContext.Reservaciones.AddRange(reservaciones);
        await dbContext.SaveChangesAsync(cancellationToken);

        var ventas = GenerarVentasDemo(reservaciones, cajeros);
        dbContext.Ventas.AddRange(ventas);
        await dbContext.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "Datos demo: {Productos} productos, {Reservas} reservaciones, {Ventas} ventas en tienda.",
            productos.Count,
            reservaciones.Count,
            ventas.Count);
    }

    private static List<Producto> CrearProductosDemo(string marcaId, string categoriaId)
    {
        (string codigo, string nombre, int precio, int stock, int min)[] items =
        [
            (DemoMarkerCodigo, "Leche Deslactosada", 16, 80, 20),
            ("DEMO-YOGURT-FRESA", "Yogurt Fresa", 15, 60, 15),
            ("DEMO-QUESO-FRESCO", "Queso Fresco", 25, 45, 15),
            ("DEMO-MANTEQUILLA", "Mantequilla", 20, 40, 15),
            ("DEMO-CREMA-LECHE", "Crema de Leche", 20, 5, 20),
            ("DEMO-YOGURT-NAT", "Yogurt Natural", 12, 8, 15),
            ("DEMO-QUESO-MOZZ", "Queso Mozzarella", 30, 3, 10),
            ("DEMO-LECHE-ENTERA", "Leche Entera", 14, 12, 20)
        ];

        return items
            .Select(i => Producto.Crear(
                i.codigo,
                i.nombre,
                i.precio,
                i.stock,
                stockMinimo: i.min,
                descripcion: $"Producto demo {i.nombre}",
                idMarca: marcaId,
                categoriaId: categoriaId))
            .ToList();
    }

    private static List<Reservacion> GenerarReservacionesDemo(
        string clienteId,
        MetodoPago[] metodos,
        Dictionary<string, Producto> productos)
    {
        var reservaciones = new List<Reservacion>();
        var random = new Random(42);
        var ahora = DateTime.UtcNow;

        var lineasCatalogo = new (string codigo, int cantidad)[]
        {
            (DemoMarkerCodigo, 3),
            ("DEMO-YOGURT-FRESA", 2),
            ("DEMO-QUESO-FRESCO", 1),
            ("DEMO-MANTEQUILLA", 2),
            ("DEMO-CREMA-LECHE", 1),
            ("DEMO-YOGURT-NAT", 2),
            ("DEMO-QUESO-MOZZ", 1),
            ("DEMO-LECHE-ENTERA", 2)
        };

        for (var dia = 0; dia < 25; dia++)
        {
            for (var hora = 8; hora <= 18; hora++)
            {
                var transacciones = hora is >= 12 and <= 14 ? random.Next(2, 5) : random.Next(0, 3);
                for (var t = 0; t < transacciones; t++)
                {
                    var metodo = metodos[random.Next(metodos.Length)];
                    var reservacion = Reservacion.Crear(clienteId, metodo.IdMetodoPago);
                    var fecha = new DateTime(
                        ahora.Year,
                        ahora.Month,
                        ahora.Day,
                        hora,
                        random.Next(0, 59),
                        0,
                        DateTimeKind.Utc).AddDays(-dia);

                    EstablecerPropiedad(reservacion, nameof(Reservacion.FechaRenovacion), fecha);
                    EstablecerPropiedad(reservacion, nameof(Reservacion.EstadoRenovacion), "completada");
                    reservacion.MarcarComoPagada();

                    foreach (var (codigo, cantidadBase) in lineasCatalogo.OrderBy(_ => random.Next()).Take(random.Next(1, 4)))
                    {
                        var producto = productos[codigo];
                        reservacion.AgregarDetalle(
                            producto.IdProducto,
                            producto.NombreProducto,
                            cantidadBase + random.Next(0, 2),
                            producto.Precio);
                    }

                    reservacion.CalcularTotal();
                    reservaciones.Add(reservacion);
                }
            }
        }

        return reservaciones;
    }

    private static List<Venta> GenerarVentasDemo(List<Reservacion> reservaciones, Usuario[] cajeros)
    {
        var ventas = new List<Venta>();
        var random = new Random(99);
        var objetivoPorCajero = new[] { 45, 38, 32, 28 };
        var indiceReserva = 0;

        for (var i = 0; i < cajeros.Length; i++)
        {
            var cajero = cajeros[i];
            for (var v = 0; v < objetivoPorCajero[i] && indiceReserva < reservaciones.Count; v++)
            {
                var reservacion = reservaciones[indiceReserva++];
                ventas.Add(Venta.Crear(
                    reservacion.TotalRenovacion ?? 0,
                    reservacion.IdReservacion,
                    cajero.Id,
                    reservacion.FechaRenovacion.AddMinutes(random.Next(5, 90))));
            }
        }

        return ventas;
    }

    private static void EstablecerPropiedad<T>(T entidad, string nombre, object valor)
    {
        typeof(T).GetProperty(nombre, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)!
            .SetValue(entidad, valor);
    }
}
