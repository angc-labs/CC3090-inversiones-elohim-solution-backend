namespace ElohimShop.Application.Pagos;

public class MetodoPagoGuardadoDto
{
    public string IdMetodoPago { get; set; } = string.Empty;
    public string NombreMetodo { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public bool Activo { get; set; }
    public string? StripePaymentMethodId { get; set; }
    public string? UltimosDigitos { get; set; }
    public string? MarcaTarjeta { get; set; }
    public int? ExpiraMes { get; set; }
    public int? ExpiraAnio { get; set; }
    public string? Alias { get; set; }
}

public class GuardarMetodoPagoDto
{
    public string NombreMetodo { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public string StripePaymentMethodId { get; set; } = string.Empty;
    public string? UltimosDigitos { get; set; }
    public string? MarcaTarjeta { get; set; }
    public int? ExpiraMes { get; set; }
    public int? ExpiraAnio { get; set; }
    public string? Alias { get; set; }
}