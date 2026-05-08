namespace ElohimShop.Application.Pagos;

public class ConfigStripeClienteDto
{
    public string PublishableKey { get; set; } = string.Empty;
    public string DefaultCurrency { get; set; } = string.Empty;
}
