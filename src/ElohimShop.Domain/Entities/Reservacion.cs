using ElohimShop.Domain.Enums;

namespace ElohimShop.Domain.Entities;

public class Reservacion
{
    public string IdReservacion { get; private set; } = Guid.NewGuid().ToString();
    public string CodigoReservacion { get; private set; } = string.Empty;
    public string? ClienteId { get; private set; }
    public DateTime FechaRenovacion { get; private set; }
    public EstadoRenovacion? EstadoRenovacion { get; private set; }
    public decimal? TotalRenovacion { get; private set; }
    public string? MetodoPagoId { get; private set; }
    public bool Pagado { get; private set; }
    public string? Observaciones { get; private set; }
    public DateTime FechaLimiteRetiro { get; private set; }
    public Cliente? Cliente { get; private set; }
    public MetodoPago? MetodoPago { get; private set; }
    public ICollection<DetalleReservacion> Detalles { get; private set; } = new List<DetalleReservacion>();
    public Venta? Venta { get; private set; }
}