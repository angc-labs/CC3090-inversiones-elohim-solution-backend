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
        logger.LogInformation("Iniciando verificación y creación del esquema de base de datos vía EF Core...");

        try
        {
            var script = dbContext.Database.GenerateCreateScript();
            
            // Separar el script en sentencias individuales usando punto y coma seguido de salto de línea
            var statements = script.Split(new[] { ";\r\n", ";\n" }, StringSplitOptions.RemoveEmptyEntries);

            int executedCount = 0;
            int skippedCount = 0;

            foreach (var statement in statements)
            {
                var trimmed = statement.Trim();
                if (string.IsNullOrWhiteSpace(trimmed)) continue;

                try
                {
                    await dbContext.Database.ExecuteSqlRawAsync(trimmed, cancellationToken);
                    executedCount++;
                }
                catch (Exception ex)
                {
                    // Ignorar errores si la tabla, índice o constraint ya existe en PostgreSQL
                    var msg = ex.Message.ToLowerInvariant();
                    if (msg.Contains("already exists") || 
                        msg.Contains("ya existe") || 
                        msg.Contains("duplicate"))
                    {
                        skippedCount++;
                    }
                    else
                    {
                        logger.LogWarning(ex, "Error al ejecutar sentencia de esquema: {Sql}", trimmed);
                    }
                }
            }

            logger.LogInformation("Esquema de base de datos procesado. Ejecutados: {Executed}, Omitidos/Existentes: {Skipped}", executedCount, skippedCount);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error crítico durante la inicialización del esquema de la base de datos.");
        }
    }
}
