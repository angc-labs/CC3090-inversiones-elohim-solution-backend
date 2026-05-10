namespace ElohimShop.Domain.Entities;

public class Categoria
{
    private Categoria()
    {
    }

    public string Id { get; private set; } = Guid.NewGuid().ToString();
    public string NombreCategoria { get; private set; } = string.Empty;
    public string? Descripcion { get; private set; }
    public DateTime? FechaCreacion { get; private set; }
    public ICollection<Producto> Productos { get; private set; } = new List<Producto>();

    public static Categoria Crear(string nombreCategoria, string? descripcion = null, DateTime? fechaCreacion = null)
    {
        return new Categoria
        {
            NombreCategoria = nombreCategoria.Trim(),
            Descripcion = descripcion?.Trim(),
            FechaCreacion = fechaCreacion ?? DateTime.UtcNow
        };
    }
}