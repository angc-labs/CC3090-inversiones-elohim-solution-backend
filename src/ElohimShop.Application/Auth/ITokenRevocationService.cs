namespace ElohimShop.Application.Auth;

public interface ITokenRevocationService
{
    Task<bool> IsTokenRevokedAsync(string jti, CancellationToken cancellationToken);

    Task RevokeTokenAsync(string jti, string clienteId, DateTime expiresAt, CancellationToken cancellationToken);
}