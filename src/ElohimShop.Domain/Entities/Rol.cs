namespace ElohimShop.Domain.Entities;

public class Rol
{
    public string Id { get; private set; } = Guid.NewGuid().ToString();
    public string Nombre { get; private set; } = string.Empty;
    public string? Descripcion { get; private set; }
    public DateTime? FechaCreacion { get; private set; }
    public ICollection<Administrador> Administradores { get; private set; } = new List<Administrador>();
}