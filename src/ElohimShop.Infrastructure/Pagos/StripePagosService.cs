using ElohimShop.Application.Pagos;
using ElohimShop.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Stripe;

namespace ElohimShop.Infrastructure.Pagos;

public class StripePagosService : IPagosService
{
    private readonly ElohimShopDbContext _db;
    private readonly StripePaymentOptions _options;

    public StripePagosService(ElohimShopDbContext db, IOptions<StripePaymentOptions> options)
    {
        _db = db;
        _options = options.Value;
    }

    public async Task<PaymentIntentCreadoDto> CrearPaymentIntentAsync(
        string usuarioIdCliente,
        CrearPaymentIntentDto dto,
        CancellationToken ct = default)
    {
        AplicarApiKey();

        var reservacion = await _db.Reservaciones
            .Include(r => r.MetodoPago)
            .FirstOrDefaultAsync(r => r.IdReservacion == dto.ReservacionId, ct)
            .ConfigureAwait(false);

        if (reservacion is null || reservacion.Pagado || reservacion.ClienteId != usuarioIdCliente)
        {
            throw new InvalidOperationException("La reservación ya fue pagada o no existe.");
        }

        var metodo = reservacion.MetodoPago;
        if (metodo is null || string.IsNullOrWhiteSpace(metodo.StripePaymentMethodId))
        {
            throw new InvalidOperationException("La reservación requiere un método de pago Stripe guardado.");
        }

        var total = reservacion.TotalRenovacion ?? 0;
        if (total <= 0)
        {
            throw new InvalidOperationException("El monto de la reservación no es válido.");
        }

        var usuario = await _db.Usuarios
            .FirstOrDefaultAsync(u => u.Id == reservacion.ClienteId, ct)
            .ConfigureAwait(false)
            ?? throw new InvalidOperationException("Cliente no encontrado.");

        if (string.IsNullOrEmpty(usuario.StripeCustomerId))
        {
            var customerService = new CustomerService();
            var customer = await customerService.CreateAsync(
                new CustomerCreateOptions
                {
                    Email = usuario.Correo,
                    Metadata = new Dictionary<string, string> { ["usuario_id"] = usuario.Id }
                },
                cancellationToken: ct).ConfigureAwait(false);

            usuario.AsignarStripeCustomerId(customer.Id);
            await _db.SaveChangesAsync(ct).ConfigureAwait(false);
        }

        var moneda = (Environment.GetEnvironmentVariable("STRIPE_DEFAULT_CURRENCY")?.Trim()
            ?? _options.DefaultCurrency
            ?? "gtq").ToLowerInvariant();
        var montoCentavos = AMonedaMasPequena(total, moneda);

        var paymentIntentService = new PaymentIntentService();
        var options = new PaymentIntentCreateOptions
        {
            Amount = montoCentavos,
            Currency = moneda,
            Customer = usuario.StripeCustomerId,
            PaymentMethod = metodo.StripePaymentMethodId,
            Metadata = new Dictionary<string, string> { ["reservacion_id"] = reservacion.IdReservacion },
            AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions
            {
                Enabled = true,
                AllowRedirects = "never"
            }
        };

        var paymentIntent = await paymentIntentService.CreateAsync(options, cancellationToken: ct).ConfigureAwait(false);

        reservacion.AsignarStripePaymentIntent(paymentIntent.Id);
        await _db.SaveChangesAsync(ct).ConfigureAwait(false);

        var clientSecret = paymentIntent.ClientSecret
            ?? throw new InvalidOperationException("Stripe no devolvió clientSecret.");

        return new PaymentIntentCreadoDto
        {
            ClientSecret = clientSecret,
            ReservacionId = reservacion.IdReservacion,
            MontoCentavos = montoCentavos,
            Moneda = moneda
        };
    }

