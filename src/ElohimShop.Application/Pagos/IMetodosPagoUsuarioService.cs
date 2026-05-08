namespace ElohimShop.Application.Pagos;

public interface IMetodosPagoUsuarioService
{
    Task<IReadOnlyList<MetodoPagoGuardadoDto>> ListarAsync(string usuarioId, CancellationToken ct = default);
    Task<MetodoPagoGuardadoDto> GuardarAsync(string usuarioId, GuardarMetodoPagoDto dto, CancellationToken ct = default);
    Task EliminarAsync(string usuarioId, string idMetodoPagoInterno, CancellationToken ct = default);
}
