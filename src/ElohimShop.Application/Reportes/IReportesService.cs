namespace ElohimShop.Application.Reportes;

public interface IReportesService
{
    Task<ReporteProductosDto> ObtenerProductosAsync(ReportesFiltroDto filtro, CancellationToken cancellationToken);
    Task<ReporteEmpleadosDto> ObtenerEmpleadosAsync(ReportesFiltroDto filtro, CancellationToken cancellationToken);
    Task<ReporteStockCriticoDto> ObtenerStockCriticoAsync(CancellationToken cancellationToken);
    Task<ReporteDemandaDto> ObtenerDemandaAsync(ReportesFiltroDto filtro, CancellationToken cancellationToken);
    Task<ReporteMetodosPagoDto> ObtenerMetodosPagoAsync(ReportesFiltroDto filtro, CancellationToken cancellationToken);
}
