using System.ComponentModel.DataAnnotations;

namespace ElohimShop.Application.Auth;

public sealed record RegisterClientRequestDto(
    [param: Required, EmailAddress] string Correo,
    [param: Required] string Nombre,
    [param: Required] string Contrasena,
    string? Apellido = null,
    string? Telefono = null,
    string? Direccion = null
);