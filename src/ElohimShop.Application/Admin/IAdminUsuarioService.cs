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

    Task<UsuarioAdminDto> CrearAsync(
        CrearUsuarioAdminRequestDto request,
        CancellationToken cancellationToken);

    Task<UsuarioAdminDto> CambiarRolAsync(
        string usuarioId,
        CambiarRolUsuarioRequestDto request,
        CancellationToken cancellationToken);
}
