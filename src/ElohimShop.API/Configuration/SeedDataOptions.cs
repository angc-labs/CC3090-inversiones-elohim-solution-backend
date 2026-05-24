namespace ElohimShop.API.Configuration;

/// <summary>
/// Lee SEED_DATA (o SEED_DEMO_DATA por compatibilidad) desde configuración o variables de entorno.
/// </summary>
public static class SeedDataOptions
{
    public static bool IsEnabled(IConfiguration configuration)
    {
        if (IsTruthy(configuration["SEED_DATA"]))
        {
            return true;
        }

        if (IsTruthy(Environment.GetEnvironmentVariable("SEED_DATA")))
        {
            return true;
        }

        // Compatibilidad con nombre anterior
        if (IsTruthy(configuration["SEED_DEMO_DATA"]))
        {
            return true;
        }

        return IsTruthy(Environment.GetEnvironmentVariable("SEED_DEMO_DATA"));
    }

    private static bool IsTruthy(string? value) =>
        string.Equals(value, "true", StringComparison.OrdinalIgnoreCase)
        || string.Equals(value, "1", StringComparison.Ordinal);
}
