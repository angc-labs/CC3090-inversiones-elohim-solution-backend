namespace ElohimShop.Application.Admin;

public interface IAdminUsuarioService
{
    Task<IEnumerable<UsuarioAdminDto>> ObtenerTodosAsync(
        string? busqueda,
        string? tipoUsuario,
        bool? estado,
        CancellationToken cancellationToken);

    Task<UsuarioAdminDto> CambiarEstadoAsync(
        string usuarioId,
        bool nuevoEstado,
        CancellationToken cancellationToken);
}
