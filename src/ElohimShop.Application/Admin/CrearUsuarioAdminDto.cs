using System.ComponentModel.DataAnnotations;

namespace ElohimShop.Application.Admin;

public class CrearUsuarioAdminDto
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
    public string? Rol { get; set; }
}
