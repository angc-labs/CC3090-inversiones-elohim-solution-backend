namespace ElohimShop.Domain.Entities;

public class Reservacion
{
    private readonly List<DetalleReservacion> _detalles = new();

    public string IdReservacion { get; private set; } = Guid.NewGuid().ToString();
    public string CodigoReservacion { get; private set; } = string.Empty;
    public string? ClienteId { get; private set; }
    public DateTime FechaRenovacion { get; private set; }
    public string EstadoRenovacion { get; private set; } = "pendiente";
    public decimal? TotalRenovacion { get; private set; }
    public string? MetodoPagoId { get; private set; }
    public bool Pagado { get; private set; }
    public string? Observaciones { get; private set; }
    public DateTime FechaLimiteRetiro { get; private set; }
    public Usuario? Cliente { get; private set; }
    public MetodoPago? MetodoPago { get; private set; }
    public Venta? Venta { get; private set; }

    public IReadOnlyCollection<DetalleReservacion> Detalles => _detalles.AsReadOnly();

    private Reservacion() { }
}
