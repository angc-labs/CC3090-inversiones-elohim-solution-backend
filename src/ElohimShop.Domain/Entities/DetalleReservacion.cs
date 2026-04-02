namespace ElohimShop.Domain.Entities;

public class DetalleReservacion
{
    public string IdDetails { get; private set; } = Guid.NewGuid().ToString();
    public string? ReservacionId { get; private set; }
    public string? ProductoId { get; private set; }
    public int Cantidad { get; private set; }
    public decimal PrecioUnitario { get; private set; }
    public decimal Subtotal { get; private set; }
    public Reservacion? Reservacion { get; private set; }
    public Producto? Producto { get; private set; }
}