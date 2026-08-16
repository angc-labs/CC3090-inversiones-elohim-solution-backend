using System.Security.Claims;
using System.Text;
using ElohimShop.API.Controllers;
using ElohimShop.Application.Pagos;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace ElohimShop.Tests.PagosControllerTests;

public class PagosControllerTests
{
    private readonly Mock<IPagosService> _pagosServiceMock = new();
    private readonly Mock<IStripeWebhookHandler> _webhookHandlerMock = new();

    [Fact]
    public async Task CrearIntent_ClienteAutenticado_RetornaOkConPaymentIntent()
    {
        // Arrange
        var controller = CreateController();
        var dto = new CrearPaymentIntentDto { ReservacionId = "res-123", MetodoPagoId = "pm_123" };
        var expected = new PaymentIntentCreadoDto
        {
            ReservacionId = "res-123",
            ClientSecret = "secret_123",
            MontoCentavos = 1500,
            Moneda = "gtq"
        };

        _pagosServiceMock
            .Setup(x => x.CrearPaymentIntentAsync("user-42", dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        // Act
        var result = await controller.CrearIntent(dto, CancellationToken.None);

        // Assert
        var ok = Assert.IsType<OkObjectResult>(result);
        var value = Assert.IsType<PaymentIntentCreadoDto>(ok.Value);
        Assert.Equal("res-123", value.ReservacionId);
        Assert.Equal("secret_123", value.ClientSecret);
        Assert.Equal(1500, value.MontoCentavos);
    }

    [Fact]
    public async Task CrearIntent_UsuarioNoCliente_DevuelveForbid()
    {
        // Arrange
        var controller = CreateController(tipoUsuario: "administrador", subject: "admin-1");
        var dto = new CrearPaymentIntentDto { ReservacionId = "res-123" };

        // Act
        var result = await controller.CrearIntent(dto, CancellationToken.None);

        // Assert
        Assert.IsType<ForbidResult>(result);
    }

    [Fact]
    public async Task CrearIntent_ServicioLanzaInvalidOperation_DevuelveBadRequest()
    {
        // Arrange
        var controller = CreateController();
        var dto = new CrearPaymentIntentDto { ReservacionId = "res-invalid" };

        _pagosServiceMock
            .Setup(x => x.CrearPaymentIntentAsync("user-42", dto, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("La reservación no es válida."));

        // Act
        var result = await controller.CrearIntent(dto, CancellationToken.None);

        // Assert
        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.NotNull(badRequest.Value);
        Assert.Contains("La reservación no es válida.", badRequest.Value.ToString());
    }

    [Fact]
    public async Task WebhookStripe_HeaderFaltante_DevuelveBadRequest()
    {
        // Arrange
        var controller = CreateController();
        controller.ControllerContext.HttpContext.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes("{\"type\":\"payment_intent.succeeded\"}"));

        // Act
        var result = await controller.WebhookStripe(CancellationToken.None);

        // Assert
        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.NotNull(badRequest.Value);
        Assert.Contains("Stripe-Signature", badRequest.Value.ToString());
    }

    [Fact]
    public async Task WebhookStripe_ConFirmaValida_ProcesaEventoYDevuelveOk()
    {
        // Arrange
        var controller = CreateController();
        controller.ControllerContext.HttpContext.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes("{\"type\":\"payment_intent.succeeded\"}"));
        controller.ControllerContext.HttpContext.Request.Headers["Stripe-Signature"] = "valid-signature";

        _webhookHandlerMock
            .Setup(x => x.ProcesarEventoRawAsync(It.IsAny<string>(), "valid-signature", It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await controller.WebhookStripe(CancellationToken.None);

        // Assert
        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(ok.Value);
        Assert.Equal(true, ok.Value.GetType().GetProperty("recibido")?.GetValue(ok.Value));
        _webhookHandlerMock.Verify(x => x.ProcesarEventoRawAsync(
            It.Is<string>(json => json.Contains("payment_intent.succeeded")),
            "valid-signature",
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Estado_Administrador_DevuelveOkConEstadoPago()
    {
        // Arrange
        var controller = CreateController(tipoUsuario: "administrador");
        var expected = new PagoEstadoDto
        {
            PaymentIntentId = "pi_123",
            Status = "succeeded",
            ReservacionId = "res-123",
            MontoCentavos = 2500,
            Moneda = "gtq"
        };

        _pagosServiceMock
            .Setup(x => x.ObtenerEstadoPagoAsync("pi_123", null, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        // Act
        var result = await controller.Estado("pi_123", CancellationToken.None);

        // Assert
        var ok = Assert.IsType<OkObjectResult>(result);
        var value = Assert.IsType<PagoEstadoDto>(ok.Value);
        Assert.Equal("succeeded", value.Status);
        Assert.Equal("res-123", value.ReservacionId);
    }

    private PagosController CreateController(string tipoUsuario = "cliente", string subject = "user-42")
    {
        var httpContext = new DefaultHttpContext();
        httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim("tipo_usuario", tipoUsuario),
            new Claim("sub", subject),
            new Claim(ClaimTypes.NameIdentifier, subject)
        }, "TestAuth"));

        var controller = new ElohimShop.API.Controllers.PagosController(_pagosServiceMock.Object, _webhookHandlerMock.Object)
        {
            ControllerContext = new ControllerContext { HttpContext = httpContext }
        };

        return controller;
    }
}

