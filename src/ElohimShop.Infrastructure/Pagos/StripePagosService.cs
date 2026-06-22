using ElohimShop.Application.Pagos;
using ElohimShop.Domain.Entities;
using ElohimShop.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Stripe;

namespace ElohimShop.Infrastructure.Pagos;

public class StripePagosService : IPagosService
{
    private readonly ElohimShopDbContext _db;
    private readonly PlatformDbContext _platformDb;
    private readonly ITenantProvider _tenantProvider;
    private readonly StripePaymentOptions _options;

    public StripePagosService(
        ElohimShopDbContext db,
        PlatformDbContext platformDb,
        ITenantProvider tenantProvider,
        IOptions<StripePaymentOptions> options)
    {
        _db = db;
        _platformDb = platformDb;
        _tenantProvider = tenantProvider;
        _options = options.Value;
    }

    public async Task<PaymentIntentCreadoDto> CrearPaymentIntentAsync(
        string usuarioIdCliente,
        CrearPaymentIntentDto dto,
        CancellationToken ct = default)
    {
        AplicarApiKey();

        var platformReservacion = await _platformDb.Reservaciones
            .FirstOrDefaultAsync(r => r.Id == dto.ReservacionId, ct)
            .ConfigureAwait(false);

        decimal total = 0;
        string? clienteId = null;

        if (platformReservacion != null)
        {
            if (platformReservacion.EstadoPago == "pagado")
            {
                throw new InvalidOperationException("La reservación ya fue pagada.");
            }
            if (platformReservacion.UsuarioId != usuarioIdCliente)
            {
                throw new InvalidOperationException("La reservación no pertenece al usuario.");
            }

            total = platformReservacion.MontoTotal;
            clienteId = platformReservacion.UsuarioId;
        }
        else
        {
            try
            {
                var reservacion = await _db.Reservaciones
                    .FirstOrDefaultAsync(r => r.IdReservacion == dto.ReservacionId, ct)
                    .ConfigureAwait(false);

                if (reservacion is null || reservacion.Pagado || reservacion.ClienteId != usuarioIdCliente)
                {
                    throw new InvalidOperationException("La reservación ya fue pagada o no existe.");
                }

                total = reservacion.TotalRenovacion ?? 0;
                clienteId = reservacion.ClienteId;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("La reservación no existe o no se pudo cargar.", ex);
            }
        }

        if (total <= 0)
        {
            throw new InvalidOperationException("El monto de la reservación no es válido.");
        }

        MetodoPago? metodo = null;
        if (!string.IsNullOrWhiteSpace(dto.MetodoPagoId))
        {
            metodo = await _db.MetodosPago
                .FirstOrDefaultAsync(m => m.IdMetodoPago == dto.MetodoPagoId && m.UsuarioId == usuarioIdCliente, ct)
                .ConfigureAwait(false);
        }
        else
        {
            metodo = await _db.MetodosPago
                .Where(m => m.UsuarioId == usuarioIdCliente && m.Activo && m.StripePaymentMethodId != null)
                .OrderByDescending(m => m.IdMetodoPago)
                .FirstOrDefaultAsync(ct)
                .ConfigureAwait(false);
        }

        if (metodo is null || string.IsNullOrWhiteSpace(metodo.StripePaymentMethodId))
        {
            throw new InvalidOperationException("No se encontró un método de pago Stripe guardado válido para este usuario.");
        }

        var usuario = await _platformDb.Users
            .FirstOrDefaultAsync(u => u.Id == clienteId, ct)
            .ConfigureAwait(false)
            ?? throw new InvalidOperationException("Cliente no encontrado.");

        if (string.IsNullOrEmpty(usuario.StripeCustomerId))
        {
            var customerService = new CustomerService();
            var customer = await customerService.CreateAsync(
                new CustomerCreateOptions
                {
                    Email = usuario.Email,
                    Metadata = new Dictionary<string, string> { ["usuario_id"] = usuario.Id }
                },
                cancellationToken: ct).ConfigureAwait(false);

            usuario.StripeCustomerId = customer.Id;
            await _platformDb.SaveChangesAsync(ct).ConfigureAwait(false);
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
            Confirm = true,
            OffSession = false,
            Metadata = new Dictionary<string, string> { ["reservacion_id"] = dto.ReservacionId },
            AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions
            {
                Enabled = true,
                AllowRedirects = "never"
            }
        };

        var paymentIntent = await paymentIntentService.CreateAsync(options, cancellationToken: ct).ConfigureAwait(false);

        if (platformReservacion != null)
        {
            platformReservacion.StripeIntentId = paymentIntent.Id;
            if (paymentIntent.Status == "succeeded")
            {
                platformReservacion.EstadoPago = "pagado";
            }
            await _platformDb.SaveChangesAsync(ct).ConfigureAwait(false);
        }
        else
        {
            try
            {
                var reservacion = await _db.Reservaciones
                    .FirstOrDefaultAsync(r => r.IdReservacion == dto.ReservacionId, ct)
                    .ConfigureAwait(false);
                if (reservacion != null)
                {
                    reservacion.AsignarStripePaymentIntent(paymentIntent.Id);
                    if (paymentIntent.Status == "succeeded")
                    {
                        reservacion.MarcarComoPagada();
                    }
                    await _db.SaveChangesAsync(ct).ConfigureAwait(false);
                }
            }
            catch (Exception)
            {
            }
        }

        var clientSecret = paymentIntent.ClientSecret
            ?? throw new InvalidOperationException("Stripe no devolvió clientSecret.");

        return new PaymentIntentCreadoDto
        {
            ClientSecret = clientSecret,
            ReservacionId = dto.ReservacionId,
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

        var platformReservacion = await _platformDb.Reservaciones
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == rid, ct)
            .ConfigureAwait(false);

        if (platformReservacion != null)
        {
            if (!esAdministrador && platformReservacion.UsuarioId != clienteId)
            {
                throw new UnauthorizedAccessException("No tenés permisos para consultar este pago.");
            }

            await SincronizarPagadoConStripePlatformAsync(rid, pi.Id, pi.Status, ct).ConfigureAwait(false);

            return new PagoEstadoDto
            {
                PaymentIntentId = pi.Id,
                Status = MapearEstadoParaApi(pi.Status),
                ReservacionId = rid,
                MontoCentavos = pi.Amount,
                Moneda = pi.Currency
            };
        }

        try
        {
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

            await SincronizarPagadoConStripeAsync(rid, pi.Id, pi.Status, ct).ConfigureAwait(false);

            return new PagoEstadoDto
            {
                PaymentIntentId = pi.Id,
                Status = MapearEstadoParaApi(pi.Status),
                ReservacionId = rid,
                MontoCentavos = pi.Amount,
                Moneda = pi.Currency
            };
        }
        catch (Exception)
        {
            return null;
        }
    }

