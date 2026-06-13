using ElohimShop.Application.Platform;
using Microsoft.AspNetCore.Mvc;

namespace ElohimShop.API.Controllers;

[ApiController]
[Route("api/v1/checkout")]
public class CheckoutController : V1ControllerBase
{
    private readonly IPlatformService _platformService;

    public CheckoutController(IPlatformService platformService)
    {
        _platformService = platformService;
    }

    [HttpPost("crear-intento")]
    public async Task<IActionResult> CrearIntento([FromBody] CrearIntentoPagoRequest request, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (string.IsNullOrWhiteSpace(userId) || GetTenantId() is null)
        {
            return Unauthorized(new { error = "Se requiere autenticación y tenant." });
        }

        var intento = await _platformService.CrearIntentoPagoAsync(userId, request, cancellationToken);
        return Ok(intento);
    }
}