namespace ElohimShop.Domain.Entities;

public class DetalleReservacion
{
    public string IdDetails { get; private set; } = Guid.NewGuid().ToString();
    public string? ReservacionId { get; private set; }
    public string? ProductoId { get; private set; }
    public string NombreProducto { get; private set; } = string.Empty;
    public int Cantidad { get; private set; }
    public decimal PrecioUnitario { get; private set; }
    public decimal Subtotal { get; private set; }
    public Reservacion? Reservacion { get; private set; }
    public Producto? Producto { get; private set; }

    private DetalleReservacion() { }

    public static DetalleReservacion Crear(string reservacionId, string? productoId, string nombreProducto, int cantidad, decimal precioUnitario)
    {
        return new DetalleReservacion
        {
            ReservacionId = reservacionId,
            ProductoId = productoId,
            NombreProducto = nombreProducto,
            Cantidad = cantidad,
            PrecioUnitario = precioUnitario,
            Subtotal = cantidad * precioUnitario
        };
    }
}