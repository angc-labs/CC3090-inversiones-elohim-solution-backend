namespace ElohimShop.Application.Reportes;

/// <param name="Modo">todos | ventas | reservaciones</param>
public sealed record ReportesFiltroDto(
    DateTime? Desde,
    DateTime? Hasta,
    string Modo = "todos");

public sealed record ReporteProductosDto(
    int TotalProductosVendidos,
    decimal IngresosTotales,
    string? ProductoTop,
    int? UnidadesProductoTop,
    IReadOnlyList<ReporteProductoItemDto> Productos,
    IReadOnlyList<ReporteProductoDetalleDto> Detalle);

public sealed record ReporteProductoItemDto(
    string Producto,
    int CantidadVendida,
    decimal Ingresos);

public sealed record ReporteProductoDetalleDto(
    int Posicion,
    string Producto,
    int CantidadVendida,
    decimal Ingresos,
    decimal PrecioPromedio);

public sealed record ReporteEmpleadosDto(
    int TotalEmpleados,
    int TotalVentas,
    decimal MontoTotal,
    string? TopVendedor,
    int? VentasTopVendedor,
    IReadOnlyList<ReporteEmpleadoItemDto> Empleados,
    IReadOnlyList<ReporteEmpleadoDetalleDto> Detalle);

public sealed record ReporteEmpleadoItemDto(
    string Empleado,
    int Ventas,
    decimal Monto);

public sealed record ReporteEmpleadoDetalleDto(
    string Empleado,
    int VentasRealizadas,
    decimal MontoTotal,
    decimal PromedioPorVenta,
    string Desempeno);

public sealed record ReporteStockCriticoDto(
    int ProductosEnRiesgo,
    int UnidadesFaltantes,
    int FrecuenciaAlta,
    IReadOnlyList<ReporteStockChartItemDto> Grafico,
    IReadOnlyList<ReporteStockDetalleDto> Detalle);

public sealed record ReporteStockChartItemDto(
    string Producto,
    int StockActual,
    int StockMinimo);

public sealed record ReporteStockDetalleDto(
    string Producto,
    int StockActual,
    int StockMinimo,
    int Faltante,
    string FrecuenciaQuiebre,
    string Estado);

public sealed record ReporteDemandaDto(
    string HoraPico,
    int VentasHoraPico,
    double PromedioPorHora,
    IReadOnlyList<ReporteDemandaChartItemDto> Grafico,
    IReadOnlyList<ReporteDemandaDetalleDto> Detalle);

public sealed record ReporteDemandaChartItemDto(
    string Horario,
    int Ventas,
    int Clientes);

public sealed record ReporteDemandaDetalleDto(
    string Horario,
    int Ventas,
    int Clientes,
    decimal RatioConversion,
    string Clasificacion);

public sealed record ReporteMetodosPagoDto(
    IReadOnlyList<ReporteMetodoPagoResumenDto> Resumen,
    IReadOnlyList<ReporteMetodoPagoChartItemDto> Distribucion,
    IReadOnlyList<ReporteMetodoPagoDetalleDto> Detalle);

public sealed record ReporteMetodoPagoResumenDto(
    string Metodo,
    int Transacciones,
    decimal Monto);

public sealed record ReporteMetodoPagoChartItemDto(
    string Metodo,
    int Transacciones,
    decimal Monto,
    decimal Porcentaje);

public sealed record ReporteMetodoPagoDetalleDto(
    string Metodo,
    int CantidadTransacciones,
    decimal Porcentaje,
    decimal MontoTotal,
    decimal MontoPromedio);
