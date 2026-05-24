using System.Text;
using System.Threading;
using ElohimShop.Application.Inventario;
using ElohimShop.Application.Inventario.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ElohimShop.API.Controllers.Admin;

[ApiController]
[Route("api/admin/inventario")]
[Authorize(Roles = "Administrador")]
public class InventarioController : ControllerBase
{
    private readonly IGetInventarioUseCase _useCase;
    private readonly IExportInventarioUseCase _exportUseCase;

    public InventarioController(IGetInventarioUseCase useCase, IExportInventarioUseCase exportUseCase)
    {
        _useCase = useCase;
        _exportUseCase = exportUseCase;
    }

    [HttpGet]
    [ProducesResponseType(typeof(InventarioResponseDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get(
        [FromQuery(Name = "q")] string? q,
        [FromQuery(Name = "categoriaId")] string? categoriaId,
        [FromQuery(Name = "estado")] string? estado,
        [FromQuery(Name = "orderBy")] string? orderBy,
        [FromQuery(Name = "order")] string? order,
        [FromQuery(Name = "page")] int? page,
        [FromQuery(Name = "limit")] int? limit,
        CancellationToken cancellationToken)
    {
        var query = new InventarioQuery(q, categoriaId, estado, orderBy, order, page ?? 1, limit ?? 20);
        var result = await _useCase.EjecutarAsync(query, cancellationToken);
        return Ok(result);
    }

    [HttpGet("exportar")]
    [Produces("text/csv")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Exportar(
        [FromQuery(Name = "q")] string? q,
        [FromQuery(Name = "categoriaId")] string? categoriaId,
        [FromQuery(Name = "estado")] string? estado,
        [FromQuery(Name = "orderBy")] string? orderBy,
        [FromQuery(Name = "order")] string? order,
        CancellationToken cancellationToken)
    {
        var query = new InventarioQuery(q, categoriaId, estado, orderBy, order, 1, 0);
        var productos = await _exportUseCase.EjecutarAsync(query, cancellationToken);

        var csvBuilder = new StringBuilder();
        csvBuilder.AppendLine("codigo,nombre,categoría,marca,precio,stock actual,stock mínimo,estado,valor stock,fecha vencimiento,descuento activo,fecha fin oferta");

        foreach (var producto in productos)
        {
            csvBuilder.AppendLine(string.Join(",",
                EscapeCsv(producto.CodigoProducto),
                EscapeCsv(producto.NombreProducto),
                EscapeCsv(producto.Categoria?.Nombre),
                EscapeCsv(producto.Marca?.Nombre),
                producto.Precio.ToString(),
                producto.StockActual.ToString(),
                producto.StockMinimo.ToString(),
                EscapeCsv(producto.Estado),
                producto.ValorStock.ToString(),
                EscapeCsv(producto.FechaVencimiento.ToString("o")),
                producto.DescuentoActivo ? "true" : "false",
                EscapeCsv(producto.FechaFinOferta?.ToString("o") ?? string.Empty)
            ));
        }

        return File(Encoding.UTF8.GetBytes(csvBuilder.ToString()), "text/csv", "inventario.csv");
    }

    private static string EscapeCsv(string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return string.Empty;
        }

        return '"' + value.Replace("\"", "\"\"") + '"';
    }
}
