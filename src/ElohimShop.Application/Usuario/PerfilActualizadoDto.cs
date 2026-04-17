namespace ElohimShop.Application.Usuario;

public class PerfilActualizadoDto
{
    public string UsuarioId { get; init; } = string.Empty;
    public string Nombre { get; init; } = string.Empty;
    public string? Apellido { get; init; }
    public string Correo { get; init; } = string.Empty;
    public string? Telefono { get; init; }
}
