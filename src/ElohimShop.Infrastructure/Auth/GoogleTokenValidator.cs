using ElohimShop.Application.Auth;
using Google.Apis.Auth;
using Microsoft.Extensions.Configuration;

namespace ElohimShop.Infrastructure.Auth;

public sealed class GoogleTokenValidator : IGoogleTokenValidator
{
    private readonly IConfiguration _configuration;

    public GoogleTokenValidator(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task<GoogleTokenPayload> ValidateAsync(string idToken, CancellationToken cancellationToken)
    {
        var clientId = Environment.GetEnvironmentVariable("GOOGLE_CLIENT_ID")
            ?? _configuration["GOOGLE_CLIENT_ID"]
            ?? _configuration["Google:ClientId"];

        if (string.IsNullOrWhiteSpace(clientId))
        {
            throw new InvalidOperationException("Google OAuth no está configurado en el servidor.");
        }

        try
        {
            var settings = new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = new[] { clientId }
            };
            var payload = await GoogleJsonWebSignature.ValidateAsync(idToken, settings)
                .WaitAsync(cancellationToken);

            return new GoogleTokenPayload(
                payload.Subject,
                payload.Email,
                payload.Name ?? payload.Email,
                payload.EmailVerified,
                payload.Picture);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex) when (ex is InvalidJwtException or ArgumentException)
        {
            throw new UnauthorizedAccessException("El token de Google es inválido o expiró.", ex);
        }
    }
}
