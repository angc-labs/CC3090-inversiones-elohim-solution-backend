namespace ElohimShop.Application.Admin;

/// <summary>
/// Rol destino: cliente | cajero | administrador (solo super admin).
/// </summary>
public sealed class CambiarRolUsuarioRequestDto
{
    public string Rol { get; init; } = string.Empty;
    public string? TipoCliente { get; init; }
    public string? Direccion { get; init; }
}
