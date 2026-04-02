namespace ElohimShop.Domain.Entities;

public class Producto
{
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
}