using System.Net.Http.Json;
using System.Text.Json.Serialization;
using ElohimShop.Application.Auth;
using Google.Apis.Auth;
using Microsoft.Extensions.Configuration;

namespace ElohimShop.Infrastructure.Auth;

public sealed class GoogleTokenValidator : IGoogleTokenValidator
{
    private readonly IConfiguration _configuration;
    private readonly HttpClient _httpClient;

    public GoogleTokenValidator(IConfiguration configuration, HttpClient? httpClient = null)
    {
        _configuration = configuration;
        _httpClient = httpClient ?? new HttpClient();
    }

    public async Task<GoogleTokenPayload> ValidateAsync(string token, CancellationToken cancellationToken)
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
            var payload = await GoogleJsonWebSignature.ValidateAsync(token, settings)
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
            // Fallback: Check if token is a Google OAuth2 access token
            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Get, "https://www.googleapis.com/oauth2/v3/userinfo");
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                
                var response = await _httpClient.SendAsync(request, cancellationToken);
                if (response.IsSuccessStatusCode)
                {
                    var userInfo = await response.Content.ReadFromJsonAsync<GoogleUserInfoResponse>(cancellationToken: cancellationToken);
                    if (userInfo != null && !string.IsNullOrWhiteSpace(userInfo.Email))
                    {
                        return new GoogleTokenPayload(
                            userInfo.Sub,
                            userInfo.Email,
                            userInfo.Name ?? userInfo.Email,
                            userInfo.EmailVerified,
                            userInfo.Picture);
                    }
                }
            }
            catch
            {
                // Ignore fallback error and throw original exception below
            }

            throw new UnauthorizedAccessException("El token de Google es inválido o expiró.", ex);
        }
    }

    private sealed record GoogleUserInfoResponse(
        [property: JsonPropertyName("sub")] string Sub,
        [property: JsonPropertyName("email")] string Email,
        [property: JsonPropertyName("name")] string Name,
        [property: JsonPropertyName("email_verified")] bool EmailVerified,
        [property: JsonPropertyName("picture")] string Picture
    );
}

