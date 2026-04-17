using System.ComponentModel.DataAnnotations;

namespace ElohimShop.Application.Usuario;

public class ActualizarPerfilDto
{
    public string? Nombre { get; init; }
    public string? Apellido { get; init; }

    [EmailAddress]
    public string? Correo { get; init; }

    public string? Contrasena { get; init; }
    public string? Telefono { get; init; }
    public string? Direccion { get; init; }
}
