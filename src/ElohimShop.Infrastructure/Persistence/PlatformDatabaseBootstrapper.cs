using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ElohimShop.Infrastructure.Persistence;

public static class PlatformDatabaseBootstrapper
{
    public static async Task EnsureCreatedAsync(
        PlatformDbContext dbContext,
        ILogger logger,
        CancellationToken cancellationToken = default)
    {
        var created = await dbContext.Database.EnsureCreatedAsync(cancellationToken);
        if (created)
        {
            logger.LogInformation("Base de datos creada desde el modelo EF Core.");
        }
    }
}