    private async Task SincronizarPagadoConStripePlatformAsync(
        string reservacionId,
        string paymentIntentId,
        string stripeStatus,
        CancellationToken ct)
    {
        if (!string.Equals(stripeStatus, "succeeded", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        var entity = await _platformDb.Reservaciones
            .FirstOrDefaultAsync(r => r.Id == reservacionId, ct)
            .ConfigureAwait(false);

        if (entity is null || entity.EstadoPago == "pagado")
        {
            return;
        }

        if (!string.IsNullOrEmpty(entity.StripeIntentId) &&
            !string.Equals(entity.StripeIntentId, paymentIntentId, StringComparison.Ordinal))
        {
            return;
        }

        if (string.IsNullOrEmpty(entity.StripeIntentId))
        {
            entity.StripeIntentId = paymentIntentId;
        }

        entity.EstadoPago = "pagado";
        await _platformDb.SaveChangesAsync(ct).ConfigureAwait(false);
    }

    private async Task SincronizarPagadoConStripeAsync(
        string reservacionId,
        string paymentIntentId,
        string stripeStatus,
        CancellationToken ct)
    {
        if (!string.Equals(stripeStatus, "succeeded", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        var entity = await _db.Reservaciones
            .FirstOrDefaultAsync(r => r.IdReservacion == reservacionId, ct)
            .ConfigureAwait(false);

        if (entity is null || entity.Pagado)
        {
            return;
        }

        if (!string.IsNullOrEmpty(entity.StripePaymentIntentId) &&
            !string.Equals(entity.StripePaymentIntentId, paymentIntentId, StringComparison.Ordinal))
        {
            return;
        }

        if (string.IsNullOrEmpty(entity.StripePaymentIntentId))
        {
            entity.AsignarStripePaymentIntent(paymentIntentId);
        }

        entity.MarcarComoPagada();
        await _db.SaveChangesAsync(ct).ConfigureAwait(false);
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
        var tenantId = _tenantProvider.GetTenantId();
        string? key = null;

        if (!string.IsNullOrEmpty(tenantId))
        {
            var creds = _platformDb.CredencialesIntegraciones
                .AsNoTracking()
                .FirstOrDefault(x => x.TiendaId == tenantId);
            if (creds != null && !string.IsNullOrEmpty(creds.StripeSecretKey))
            {
                key = creds.StripeSecretKey.Trim();
            }
        }

        if (string.IsNullOrEmpty(key))
        {
            key = Environment.GetEnvironmentVariable("STRIPE_SECRET_KEY")?.Trim()
                ?? _options.SecretKey?.Trim();
        }

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
