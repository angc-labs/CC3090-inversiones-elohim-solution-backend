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

    public InventarioController(IGetInventarioUseCase useCase)
    {
        _useCase = useCase;
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
}
