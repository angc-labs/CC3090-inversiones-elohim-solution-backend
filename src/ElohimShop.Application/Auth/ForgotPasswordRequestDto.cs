using System.ComponentModel.DataAnnotations;

namespace ElohimShop.Application.Auth;

public sealed record ForgotPasswordRequestDto(
    [param: Required, EmailAddress] string Correo
);
