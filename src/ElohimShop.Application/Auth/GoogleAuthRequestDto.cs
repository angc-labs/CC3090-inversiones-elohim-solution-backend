using System.ComponentModel.DataAnnotations;

namespace ElohimShop.Application.Auth;

public sealed class GoogleAuthRequestDto
{
    [Required]
    public string IdToken { get; set; } = string.Empty;

    public string? TiendaId { get; set; }

    [Required]
    [RegularExpression("^(cliente|administrador)$")]
    public string TipoUsuario { get; set; } = string.Empty;
}
