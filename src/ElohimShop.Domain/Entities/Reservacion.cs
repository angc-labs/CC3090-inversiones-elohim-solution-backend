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

    public static Reservacion Crear(string? clienteId, string? metodoPagoId)
    {
        var ahora = DateTime.UtcNow;
        return new Reservacion
        {
            CodigoReservacion = $"RES-{ahora:yyyyMMdd}-{Guid.NewGuid().ToString()[..8].ToUpper()}",
            ClienteId = clienteId,
            EstadoRenovacion = "pendiente",
            MetodoPagoId = metodoPagoId,
            Pagado = false,
            FechaRenovacion = ahora,
            FechaLimiteRetiro = ahora.AddDays(3)
        };
    }

    public void AgregarDetalle(string? productoId, string nombreProducto, int cantidad, decimal precioUnitario)
    {
        _detalles.Add(DetalleReservacion.Crear(IdReservacion, productoId, nombreProducto, cantidad, precioUnitario));
    }

    public void CalcularTotal()
    {
        TotalRenovacion = _detalles.Sum(d => d.Subtotal);
    }
}
