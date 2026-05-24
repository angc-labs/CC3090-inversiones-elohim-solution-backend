namespace ElohimShop.Application.Admin;

public sealed class CrearUsuarioAdminRequestDto
{
    public string Correo { get; init; } = string.Empty;
    public string Nombre { get; init; } = string.Empty;
    public string? Apellido { get; init; }
    public string? Telefono { get; init; }
    public string Contrasena { get; init; } = string.Empty;
    /// <summary>cliente | administrador (empleados usan administrador + rol cajero)</summary>
    public string TipoUsuario { get; init; } = string.Empty;
    public string? Rol { get; init; }
    public string? TipoCliente { get; init; }
    public string? Direccion { get; init; }
}
