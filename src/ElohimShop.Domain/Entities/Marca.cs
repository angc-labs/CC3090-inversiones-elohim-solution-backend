namespace ElohimShop.Domain.Entities;

public class Marca
{
    public string Id { get; private set; } = Guid.NewGuid().ToString();
    public string NombreMarca { get; private set; } = string.Empty;
    public string? Descripcion { get; private set; }
    public ICollection<Producto> Productos { get; private set; } = new List<Producto>();
}