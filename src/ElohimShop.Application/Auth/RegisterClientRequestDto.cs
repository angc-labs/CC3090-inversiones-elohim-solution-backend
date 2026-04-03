namespace ElohimShop.Application.Auth;

public sealed record RegisterClientRequestDto(
    string Correo,
    string Nombre,
    string Contrasena,
    string? Apellido = null,
    string? Telefono = null,
    string? Direccion = null
);