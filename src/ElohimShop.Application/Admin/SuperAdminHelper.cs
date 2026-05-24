namespace ElohimShop.Application.Admin;

public static class SuperAdminHelper
{
    private const string DefaultEmail = "superadmin@elohim.gt";

    public static string ResolveEmail(string? configEmail = null)
    {
        return (
            Environment.GetEnvironmentVariable("SUPER_ADMIN_EMAIL")
            ?? configEmail
            ?? DefaultEmail
        ).Trim();
    }

    public static bool IsSuperAdminEmail(string? email, string? configEmail = null)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return false;
        }

        return string.Equals(
            email.Trim(),
            ResolveEmail(configEmail),
            StringComparison.OrdinalIgnoreCase);
    }
}
