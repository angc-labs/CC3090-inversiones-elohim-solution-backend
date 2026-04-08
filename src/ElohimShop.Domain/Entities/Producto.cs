namespace ElohimShop.Domain.Entities;

public class Producto
{
    private Producto()
    {
    }

    public string IdProducto { get; private set; } = Guid.NewGuid().ToString();
    public string CodigoProducto { get; private set; } = string.Empty;
    public string NombreProducto { get; private set; } = string.Empty;
    public string? Descripcion { get; private set; }
    public int Precio { get; private set; }
    public int StockActual { get; private set; }
    public string? IdMarca { get; private set; }
    public string? CategoriaId { get; private set; }
    public DateTime FechaVencimiento { get; private set; }
    public string? ImagenPrincipal { get; private set; }
    public DateTime FechaCreacion { get; private set; }
    public DateTime FechaActualizacion { get; private set; }
    public Marca? Marca { get; private set; }
    public Categoria? Categoria { get; private set; }
    public ICollection<DetalleReservacion> DetallesReservacion { get; private set; } = new List<DetalleReservacion>();

    public static Producto Crear(
        string codigoProducto,
        string nombreProducto,
        int precio,
        int stockActual,
        string? descripcion = null,
        string? idMarca = null,
        string? categoriaId = null,
        DateTime? fechaVencimiento = null,
        string? imagenPrincipal = null)
    {
        var ahora = DateTime.UtcNow;

        return new Producto
        {
            CodigoProducto = codigoProducto.Trim(),
            NombreProducto = nombreProducto.Trim(),
            Precio = precio,
            StockActual = stockActual,
            Descripcion = descripcion?.Trim(),
            IdMarca = idMarca,
            CategoriaId = categoriaId,
            FechaVencimiento = fechaVencimiento ?? ahora.AddYears(1),
            ImagenPrincipal = imagenPrincipal?.Trim(),
            FechaCreacion = ahora,
            FechaActualizacion = ahora
        };
    }
}