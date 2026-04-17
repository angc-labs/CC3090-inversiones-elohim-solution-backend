using System.ComponentModel.DataAnnotations;

namespace ElohimShop.Application.Auth;

public sealed class RegisterRequestDto
{
    [Required, EmailAddress]
    public string Correo { get; set; } = string.Empty;

    [Required]
    public string Nombre { get; set; } = string.Empty;

    [Required]
    public string Contrasena { get; set; } = string.Empty;

    [Required]
    public string TipoUsuario { get; set; } = string.Empty;

    public string? Apellido { get; set; }
    public string? Telefono { get; set; }
    public string? TipoCliente { get; set; }
    public string? Rol { get; set; }
    public string? Direccion { get; set; }
}
