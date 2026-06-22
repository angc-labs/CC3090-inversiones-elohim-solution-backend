namespace ElohimShop.Application.Pagos;

public class CrearPaymentIntentDto
{
    public string ReservacionId { get; set; } = string.Empty;
    public string? MetodoPagoId { get; set; }
}

public class PaymentIntentCreadoDto
{
    public string ClientSecret { get; set; } = string.Empty;
    public string ReservacionId { get; set; } = string.Empty;
    public long MontoCentavos { get; set; }
    public string Moneda { get; set; } = string.Empty;
}

public class PagoEstadoDto
{
    public string PaymentIntentId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string ReservacionId { get; set; } = string.Empty;
    public long MontoCentavos { get; set; }
    public string Moneda { get; set; } = string.Empty;
}

public class ReembolsoRequestDto
{
    public long? MontoCentavos { get; set; }
    public string? Razon { get; set; }
}

public class ReembolsoCreadoDto
{
    public string ReembolsoId { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;
    public long MontoCentavos { get; set; }
    public string Moneda { get; set; } = string.Empty;
}
