namespace ElohimShop.Application.Auth;

public sealed record AuthResponseDto(
    string ClienteId,
    string Correo,
    string Nombre,
    string Token,
    DateTime ExpiraEn
);