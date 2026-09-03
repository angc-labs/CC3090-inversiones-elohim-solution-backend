namespace ElohimShop.Application.Auth;

public sealed record GoogleTokenPayload(
    string Subject,
    string Email,
    string Name,
    bool EmailVerified,
    string? Picture);

public interface IGoogleTokenValidator
{
    Task<GoogleTokenPayload> ValidateAsync(string idToken, CancellationToken cancellationToken);
}
