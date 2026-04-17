namespace ElohimShop.Application.Usuario;

public class UsuarioPerfilDto
{
    public string UsuarioId { get; init; } = string.Empty;
    public string Nombre { get; init; } = string.Empty;
    public string? Apellido { get; init; }
    public string Correo { get; init; } = string.Empty;
    public string? Telefono { get; init; }
    public string TipoUsuario { get; init; } = string.Empty;
    public string? TipoCliente { get; init; }
    public string? Direccion { get; init; }
    public string? Rol { get; init; }
    public DateTime FechaRegistro { get; init; }
}
