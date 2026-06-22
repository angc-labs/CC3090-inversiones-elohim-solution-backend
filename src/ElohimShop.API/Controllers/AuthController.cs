using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Net;
using System.Net.Mail;
using ElohimShop.Application.Auth;
using ElohimShop.Domain.Entities;
using ElohimShop.Infrastructure.Persistence;
using ElohimShop.Infrastructure.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ElohimShop.API.Controllers;

[ApiController]
[Route("api/v1/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly PlatformDbContext _dbContext;
    private readonly ElohimShopDbContext _elohimDbContext;

    public AuthController(IAuthService authService, PlatformDbContext dbContext, ElohimShopDbContext elohimDbContext)
    {
        _authService = authService;
        _dbContext = dbContext;
        _elohimDbContext = elohimDbContext;
    }

    [HttpPost("register")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(object), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto request, CancellationToken cancellationToken)
    {
        try
        {
            AuthResponseDto result;

            if (request.TipoUsuario == "administrador")
            {
                string? tenantId = null;
                var callerRole = User.FindFirstValue("rol");
                var isAnonymous = User.Identity?.IsAuthenticated != true;

                if (!isAnonymous)
                {
                    if (Request.Headers.TryGetValue("X-Tenant-ID", out var tenantIdHeader) && !string.IsNullOrWhiteSpace(tenantIdHeader))
                    {
                        tenantId = tenantIdHeader.ToString();
                    }
                    else
                    {
                        tenantId = User.FindFirstValue("tienda_id");
                    }
                }

                if (!isAnonymous && !string.IsNullOrWhiteSpace(tenantId))
                {
                    var existeAdministradorEnTienda = await _dbContext.Users
                        .IgnoreQueryFilters()
                        .AsNoTracking()
                        .AnyAsync(u => u.TiendaId == tenantId && u.TipoUsuario == "staff" && u.RolStaff == "administrador", cancellationToken);

                    if (callerRole != "administrador" && existeAdministradorEnTienda)
                    {
                        return StatusCode(403, new { error = "No tenés permisos para registrar administradores en esta tienda." });
                    }
                }

                result = await _authService.RegisterAdminAsync(request, cancellationToken);
            }
            else
            {
                result = await _authService.RegisterAsync(request, cancellationToken);
            }

            return StatusCode(201, result);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { error = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = "Datos inválidos.", detalles = new[] { ex.Message } });
        }
    }

    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _authService.LoginAsync(request, cancellationToken);
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { error = ex.Message });
        }
    }

    [Authorize]
    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Logout(CancellationToken cancellationToken)
    {
        var jti = User.FindFirstValue(JwtRegisteredClaimNames.Jti);
        var usuarioId = User.FindFirstValue(JwtRegisteredClaimNames.Sub) ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        var expClaim = User.FindFirstValue(JwtRegisteredClaimNames.Exp);

        if (string.IsNullOrWhiteSpace(jti) || string.IsNullOrWhiteSpace(usuarioId) || string.IsNullOrWhiteSpace(expClaim))
        {
            return Unauthorized(new { error = "Token inválido, expirado o ya revocado." });
        }

        if (!long.TryParse(expClaim, out var expiresAtUnix))
        {
            return Unauthorized(new { error = "Token inválido, expirado o ya revocado." });
        }

        var expiresAt = DateTimeOffset.FromUnixTimeSeconds(expiresAtUnix).UtcDateTime;
        await _authService.LogoutAsync(jti, usuarioId, expiresAt, cancellationToken);

        return NoContent();
    }

    [HttpPost("forgot-password")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequestDto request, CancellationToken cancellationToken)
    {
        string? tenantId = null;
        if (Request.Headers.TryGetValue("X-Tenant-ID", out var tenantIdHeader) && !string.IsNullOrWhiteSpace(tenantIdHeader))
        {
            tenantId = tenantIdHeader.ToString();
        }

        if (string.IsNullOrWhiteSpace(tenantId))
        {
            return BadRequest(new { error = "El header X-Tenant-ID es obligatorio para esta operación." });
        }

        var tienda = await _dbContext.Tiendas
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(t => t.Id == tenantId, cancellationToken);
        var tiendaNombre = tienda?.Nombre ?? "Nuestra Tienda";

        var credenciales = await _dbContext.CredencialesIntegraciones
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.TiendaId == tenantId, cancellationToken);

        if (credenciales is null ||
            string.IsNullOrWhiteSpace(credenciales.SmtpEmail) ||
            string.IsNullOrWhiteSpace(credenciales.SmtpPassword))
        {
            return BadRequest(new { error = "El restablecimiento de contraseña por correo no está configurado para esta tienda. Por favor contacta al administrador." });
        }

        var platformUser = await _dbContext.Users
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Email == request.Correo.Trim().ToLower() && u.TiendaId == tenantId && u.TipoUsuario == "cliente", cancellationToken);

        if (platformUser is null)
        {
            return BadRequest(new { error = "No se encontró un cliente con este correo en esta tienda." });
        }

        // Check if user exists in ElohimShopDbContext by Email
        var elohimUser = await _elohimDbContext.Usuarios
            .FirstOrDefaultAsync(u => u.Correo == platformUser.Email.Trim().ToLower(), cancellationToken);

        if (elohimUser is null)
        {
            // Fetch password hash from platform accounts if available
            var account = await _dbContext.Accounts
                .FirstOrDefaultAsync(a => a.UserId == platformUser.Id && a.ProviderId == "credential", cancellationToken);
            
            var passwordHash = account?.Password ?? string.Empty;

            elohimUser = Usuario.CrearCliente(
                platformUser.Email,
                platformUser.Name,
                passwordHash,
                "particular",
                telefono: platformUser.Telefono);

            _elohimDbContext.Usuarios.Add(elohimUser);
            await _elohimDbContext.SaveChangesAsync(cancellationToken);
        }

        // Invalidar códigos anteriores del usuario
        var codigosAnteriores = await _elohimDbContext.CodigosRecuperacion
            .Where(c => c.UsuarioId == elohimUser.Id && !c.Usado)
            .ToListAsync(cancellationToken);

        foreach (var cod in codigosAnteriores)
        {
            cod.Consumir();
        }

        // Generar 1 código nuevo de 8 caracteres alfanuméricos
        const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
        var bytes = RandomNumberGenerator.GetBytes(8);
        var codigoBuilder = new StringBuilder();
        foreach (var b in bytes)
        {
            codigoBuilder.Append(chars[b % chars.Length]);
        }
        var codigoPlano = codigoBuilder.ToString();

        var hash = Convert.ToBase64String(
            SHA256.HashData(Encoding.UTF8.GetBytes(codigoPlano)));

        _elohimDbContext.CodigosRecuperacion.Add(
            CodigoRecuperacion.Crear(elohimUser.Id, hash, diasValidez: 1));

        await _elohimDbContext.SaveChangesAsync(cancellationToken);

        // Enviar el correo usando SMTP configurado de la tienda
        try
        {
            var (host, port) = GetSmtpSettings(credenciales.SmtpEmail);
            using var client = new SmtpClient(host, port)
            {
                Credentials = new NetworkCredential(credenciales.SmtpEmail, credenciales.SmtpPassword),
                EnableSsl = true
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(credenciales.SmtpEmail, tiendaNombre),
                Subject = $"Código de verificación - {tiendaNombre}",
                Body = $@"
                    <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #e2e8f0; border-radius: 12px;'>
                        <h2 style='color: #1AB38C; text-align: center; margin-bottom: 20px;'>Recuperación de Contraseña</h2>
                        <p>Hola <strong>{platformUser.Name}</strong>,</p>
                        <p>Has solicitado restablecer tu contraseña en la tienda <strong>{tiendaNombre}</strong>.</p>
                        <p>Usa el siguiente código de verificación para completar el proceso:</p>
                        <div style='background-color: #f8fafc; border: 1px dashed #cbd5e1; padding: 15px; text-align: center; margin: 20px 0; font-size: 24px; font-weight: bold; letter-spacing: 4px; color: #334155; font-family: monospace;'>
                            {codigoPlano}
                        </div>
                        <p style='font-size: 12px; color: #64748b;'>Este código es válido por 24 horas. Si no solicitaste este cambio, puedes ignorar este correo de forma segura.</p>
                        <hr style='border: 0; border-top: 1px solid #e2e8f0; margin: 20px 0;' />
                        <p style='font-size: 11px; color: #94a3b8; text-align: center;'>Mensaje enviado automáticamente por {tiendaNombre}.</p>
                    </div>",
                IsBodyHtml = true
            };
            mailMessage.To.Add(platformUser.Email);

            await client.SendMailAsync(mailMessage, cancellationToken);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = "No se pudo enviar el correo de recuperación. Valida que el correo y contraseña de aplicación SMTP configurados sean correctos.", detalles = ex.Message });
        }

        return Ok(new { mensaje = "Se ha enviado un código de verificación a tu correo electrónico." });
    }

    private static (string Host, int Port) GetSmtpSettings(string email)
    {
        var lowerEmail = email.Trim().ToLowerInvariant();
        if (lowerEmail.EndsWith("@gmail.com"))
        {
            return ("smtp.gmail.com", 587);
        }
        if (lowerEmail.EndsWith("@outlook.com") || lowerEmail.EndsWith("@hotmail.com") || lowerEmail.EndsWith("@live.com") || lowerEmail.EndsWith("@live.com.mx"))
        {
            return ("smtp.office365.com", 587);
        }
        // Fallback standard Gmail SMTP
        return ("smtp.gmail.com", 587);
    }

    /// <summary>
    /// Recupera la contraseña usando un código de recuperación generado por un admin.
    /// No requiere autenticación ni envío de email.
    /// </summary>
    [HttpPost("recover-with-code")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RecoverWithCode(
        [FromBody] RecoverWithCodeRequestDto request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Correo) ||
            string.IsNullOrWhiteSpace(request.Codigo) ||
            string.IsNullOrWhiteSpace(request.NuevaContrasena))
        {
            return BadRequest(new { error = "Correo, código y nueva contraseña son obligatorios." });
        }

        if (request.NuevaContrasena.Length < 8)
        {
            return BadRequest(new { error = "La contraseña debe tener al menos 8 caracteres." });
        }

        const string genericError = "El código de recuperación no es válido o ya fue utilizado.";

        // 1. Find user in ElohimShopDbContext (admin users) by email
        var elohimUser = await _elohimDbContext.Usuarios
            .FirstOrDefaultAsync(u => u.Correo == request.Correo.Trim().ToLower(), cancellationToken);

        if (elohimUser is null)
        {
            return BadRequest(new { error = genericError });
        }

        // 2. Hash the provided code and find a matching valid code
        var codigoHash = Convert.ToBase64String(
            SHA256.HashData(Encoding.UTF8.GetBytes(request.Codigo.Trim().ToUpper())));

        var codigoValido = await _elohimDbContext.CodigosRecuperacion
            .FirstOrDefaultAsync(c =>
                c.UsuarioId == elohimUser.Id &&
                c.CodigoHash == codigoHash &&
                !c.Usado &&
                c.FechaExpiracion > DateTime.UtcNow,
                cancellationToken);

        if (codigoValido is null)
        {
            return BadRequest(new { error = genericError });
        }

        // 3. Mark the code as consumed and update password
        codigoValido.Consumir();
        var newHash = PasswordHashing.Hash(request.NuevaContrasena);
        elohimUser.ActualizarContrasena(newHash);

        // 4. Also update the account password in PlatformDbContext (Better Auth)
        var platformUser = await _dbContext.Users
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Email == request.Correo.Trim().ToLower(), cancellationToken);

        if (platformUser is not null)
        {
            var account = await _dbContext.Accounts
                .FirstOrDefaultAsync(a => a.UserId == platformUser.Id && a.ProviderId == "credential", cancellationToken);

            if (account is not null)
            {
                account.Password = newHash;
            }
        }

        await _elohimDbContext.SaveChangesAsync(cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Ok(new { mensaje = "Contraseña actualizada correctamente. Inicia sesión con tu nueva contraseña." });
    }
}

public record RecoverWithCodeRequestDto(
    string Correo,
    string Codigo,
    string NuevaContrasena);
