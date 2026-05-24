using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ElohimShop.Infrastructure.Persistence;

public static class DatabaseSchemaBootstrapper
{
    public static async Task EnsureSchemaAsync(
        ElohimShopDbContext dbContext,
        ILogger logger,
        CancellationToken cancellationToken = default)
    {
        var connection = dbContext.Database.GetDbConnection();
        await connection.OpenAsync(cancellationToken);

        try
        {
            await using var command = connection.CreateCommand();

            command.CommandText = """
                SELECT COUNT(*)::int
                FROM information_schema.tables
                WHERE table_schema = 'public'
                  AND table_name = 'Usuario';
                """;

            var tableCount = Convert.ToInt32(await command.ExecuteScalarAsync(cancellationToken));

            if (tableCount == 0)
            {
                var schemaPath = ResolveSqlPath("elohim_db.sql");
                if (schemaPath is null)
                {
                    logger.LogWarning("No se encontró db/elohim_db.sql para inicializar el esquema.");
                    return;
                }

                logger.LogInformation("Aplicando esquema inicial desde {Path}", schemaPath);
                await ExecuteSqlFileAsync(connection, schemaPath, cancellationToken);
            }
        }
        finally
        {
            await connection.CloseAsync();
        }
    }

    private static string? ResolveSqlPath(string relativePath)
    {
        var candidates = new[]
        {
            Path.Combine("/db", relativePath),
            Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "db", relativePath),
            Path.Combine(Directory.GetCurrentDirectory(), "db", relativePath),
            Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "db", relativePath))
        };

        return candidates.FirstOrDefault(File.Exists);
    }

    private static async Task ExecuteSqlFileAsync(
        System.Data.Common.DbConnection connection,
        string filePath,
        CancellationToken cancellationToken)
    {
        var sql = await File.ReadAllTextAsync(filePath, cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        await command.ExecuteNonQueryAsync(cancellationToken);
    }
}
