namespace ElohimShop.Application.Pagos;

public interface IStripeWebhookHandler
{
    Task ProcesarEventoRawAsync(string json, string stripeSignatureHeader, CancellationToken ct = default);
}
