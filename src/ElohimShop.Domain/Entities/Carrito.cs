namespace ElohimShop.Domain.Entities;

public class Carrito
{
    private readonly List<ArticuloCarrito> _articulos = new();

    public string IdCarrito { get; private set; } = Guid.NewGuid().ToString();
    public string ClienteId { get; private set; } = string.Empty;
    public bool Activo { get; private set; } = true;
    public DateTime FechaCreacion { get; private set; }
    public DateTime FechaActualizacion { get; private set; }
    public Usuario? Cliente { get; private set; }
    public IReadOnlyCollection<ArticuloCarrito> Articulos => _articulos.AsReadOnly();

    private Carrito() { }

    public static Carrito Crear(string clienteId)
    {
        var ahora = DateTime.UtcNow;
        return new Carrito
        {
            ClienteId = clienteId,
            Activo = true,
            FechaCreacion = ahora,
            FechaActualizacion = ahora
        };
    }

    public void AgregarArticulo(string productoId, string nombreProducto, int cantidad, int precioUnitario)
    {
        var articuloExistente = _articulos.FirstOrDefault(a => a.ProductoId == productoId);
        
        if (articuloExistente is not null)
        {
            articuloExistente.ActualizarCantidad(articuloExistente.Cantidad + cantidad);
        }
        else
        {
            _articulos.Add(ArticuloCarrito.Crear(IdCarrito, productoId, nombreProducto, cantidad, precioUnitario));
        }
        
        FechaActualizacion = DateTime.UtcNow;
    }

    public void ActualizarCantidadArticulo(string articuloId, int cantidad)
    {
        var articulo = _articulos.FirstOrDefault(a => a.IdArticulo == articuloId);
        if (articulo is not null)
        {
            if (cantidad <= 0)
            {
                _articulos.Remove(articulo);
            }
            else
            {
                articulo.ActualizarCantidad(cantidad);
            }
            FechaActualizacion = DateTime.UtcNow;
        }
    }

    public void EliminarArticulo(string articuloId)
    {
        var articulo = _articulos.FirstOrDefault(a => a.IdArticulo == articuloId);
        if (articulo is not null)
        {
            _articulos.Remove(articulo);
            FechaActualizacion = DateTime.UtcNow;
        }
    }

    public void Vaciar()
    {
        _articulos.Clear();
        FechaActualizacion = DateTime.UtcNow;
    }

    public decimal CalcularTotal()
    {
        return _articulos.Sum(a => a.Subtotal);
    }
}