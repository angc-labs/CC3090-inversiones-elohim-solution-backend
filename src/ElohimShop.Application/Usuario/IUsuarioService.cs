namespace ElohimShop.Application.Usuario;

public interface IUsuarioService
{
    Task<UsuarioPerfilDto?> ObtenerPerfilAsync(string usuarioId, CancellationToken cancellationToken);
    Task<PerfilActualizadoDto> ActualizarPerfilAsync(string usuarioId, ActualizarPerfilDto dto, CancellationToken cancellationToken);
}
