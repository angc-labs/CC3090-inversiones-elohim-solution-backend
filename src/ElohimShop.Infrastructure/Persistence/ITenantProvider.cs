namespace ElohimShop.Infrastructure.Persistence;

public interface ITenantProvider
{
    string GetTenantId();
}