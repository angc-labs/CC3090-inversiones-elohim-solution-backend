using ElohimShop.Application.Pagos;
using ElohimShop.Domain.Entities;
using ElohimShop.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Stripe;

namespace ElohimShop.Infrastructure.Pagos;

public class MetodosPagoUsuarioService : IMetodosPagoUsuarioService
{
    private readonly ElohimShopDbContext _db;
    private readonly IOptions<StripePaymentOptions> _options;

    public MetodosPagoUsuarioService(ElohimShopDbContext db, IOptions<StripePaymentOptions> options)
    {
        _db = db;
        _options = options;
    }

    public async Task<IReadOnlyList<MetodoPagoGuardadoDto>> ListarAsync(string usuarioId, CancellationToken ct = default)
    {
        var items = await _db.MetodosPago
            .AsNoTracking()
            .Where(m => m.UsuarioId == usuarioId && m.Activo && m.StripePaymentMethodId != null)
            .OrderByDescending(m => m.IdMetodoPago)
            .ToListAsync(ct)
            .ConfigureAwait(false);

        return items.Select(MapToDto).ToList();
    }

    public async Task<MetodoPagoGuardadoDto> AsegurarContraEntregaAsync(string usuarioId, CancellationToken ct = default)
    {
        const string nombreContraEntrega = "Contra entrega";

        var existente = await _db.MetodosPago
            .FirstOrDefaultAsync(
                m => m.UsuarioId == usuarioId
                    && m.Activo
                    && m.StripePaymentMethodId == null
                    && m.NombreMetodo == nombreContraEntrega,
                ct)
            .ConfigureAwait(false);

        if (existente is not null)
        {
            return MapToDto(existente);
        }

        var entidad = MetodoPago.Crear(usuarioId, nombreContraEntrega);
        _db.MetodosPago.Add(entidad);
        await _db.SaveChangesAsync(ct).ConfigureAwait(false);

        return MapToDto(entidad);
    }

    public async Task<MetodoPagoGuardadoDto> GuardarAsync(string usuarioId, GuardarMetodoPagoDto dto, CancellationToken ct = default)
    {
        var pmId = dto.StripePaymentMethodId?.Trim()
            ?? throw new ArgumentException("stripePaymentMethodId es requerido.");

        if (pmId.StartsWith("pm_", StringComparison.Ordinal) is false)
        {
            throw new ArgumentException("stripePaymentMethodId debe ser un id de método de Stripe (pm_...).");
        }

        var duplicadoActivo = await _db.MetodosPago
            .AnyAsync(
                m => m.UsuarioId == usuarioId && m.Activo && m.StripePaymentMethodId == pmId,
                ct)
            .ConfigureAwait(false);

        if (duplicadoActivo)
        {
            throw new InvalidOperationException("Este método de pago ya está guardado.");
        }

        var usuario = await _db.Usuarios.FirstOrDefaultAsync(u => u.Id == usuarioId, ct).ConfigureAwait(false)
            ?? throw new InvalidOperationException("Usuario no encontrado.");

        AplicarApiKey();

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

        var paymentMethodService = new PaymentMethodService();
        await paymentMethodService.AttachAsync(
            pmId,
            new PaymentMethodAttachOptions { Customer = usuario.StripeCustomerId },
            cancellationToken: ct).ConfigureAwait(false);

        var remote = await paymentMethodService.GetAsync(pmId, cancellationToken: ct).ConfigureAwait(false);
        if (remote.Card is null)
        {
            throw new InvalidOperationException("El método de pago debe ser una tarjeta.");
        }

        var marca = remote.Card.Brand ?? dto.MarcaTarjeta ?? "card";
        var ultimos = remote.Card.Last4 ?? dto.UltimosDigitos ?? string.Empty;
        var expMes = remote.Card.ExpMonth > 0 ? (int)remote.Card.ExpMonth : dto.ExpiraMes ?? 0;
        var expAnio = remote.Card.ExpYear > 0 ? (int)remote.Card.ExpYear : dto.ExpiraAnio ?? 0;

        var nombreMetodo = string.IsNullOrWhiteSpace(dto.NombreMetodo) ? "Tarjeta" : dto.NombreMetodo;

        MetodoPago entidad;

        try
        {
            entidad = MetodoPago.CrearDesdeStripe(
                usuarioId,
                pmId,
                nombreMetodo,
                dto.Descripcion,
                marca,
                ultimos,
                expMes,
                expAnio,
                dto.Alias?.Trim());
        }
        catch (Exception ex) when (ex is not ArgumentException and not InvalidOperationException)
        {
            throw new InvalidOperationException("No se pudo registrar el método de pago.", ex);
        }

        _db.MetodosPago.Add(entidad);
        await _db.SaveChangesAsync(ct).ConfigureAwait(false);

        return MapToDto(entidad);
    }

    public async Task EliminarAsync(string usuarioId, string idMetodoPagoInterno, CancellationToken ct = default)
    {
        var metodo = await _db.MetodosPago
            .FirstOrDefaultAsync(m => m.IdMetodoPago == idMetodoPagoInterno && m.UsuarioId == usuarioId, ct)
            .ConfigureAwait(false);

        if (metodo is null || !metodo.Activo)
        {
            throw new InvalidOperationException("Método de pago no encontrado.");
        }

        if (!string.IsNullOrEmpty(metodo.StripePaymentMethodId))
        {
            try
            {
                AplicarApiKey();
                var paymentMethodService = new PaymentMethodService();
                await paymentMethodService.DetachAsync(metodo.StripePaymentMethodId, cancellationToken: ct)
                    .ConfigureAwait(false);
            }
            catch (StripeException)
            {
                // Si ya está desvinculado en Stripe, continuar para limpiar en BD.
            }
        }

        metodo.Desactivar();
        await _db.SaveChangesAsync(ct).ConfigureAwait(false);
    }

    private static MetodoPagoGuardadoDto MapToDto(MetodoPago m) => new()
    {
        IdMetodoPago = m.IdMetodoPago,
        NombreMetodo = m.NombreMetodo,
        Descripcion = m.Descripcion,
        Activo = m.Activo,
        StripePaymentMethodId = m.StripePaymentMethodId,
        UltimosDigitos = m.UltimosDigitos,
        MarcaTarjeta = m.MarcaTarjeta,
        ExpiraMes = m.ExpiraMes,
        ExpiraAnio = m.ExpiraAnio,
        Alias = m.Alias
    };

    private void AplicarApiKey()
    {
        var opts = _options.Value;
        var key = Environment.GetEnvironmentVariable("STRIPE_SECRET_KEY")?.Trim()
            ?? opts.SecretKey?.Trim();

        if (string.IsNullOrEmpty(key))
        {
            throw new InvalidOperationException("Stripe no está configurado (STRIPE_SECRET_KEY).");
        }

        StripeConfiguration.ApiKey = key;
    }
}
