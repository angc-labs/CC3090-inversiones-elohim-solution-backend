namespace ElohimShop.Domain.Entities;

public class TipoCliente
{
    public string IdTipo { get; private set; } = Guid.NewGuid().ToString();
    public string Nombre { get; private set; } = string.Empty;
    public string? Descripcion { get; private set; }
    public DateTime FechaCreacion { get; private set; }
    public ICollection<Cliente> Clientes { get; private set; } = new List<Cliente>();
}