    public async Task<PagoEstadoDto?> ObtenerEstadoPagoAsync(
        string paymentIntentId,
        string? clienteId,
        bool esAdministrador,
        CancellationToken ct = default)
    {
        AplicarApiKey();

        var paymentIntentService = new PaymentIntentService();
        PaymentIntent pi;
        try
        {
            pi = await paymentIntentService.GetAsync(paymentIntentId, cancellationToken: ct).ConfigureAwait(false);
        }
        catch (StripeException)
        {
            return null;
        }

        if (!pi.Metadata.TryGetValue("reservacion_id", out var rid))
        {
            return null;
        }

        var reservacion = await _db.Reservaciones
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.IdReservacion == rid, ct)
            .ConfigureAwait(false);

        if (reservacion is null)
        {
            return null;
        }

        if (!esAdministrador && reservacion.ClienteId != clienteId)
        {
            throw new UnauthorizedAccessException("No tenés permisos para consultar este pago.");
        }

        return new PagoEstadoDto
        {
            PaymentIntentId = pi.Id,
            Status = MapearEstadoParaApi(pi.Status),
            ReservacionId = rid,
            MontoCentavos = pi.Amount,
            Moneda = pi.Currency
        };
    }

    public async Task<ReembolsoCreadoDto> ReembolsarAsync(
        string paymentIntentId,
        ReembolsoRequestDto dto,
        CancellationToken ct = default)
    {
        AplicarApiKey();

        var paymentIntentService = new PaymentIntentService();
        PaymentIntent pi;
        try
        {
            pi = await paymentIntentService.GetAsync(paymentIntentId, cancellationToken: ct).ConfigureAwait(false);
        }
        catch (StripeException)
        {
            throw new InvalidOperationException("PaymentIntent no encontrado.");
        }

        var refundOptions = new RefundCreateOptions
        {
            PaymentIntent = paymentIntentId,
            Amount = dto.MontoCentavos
        };

        var razon = dto.Razon?.Trim().ToLowerInvariant();
        refundOptions.Reason = razon switch
        {
            "duplicate" => "duplicate",
            "fraudulent" => "fraudulent",
            "requested_by_customer" => "requested_by_customer",
            _ => null
        };

        var refundService = new RefundService();
        Refund refund;
        try
        {
            refund = await refundService.CreateAsync(refundOptions, cancellationToken: ct).ConfigureAwait(false);
        }
        catch (StripeException ex)
        {
            throw new InvalidOperationException(ex.StripeError?.Message ?? "No se pudo procesar el reembolso.");
        }

        return new ReembolsoCreadoDto
        {
            ReembolsoId = refund.Id,
            Estado = refund.Status,
            MontoCentavos = refund.Amount,
            Moneda = refund.Currency
        };
    }

    private void AplicarApiKey()
    {
        var key = Environment.GetEnvironmentVariable("STRIPE_SECRET_KEY")?.Trim()
            ?? _options.SecretKey?.Trim();

        if (string.IsNullOrEmpty(key))
        {
            throw new InvalidOperationException("Stripe no está configurado (STRIPE_SECRET_KEY).");
        }

        StripeConfiguration.ApiKey = key;
    }

    /// <summary>Stripe espera enteros en la unidad mínima; para divisas sin decimales se usa el monto entero.</summary>
    private static long AMonedaMasPequena(decimal total, string moneda)
    {
        var ceroDecimales = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "bif", "clp", "djf", "gnf", "jpy", "kmf", "krw", "mga", "pyg", "rwf", "ugx", "vnd", "vuv", "xaf", "xof", "xpf"
        };

        if (ceroDecimales.Contains(moneda))
        {
            return (long)Math.Round(total, MidpointRounding.AwayFromZero);
        }

        return (long)Math.Round(total * 100m, MidpointRounding.AwayFromZero);
    }

    private static string MapearEstadoParaApi(string stripeStatus) => stripeStatus switch
    {
        "succeeded" => "succeeded",
        "processing" or "requires_capture" => "processing",
        "canceled" => "canceled",
        "requires_payment_method" or "requires_confirmation" or "requires_action" => "requires_payment_method",
        _ => stripeStatus
    };
}
