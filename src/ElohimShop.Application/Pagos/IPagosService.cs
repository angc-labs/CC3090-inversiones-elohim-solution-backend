namespace ElohimShop.Application.Pagos;

public interface IPagosService
{
    Task<PaymentIntentCreadoDto> CrearPaymentIntentAsync(string usuarioIdCliente, CrearPaymentIntentDto dto, CancellationToken ct = default);
    Task<PagoEstadoDto?> ObtenerEstadoPagoAsync(string paymentIntentId, string? clienteId, bool esAdministrador, CancellationToken ct = default);
    Task<ReembolsoCreadoDto> ReembolsarAsync(string paymentIntentId, ReembolsoRequestDto dto, CancellationToken ct = default);
}
