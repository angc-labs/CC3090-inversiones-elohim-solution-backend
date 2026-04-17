namespace ElohimShop.Domain.Entities;

public class ClientePerfil
{
    public string UsuarioId { get; set; } = string.Empty;
    public string? Direccion { get; set; }
    public string TipoCliente { get; set; } = string.Empty;
    public Usuario? Usuario { get; set; }
}
