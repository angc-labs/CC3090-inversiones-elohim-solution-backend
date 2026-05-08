using ElohimShop.Application.Pagos;
using ElohimShop.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Stripe;

namespace ElohimShop.Infrastructure.Pagos;

public class StripeWebhookHandler : IStripeWebhookHandler
{
    private readonly ElohimShopDbContext _db;
    private readonly StripePaymentOptions _options;

    public StripeWebhookHandler(ElohimShopDbContext db, IOptions<StripePaymentOptions> options)
    {
        _db = db;
        _options = options.Value;
    }

    public async Task ProcesarEventoRawAsync(string json, string stripeSignatureHeader, CancellationToken ct = default)
    {
        var whSecret = Environment.GetEnvironmentVariable("STRIPE_WEBHOOK_SECRET")?.Trim()
            ?? _options.WebhookSecret?.Trim();

        if (string.IsNullOrEmpty(whSecret))
        {
            throw new InvalidOperationException("STRIPE_WEBHOOK_SECRET no configurado.");
        }

        var secretKey = Environment.GetEnvironmentVariable("STRIPE_SECRET_KEY")?.Trim()
            ?? _options.SecretKey?.Trim();

        if (string.IsNullOrEmpty(secretKey))
        {
            throw new InvalidOperationException("STRIPE_SECRET_KEY no configurado.");
        }

        StripeConfiguration.ApiKey = secretKey;

        Event stripeEvent = EventUtility.ConstructEvent(json, stripeSignatureHeader, whSecret);

        switch (stripeEvent.Type)
        {
            case EventTypes.PaymentIntentSucceeded:
                await ManejarPagoExitosoAsync(stripeEvent, ct).ConfigureAwait(false);
                break;
            case EventTypes.PaymentIntentPaymentFailed:
                await ManejarPagoFallidoAsync(stripeEvent, ct).ConfigureAwait(false);
                break;
            case EventTypes.ChargeRefunded:
                await ManejarCargoReembolsadoAsync(stripeEvent, ct).ConfigureAwait(false);
                break;
        }
    }

    private async Task ManejarPagoExitosoAsync(Event stripeEvent, CancellationToken ct)
    {
        if (stripeEvent.Data.Object is not PaymentIntent pi)
        {
            return;
        }

        if (!pi.Metadata.TryGetValue("reservacion_id", out var rid))
        {
            return;
        }

        var reservacion = await _db.Reservaciones
            .FirstOrDefaultAsync(r => r.IdReservacion == rid, ct)
            .ConfigureAwait(false);

        if (reservacion is null || reservacion.Pagado)
        {
            return;
        }

        reservacion.MarcarComoPagada();
        await _db.SaveChangesAsync(ct).ConfigureAwait(false);
    }

    private async Task ManejarPagoFallidoAsync(Event stripeEvent, CancellationToken ct)
    {
        if (stripeEvent.Data.Object is not PaymentIntent pi)
        {
            return;
        }

        if (!pi.Metadata.TryGetValue("reservacion_id", out var rid))
        {
            return;
        }

        var reservacion = await _db.Reservaciones
            .FirstOrDefaultAsync(r => r.IdReservacion == rid, ct)
            .ConfigureAwait(false);

        if (reservacion is null)
        {
            return;
        }

        var detalle = pi.LastPaymentError?.Message ?? "sin detalle";
        reservacion.AnexarObservacion($"Pago Stripe fallido: {detalle}");
        await _db.SaveChangesAsync(ct).ConfigureAwait(false);
    }

    private async Task ManejarCargoReembolsadoAsync(Event stripeEvent, CancellationToken ct)
    {
        if (stripeEvent.Data.Object is not Charge charge || string.IsNullOrEmpty(charge.PaymentIntentId))
        {
            return;
        }

        var reservacion = await _db.Reservaciones
            .FirstOrDefaultAsync(r => r.StripePaymentIntentId == charge.PaymentIntentId, ct)
            .ConfigureAwait(false);

        if (reservacion is null)
        {
            return;
        }

        reservacion.AnexarObservacion($"Reembolso Stripe registrado (cargo {charge.Id}).");
        await _db.SaveChangesAsync(ct).ConfigureAwait(false);
    }
}
