namespace ElohimShop.Domain.Entities;

public class ArticuloCarrito
{
    private ArticuloCarrito()
    {
    }

    public string IdArticulo { get; private set; } = Guid.NewGuid().ToString();
    public string CarritoId { get; private set; } = string.Empty;
    public string ProductoId { get; private set; } = string.Empty;
    public string NombreProducto { get; private set; } = string.Empty;
    public int Cantidad { get; private set; }
    public int PrecioUnitario { get; private set; }
    public decimal Subtotal { get; private set; }
    public Carrito? Carrito { get; private set; }
    public Producto? Producto { get; private set; }

    private ArticuloCarrito(string carritoId, string productoId, string nombreProducto, int cantidad, int precioUnitario)
    {
        IdArticulo = Guid.NewGuid().ToString();
        CarritoId = carritoId;
        ProductoId = productoId;
        NombreProducto = nombreProducto;
        Cantidad = cantidad;
        PrecioUnitario = precioUnitario;
        Subtotal = cantidad * precioUnitario;
    }

    public static ArticuloCarrito Crear(string carritoId, string productoId, string nombreProducto, int cantidad, int precioUnitario)
    {
        return new ArticuloCarrito(carritoId, productoId, nombreProducto, cantidad, precioUnitario);
    }

    public void ActualizarCantidad(int nuevaCantidad)
    {
        Cantidad = nuevaCantidad;
        Subtotal = nuevaCantidad * PrecioUnitario;
    }
}