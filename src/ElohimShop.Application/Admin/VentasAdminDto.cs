namespace ElohimShop.Application.Admin;

public record VentasResumenDto(
    int VentasHoy,
    decimal IngresosHoy,
    decimal TicketPromedio,
    int ProductosVendidos);

public record VentaAdminItemDto(
    string Id,
    string Cliente,
    int Productos,
    decimal Subtotal,
    decimal Descuento,
    decimal Total,
    DateTime Fecha,
    string MetodoPago,
    string Empleado,
    string EstadoVenta);

public record VentasAdminListadoDto(
    VentasResumenDto Resumen,
    IReadOnlyList<VentaAdminItemDto> Ventas);
