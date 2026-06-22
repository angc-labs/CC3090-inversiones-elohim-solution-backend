using ElohimShop.Application.Pagos;
using ElohimShop.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Stripe;

namespace ElohimShop.Infrastructure.Pagos;

public class StripeWebhookHandler : IStripeWebhookHandler
{
    private readonly ElohimShopDbContext _db;
    private readonly PlatformDbContext _platformDb;
    private readonly StripePaymentOptions _options;

    public StripeWebhookHandler(
        ElohimShopDbContext db,
        PlatformDbContext platformDb,
        IOptions<StripePaymentOptions> options)
    {
        _db = db;
        _platformDb = platformDb;
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

        // Primero verificamos el evento usando la clave global/secreta temporalmente
        StripeConfiguration.ApiKey = secretKey;

        Event stripeEvent = EventUtility.ConstructEvent(json, stripeSignatureHeader, whSecret);

        // Intentamos obtener la reservación para encontrar el TiendaId e inyectar su API Key
        string? reservacionId = null;
        if (stripeEvent.Data.Object is PaymentIntent pi)
        {
            pi.Metadata.TryGetValue("reservacion_id", out reservacionId);
        }

        string? tiendaId = null;

        if (!string.IsNullOrEmpty(reservacionId))
        {
            var resPlatform = await _platformDb.Reservaciones
                .IgnoreQueryFilters()
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Id == reservacionId, ct)
                .ConfigureAwait(false);

            if (resPlatform != null)
            {
                tiendaId = resPlatform.TiendaId;
            }
        }
        else if (stripeEvent.Data.Object is Charge charge && !string.IsNullOrEmpty(charge.PaymentIntentId))
        {
            var resPlatform = await _platformDb.Reservaciones
                .IgnoreQueryFilters()
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.StripeIntentId == charge.PaymentIntentId, ct)
                .ConfigureAwait(false);

            if (resPlatform != null)
            {
                tiendaId = resPlatform.TiendaId;
            }
        }

        if (!string.IsNullOrEmpty(tiendaId))
        {
            var creds = await _platformDb.CredencialesIntegraciones
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.TiendaId == tiendaId, ct)
                .ConfigureAwait(false);

            if (creds != null && !string.IsNullOrEmpty(creds.StripeSecretKey))
            {
                secretKey = creds.StripeSecretKey.Trim();
            }
        }

        StripeConfiguration.ApiKey = secretKey;

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

        var platformReservacion = await _platformDb.Reservaciones
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(r => r.Id == rid, ct)
            .ConfigureAwait(false);

        if (platformReservacion != null)
        {
            if (platformReservacion.EstadoPago == "pagado")
            {
                return;
            }

            if (!string.IsNullOrEmpty(platformReservacion.StripeIntentId) &&
                !string.Equals(platformReservacion.StripeIntentId, pi.Id, StringComparison.Ordinal))
            {
                return;
            }

            if (string.IsNullOrEmpty(platformReservacion.StripeIntentId))
            {
                platformReservacion.StripeIntentId = pi.Id;
            }

            platformReservacion.EstadoPago = "pagado";
            await _platformDb.SaveChangesAsync(ct).ConfigureAwait(false);
            return;
        }

        try
        {
            var reservacion = await _db.Reservaciones
                .FirstOrDefaultAsync(r => r.IdReservacion == rid, ct)
                .ConfigureAwait(false);

            if (reservacion is null)
            {
                return;
            }

            if (reservacion.Pagado)
            {
                return;
            }

            if (!string.IsNullOrEmpty(reservacion.StripePaymentIntentId) &&
                !string.Equals(reservacion.StripePaymentIntentId, pi.Id, StringComparison.Ordinal))
            {
                reservacion.AnexarObservacion(
                    $"Webhook payment_intent.succeeded: PI {pi.Id} no coincide con el PI de la reserva ({reservacion.StripePaymentIntentId}).");
                await _db.SaveChangesAsync(ct).ConfigureAwait(false);
                return;
            }

            if (string.IsNullOrEmpty(reservacion.StripePaymentIntentId))
            {
                reservacion.AsignarStripePaymentIntent(pi.Id);
            }

            reservacion.MarcarComoPagada();
            await _db.SaveChangesAsync(ct).ConfigureAwait(false);
        }
        catch (Exception)
        {
        }
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

        var platformReservacion = await _platformDb.Reservaciones
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(r => r.Id == rid, ct)
            .ConfigureAwait(false);

        if (platformReservacion != null)
        {
            platformReservacion.EstadoPago = "fallido";
            await _platformDb.SaveChangesAsync(ct).ConfigureAwait(false);
            return;
        }

        try
        {
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
        catch (Exception)
        {
        }
    }

    private async Task ManejarCargoReembolsadoAsync(Event stripeEvent, CancellationToken ct)
    {
        if (stripeEvent.Data.Object is not Charge charge || string.IsNullOrEmpty(charge.PaymentIntentId))
        {
            return;
        }

        var platformReservacion = await _platformDb.Reservaciones
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(r => r.StripeIntentId == charge.PaymentIntentId, ct)
            .ConfigureAwait(false);

        if (platformReservacion != null)
        {
            platformReservacion.EstadoPago = "reembolsado";
            await _platformDb.SaveChangesAsync(ct).ConfigureAwait(false);
            return;
        }

        try
        {
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
        catch (Exception)
        {
        }
    }
}
