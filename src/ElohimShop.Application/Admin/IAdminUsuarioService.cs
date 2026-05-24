namespace ElohimShop.Application.Admin;

public interface IAdminUsuarioService
{
    Task<IEnumerable<UsuarioAdminDto>> ObtenerTodosAsync(
        string? busqueda,
        string? tipoUsuario,
        bool? estado,
        CancellationToken cancellationToken);

    Task<UsuarioAdminDto> ObtenerPorIdAsync(
        string id,
        CancellationToken cancellationToken);

    Task<UsuarioAdminDto> CambiarEstadoAsync(
        string usuarioId,
        bool nuevoEstado,
        CancellationToken cancellationToken);

    Task<UsuarioAdminDto> CrearAsync(
        CrearUsuarioAdminDto dto,
        CancellationToken cancellationToken);

    Task<UsuarioAdminDto> ActualizarAsync(
        string id,
        ActualizarUsuarioAdminDto dto,
        CancellationToken cancellationToken);

    Task EliminarAsync(
        string id,
        CancellationToken cancellationToken);
}
