namespace ElohimShop.Domain.Entities;

public class AdministradorPerfil
{
    public string UsuarioId { get; set; } = string.Empty;
    public string Rol { get; set; } = string.Empty;
    public Usuario? Usuario { get; set; }
}
