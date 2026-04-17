using System.ComponentModel.DataAnnotations;

namespace ElohimShop.Application.Auth;

public sealed record LoginRequestDto(
    [param: Required, EmailAddress] string Correo,
    [param: Required] string Contrasena
);
