namespace ElohimShop.Application.Auth;

public sealed record AuthResponseDto(
    string UsuarioId,
    string Correo,
    string Nombre,
    string TipoUsuario,
    string? Rol,
    string? TipoCliente,
    string Token,
    DateTime ExpiraEn
);
