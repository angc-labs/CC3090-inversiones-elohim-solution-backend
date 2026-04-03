namespace ElohimShop.Application.Auth;

public interface IClientAuthService
{
    Task<AuthResponseDto> RegisterAsync(RegisterClientRequestDto request, CancellationToken cancellationToken);

    Task LogoutAsync(string jti, string clienteId, DateTime expiresAt, CancellationToken cancellationToken);
}