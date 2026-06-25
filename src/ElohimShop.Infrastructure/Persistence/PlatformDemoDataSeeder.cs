using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ElohimShop.Domain.Platform;
using PlatformUser = ElohimShop.Domain.Platform.User;
using ElohimShop.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ElohimShop.Infrastructure.Persistence;

public static class PlatformDemoDataSeeder
{
    private const string DemoMarkerSku = "DEMO-LECHE-DESL";
    private const string DemoPassword = "Demo123!";

    public static async Task SeedAsync(
        PlatformDbContext dbContext,
        bool enabled,
        ILogger logger,
        CancellationToken cancellationToken = default)
    {
        if (!enabled)
        {
            return;
        }

        // Check if there is any Tienda. If not, create a default one.
        var tienda = await dbContext.Tiendas.FirstOrDefaultAsync(cancellationToken);
        if (tienda == null)
        {
            logger.LogInformation("No se encontraron tiendas. Creando tienda demo...");
            tienda = new Tienda
            {
                Id = "7a2a29c5-393e-43e8-afb5-8363b95ef07e",
                Nombre = "DM Hub",
                Slug = "dmhub",
                Estado = "activo",
                ConfiguracionVisual = "{}",
                FechaCreacion = DateTime.UtcNow
            };
            dbContext.Tiendas.Add(tienda);
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        // Check environment configuration for SEED_USER_EMAIL and SEED_USER_PASSWORD
        var seedUserEmail = Environment.GetEnvironmentVariable("SEED_USER_EMAIL")?.Trim();
        var seedUserPassword = Environment.GetEnvironmentVariable("SEED_USER_PASSWORD")?.Trim();
        
        string targetTiendaId = tienda.Id;

        if (!string.IsNullOrEmpty(seedUserEmail))
        {
            var seedUser = await dbContext.Users.IgnoreQueryFilters()
                .FirstOrDefaultAsync(u => u.Email == seedUserEmail, cancellationToken);

            if (seedUser == null)
            {
                logger.LogInformation("Creando usuario seed ({Email}) en PlatformDbContext...", seedUserEmail);
                seedUser = new PlatformUser
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = "Administrador Seed",
                    Email = seedUserEmail,
                    EmailVerified = true,
                    TiendaId = tienda.Id,
                    TipoUsuario = "staff",
                    RolStaff = "administrador",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                dbContext.Users.Add(seedUser);

                var seedAccount = new Account
                {
                    Id = Guid.NewGuid().ToString(),
                    UserId = seedUser.Id,
                    ProviderId = "credential",
                    AccountId = seedUserEmail,
                    Password = PasswordHashing.Hash(string.IsNullOrEmpty(seedUserPassword) ? DemoPassword : seedUserPassword),
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                dbContext.Accounts.Add(seedAccount);
                await dbContext.SaveChangesAsync(cancellationToken);
                logger.LogInformation("Usuario seed ({Email}) creado y enlazado a la tienda ({TiendaId}).", seedUserEmail, tienda.Id);
            }
            else if (string.IsNullOrEmpty(seedUser.TiendaId))
            {
                seedUser.TiendaId = tienda.Id;
                await dbContext.SaveChangesAsync(cancellationToken);
                logger.LogInformation("Usuario seed existente ({Email}) enlazado a la tienda ({TiendaId}).", seedUserEmail, tienda.Id);
            }

            targetTiendaId = seedUser.TiendaId;
        }

        // Check if there is any Sucursal for this tienda.
        var sucursal = await dbContext.Sucursales.IgnoreQueryFilters()
            .FirstOrDefaultAsync(s => s.TiendaId == targetTiendaId, cancellationToken);
        if (sucursal == null)
        {
            logger.LogInformation("Creando sucursal demo para la tienda {TiendaId}...", targetTiendaId);
            sucursal = new Sucursal
            {
                Id = "sucursal-demo-" + Guid.NewGuid().ToString("N").Substring(0, 8),
                TiendaId = targetTiendaId,
                Nombre = "Sucursal Principal",
                Direccion = "Avenida Central 12-34, Zona 1",
                Telefono = "2233-4455",
                FechaCreacion = DateTime.UtcNow
            };
            dbContext.Sucursales.Add(sucursal);
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        // Check if products exist for this store
        var existeDemo = await dbContext.Productos.IgnoreQueryFilters()
            .AnyAsync(p => p.Sku == DemoMarkerSku && p.TiendaId == targetTiendaId, cancellationToken);

        if (existeDemo)
        {
            // If demo products already exist, check if we need to seed reservations
            var tieneReservas = await dbContext.Reservaciones.IgnoreQueryFilters()
                .AnyAsync(r => r.TiendaId == targetTiendaId, cancellationToken);
            if (!tieneReservas)
            {
                logger.LogInformation("La tienda {TiendaId} tiene productos pero no reservaciones. Generando datos de reservaciones...", targetTiendaId);
                var productos = await dbContext.Productos.IgnoreQueryFilters()
                    .Where(p => p.TiendaId == targetTiendaId)
                    .ToListAsync(cancellationToken);
                await GenerarReservacionesDemoAsync(dbContext, targetTiendaId, sucursal.Id, productos, logger, cancellationToken);
            }
            return;
        }

        logger.LogInformation("Cargando productos de demostración para la tienda {TiendaId} en PlatformDbContext...", targetTiendaId);

        var categoria = new Categoria
        {
            Id = Guid.NewGuid().ToString(),
            TiendaId = targetTiendaId,
            Nombre = "Lácteos",
            Descripcion = "Productos lácteos y derivados",
            Slug = "lacteos"
        };
        dbContext.Categorias.Add(categoria);
        await dbContext.SaveChangesAsync(cancellationToken);

        var productosDemo = CrearProductosDemo(targetTiendaId, categoria.Id);
        dbContext.Productos.AddRange(productosDemo);
        await dbContext.SaveChangesAsync(cancellationToken);

        // Seed inventories
        foreach (var prod in productosDemo)
        {
            var inv = new Inventario
            {
                Id = Guid.NewGuid().ToString(),
                TiendaId = targetTiendaId,
                SucursalId = sucursal.Id,
                ProductoId = prod.Id,
                Stock = prod.StockActual
            };
            dbContext.Inventarios.Add(inv);
        }
        await dbContext.SaveChangesAsync(cancellationToken);

        // Generate reservations and details
        await GenerarReservacionesDemoAsync(dbContext, targetTiendaId, sucursal.Id, productosDemo, logger, cancellationToken);
    }

    private static List<Producto> CrearProductosDemo(string tiendaId, string categoriaId)
    {
        (string sku, string nombre, decimal precio, int stock, int min)[] items =
        [
            (DemoMarkerSku, "Leche Deslactosada", 16m, 80, 20),
            ("DEMO-YOGURT-FRESA", "Yogurt Fresa", 15m, 60, 15),
            ("DEMO-QUESO-FRESCO", "Queso Fresco", 25m, 45, 15),
            ("DEMO-MANTEQUILLA", "Mantequilla", 20m, 40, 15),
            ("DEMO-CREMA-LECHE", "Crema de Leche", 20m, 5, 20),
            ("DEMO-YOGURT-NAT", "Yogurt Natural", 12m, 8, 15),
            ("DEMO-QUESO-MOZZ", "Queso Mozzarella", 30m, 3, 10),
            ("DEMO-LECHE-ENTERA", "Leche Entera", 14m, 12, 20)
        ];

        return items
            .Select(i => new Producto
            {
                Id = Guid.NewGuid().ToString(),
                TiendaId = tiendaId,
                CategoriaId = categoriaId,
                Nombre = i.nombre,
                Descripcion = $"Producto demo {i.nombre}",
                Sku = i.sku,
                PrecioMayoreo = i.precio * 0.85m,
                PrecioDetalle = i.precio,
                Publicado = true,
                StockActual = i.stock,
                StockMinimo = i.min,
                FechaCreacion = DateTime.UtcNow.AddDays(-30)
            })
            .ToList();
    }

    private static async Task GenerarReservacionesDemoAsync(
        PlatformDbContext dbContext,
        string tiendaId,
        string sucursalId,
        List<Producto> productos,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        // Find or create a client user
        var cliente = await dbContext.Users.IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Email == "cliente.demo@dmhub.gt" && u.TiendaId == tiendaId, cancellationToken);
        if (cliente == null)
        {
            cliente = new PlatformUser
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Cliente Demo",
                Email = "cliente.demo@dmhub.gt",
                EmailVerified = true,
                TiendaId = tiendaId,
                TipoUsuario = "cliente",
                Telefono = "5566-7788",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            dbContext.Users.Add(cliente);

            var account = new Account
            {
                Id = Guid.NewGuid().ToString(),
                UserId = cliente.Id,
                ProviderId = "credential",
                AccountId = cliente.Email,
                Password = PasswordHashing.Hash(DemoPassword),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            dbContext.Accounts.Add(account);
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        var random = new Random(42);
        var ahora = DateTime.UtcNow;
        var reservaciones = new List<Reservacion>();

        for (var dia = 0; dia < 25; dia++)
        {
            for (var hora = 8; hora <= 18; hora++)
            {
                var transacciones = hora is >= 12 and <= 14 ? random.Next(2, 5) : random.Next(0, 3);
                for (var t = 0; t < transacciones; t++)
                {
                    var fecha = new DateTime(
                        ahora.Year,
                        ahora.Month,
                        ahora.Day,
                        hora,
                        random.Next(0, 59),
                        0,
                        DateTimeKind.Utc).AddDays(-dia);

                    var reservacion = new Reservacion
                    {
                        Id = Guid.NewGuid().ToString(),
                        TiendaId = tiendaId,
                        SucursalId = sucursalId,
                        UsuarioId = cliente.Id,
                        EstadoPago = random.Next(10) < 8 ? "pagado" : "pendiente",
                        EstadoDespacho = "entregado",
                        FechaReserva = fecha,
                        StripeIntentId = random.Next(2) == 0 ? "pi_mock_" + Guid.NewGuid().ToString("N").Substring(0, 16) : null
                    };

                    decimal total = 0;
                    var cantLineas = random.Next(1, 4);
                    var elegidos = productos.OrderBy(_ => random.Next()).Take(cantLineas).ToList();

                    foreach (var prod in elegidos)
                    {
                        var cant = random.Next(1, 3);
                        var precio = prod.PrecioDetalle;
                        var subtotal = cant * precio;
                        total += subtotal;

                        var detalle = new DetalleReservacion
                        {
                            Id = Guid.NewGuid().ToString(),
                            ReservacionId = reservacion.Id,
                            ProductoId = prod.Id,
                            Cantidad = cant,
                            PrecioCobrado = precio
                        };
                        reservacion.Detalles.Add(detalle);
                    }

                    reservacion.MontoTotal = total;
                    reservaciones.Add(reservacion);
                }
            }
        }

        dbContext.Reservaciones.AddRange(reservaciones);
        await dbContext.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Generadas {Count} reservaciones de demostración en PlatformDbContext.", reservaciones.Count);
    }
}
