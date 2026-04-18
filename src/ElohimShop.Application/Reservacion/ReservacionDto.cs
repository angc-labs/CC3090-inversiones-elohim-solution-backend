namespace ElohimShop.Application.Reservacion;

public class CrearReservacionDto
{
    public string MetodoPagoId { get; set; } = string.Empty;
}

public class ReservacionDto
{
    public string IdReservacion { get; set; } = string.Empty;
    public string CodigoReservacion { get; set; } = string.Empty;
    public string ClienteId { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;
    public decimal TotalReservacion { get; set; }
    public string? MetodoPagoId { get; set; }
    public bool Pagado { get; set; }
    public string? Observaciones { get; set; }
    public DateTime FechaLimiteRetiro { get; set; }
    public List<DetalleReservacionDto> Items { get; set; } = new();
}

public class DetalleReservacionDto
{
    public string ProductoId { get; set; } = string.Empty;
    public string NombreProducto { get; set; } = string.Empty;
    public int Cantidad { get; set; }
    public int PrecioUnitario { get; set; }
    public decimal Subtotal { get; set; }
}

public class ReservacionListadoDto
{
    public string IdReservacion { get; set; } = string.Empty;
    public string CodigoReservacion { get; set; } = string.Empty;
    public string ClienteId { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;
    public decimal TotalReservacion { get; set; }
    public bool Pagado { get; set; }
    public DateTime FechaLimiteRetiro { get; set; }
}