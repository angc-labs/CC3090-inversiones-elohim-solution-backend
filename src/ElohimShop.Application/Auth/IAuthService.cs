namespace ElohimShop.Application.Auth;

public interface IAuthService
{
    Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request, CancellationToken cancellationToken);
    Task<AuthResponseDto> RegisterAdminAsync(RegisterRequestDto request, CancellationToken cancellationToken);
    Task<AuthResponseDto> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken);
    Task LogoutAsync(string jti, string usuarioId, DateTime expiresAt, CancellationToken cancellationToken);
    Task ForgotPasswordAsync(ForgotPasswordRequestDto request, CancellationToken cancellationToken);
}
