namespace ElohimShop.Application.Admin;

public class UsuarioAdminDto
{
    public string Id { get; init; } = string.Empty;
    public string Nombre { get; init; } = string.Empty;
    public string? Apellido { get; init; }
    public string Correo { get; init; } = string.Empty;
    public string? Telefono { get; init; }
    public string TipoUsuario { get; init; } = string.Empty;
    public string? Rol { get; init; }
    public bool Estado { get; init; }
    public DateTime FechaCreacion { get; init; }
}
