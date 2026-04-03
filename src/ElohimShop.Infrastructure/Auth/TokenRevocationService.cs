using ElohimShop.Application.Auth;
using ElohimShop.Domain.Entities;
using ElohimShop.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ElohimShop.Infrastructure.Auth;

public class TokenRevocationService : ITokenRevocationService
{
    private readonly ElohimShopDbContext _dbContext;

    public TokenRevocationService(ElohimShopDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<bool> IsTokenRevokedAsync(string jti, CancellationToken cancellationToken)
    {
        return _dbContext.TokensRevocados.AnyAsync(token => token.Jti == jti, cancellationToken);
    }

    public async Task RevokeTokenAsync(string jti, string clienteId, DateTime expiresAt, CancellationToken cancellationToken)
    {
        var alreadyRevoked = await _dbContext.TokensRevocados.AnyAsync(token => token.Jti == jti, cancellationToken);

        if (alreadyRevoked)
        {
            return;
        }

        var tokenRevocado = TokenRevocado.Crear(jti, clienteId, expiresAt);

        _dbContext.TokensRevocados.Add(tokenRevocado);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}