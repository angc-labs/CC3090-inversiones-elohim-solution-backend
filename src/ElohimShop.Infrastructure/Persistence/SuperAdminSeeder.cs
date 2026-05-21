using ElohimShop.Domain.Entities;
using ElohimShop.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ElohimShop.Infrastructure.Persistence;

public static class SuperAdminSeeder
{
    private const string DefaultEmail = "superadmin@elohim.gt";
    private const string DefaultNombre = "Super Admin";
    private const string DefaultPassword = "SuperAdmin123!";
    private const string AdminRol = "administrador";

    public static async Task SeedAsync(
        ElohimShopDbContext dbContext,
        IConfiguration configuration,
        bool allowDevelopmentDefaults,
        ILogger logger,
        CancellationToken cancellationToken = default)
    {
        var email = (
            Environment.GetEnvironmentVariable("SUPER_ADMIN_EMAIL")
            ?? configuration["SuperAdmin:Email"]
            ?? DefaultEmail
        ).Trim();

        var password = (
            Environment.GetEnvironmentVariable("SUPER_ADMIN_PASSWORD")
            ?? configuration["SuperAdmin:Password"]
        )?.Trim();

        if (string.IsNullOrWhiteSpace(password))
        {
            if (allowDevelopmentDefaults)
            {
                password = DefaultPassword;
            }
            else
            {
                logger.LogInformation(
                    "Super admin no creado: configure SUPER_ADMIN_PASSWORD o SuperAdmin:Password.");
                return;
            }
        }

        var nombre = (
            Environment.GetEnvironmentVariable("SUPER_ADMIN_NOMBRE")
            ?? configuration["SuperAdmin:Nombre"]
            ?? DefaultNombre
        ).Trim();

        var existe = await dbContext.Usuarios
            .AsNoTracking()
            .AnyAsync(u => u.Correo == email, cancellationToken);

        if (existe)
        {
            return;
        }

        var usuario = Usuario.CrearAdministrador(
            email,
            nombre,
            PasswordHashing.Hash(password),
            AdminRol);

        dbContext.Usuarios.Add(usuario);
        await dbContext.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Usuario super admin creado ({Email}).", email);
    }
}
