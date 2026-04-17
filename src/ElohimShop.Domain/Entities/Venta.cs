namespace ElohimShop.Domain.Entities;

public class Venta
{
    public string IdVenta { get; private set; } = Guid.NewGuid().ToString();
    public string? ReservacionId { get; private set; }
    public decimal MontoTotal { get; private set; }
    public string? UsuarioCajeroId { get; private set; }
    public DateTime FechaVenta { get; private set; }
    public string TipoComprobante { get; private set; } = string.Empty;
    public string EstadoVenta { get; private set; } = string.Empty;
    public Reservacion? Reservacion { get; private set; }
    public Usuario? UsuarioCajero { get; private set; }
}
