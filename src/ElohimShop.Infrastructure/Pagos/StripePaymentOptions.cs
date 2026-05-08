namespace ElohimShop.Infrastructure.Pagos;

public class StripePaymentOptions
{
    public const string SectionName = "Stripe";

    public string? SecretKey { get; set; }
    public string? PublishableKey { get; set; }
    public string? WebhookSecret { get; set; }
    public string? DefaultCurrency { get; set; } = "gtq";
}