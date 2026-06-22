using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using ElohimShop.Application.Admin;
using ElohimShop.Domain.Entities;
using ElohimShop.Infrastructure.Persistence;
using ElohimShop.Infrastructure.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ElohimShop.API.Controllers;

[ApiController]
[Route("api/admin/usuarios")]
[Authorize]
public class AdminUsuariosController : ControllerBase
{
    private readonly IAdminUsuarioService _adminUsuarioService;
    private readonly IConfiguration _configuration;
    private readonly ElohimShopDbContext _dbContext;
    private readonly PlatformDbContext _platformDbContext;

    public AdminUsuariosController(
        IAdminUsuarioService adminUsuarioService,
        IConfiguration configuration,
        ElohimShopDbContext dbContext,
        PlatformDbContext platformDbContext)
    {
        _adminUsuarioService = adminUsuarioService;
        _configuration = configuration;
        _dbContext = dbContext;
        _platformDbContext = platformDbContext;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<UsuarioAdminDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ObtenerTodos(
        [FromQuery] string? busqueda,
        [FromQuery] string? tipoUsuario,
        [FromQuery] bool? estado,
        CancellationToken cancellationToken)
    {
        if (!EsAdministrador())
        {
            return Forbid();
        }

        var usuarios = await _adminUsuarioService.ObtenerTodosAsync(busqueda, tipoUsuario, estado, cancellationToken);
        return Ok(usuarios);
    }

    [HttpPost]
    [ProducesResponseType(typeof(UsuarioAdminDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Crear(
        [FromBody] CrearUsuarioAdminRequestDto request,
        CancellationToken cancellationToken)
    {
        if (!EsAdministrador())
        {
            return Forbid();
        }

        try
        {
            var usuario = await _adminUsuarioService.CrearAsync(request, cancellationToken);
            return StatusCode(StatusCodes.Status201Created, usuario);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { error = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPut("{id}/rol")]
    [ProducesResponseType(typeof(UsuarioAdminDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CambiarRol(
        string id,
        [FromBody] CambiarRolUsuarioRequestDto request,
        CancellationToken cancellationToken)
    {
        if (!EsSuperAdmin())
        {
            return Forbid();
        }

        try
        {
            var resultado = await _adminUsuarioService.CambiarRolAsync(id, request, cancellationToken);
            return Ok(resultado);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPut("{id}/estado")]
    [ProducesResponseType(typeof(UsuarioAdminDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CambiarEstado(
        string id,
        [FromBody] CambiarEstadoRequestDto request,
        CancellationToken cancellationToken)
    {
        if (!EsAdministrador())
        {
            return Forbid();
        }

        try
        {
            var resultado = await _adminUsuarioService.CambiarEstadoAsync(id, request.Estado, cancellationToken);
            return Ok(resultado);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Genera 8 códigos de recuperación para un usuario.
    /// Solo superadmin o administrador pueden invocar este endpoint.
    /// Los códigos se devuelven en texto plano UNA SOLA VEZ y se almacenan hasheados.
    /// </summary>
    [HttpPost("{id}/reset-password")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GenerarCodigosRecuperacion(
        string id,
        CancellationToken cancellationToken)
    {
        if (!EsAdministrador() && !EsSuperAdmin())
        {
            return Forbid();
        }

        var platformUser = await _platformDbContext.Users
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

        if (platformUser is null)
        {
            return NotFound(new { error = "Usuario no encontrado en la plataforma." });
        }

        // Check if user exists in ElohimShopDbContext by Email
        var elohimUser = await _dbContext.Usuarios
            .FirstOrDefaultAsync(u => u.Correo == platformUser.Email.Trim().ToLower(), cancellationToken);

        if (elohimUser is null)
        {
            // Fetch password hash from platform accounts if available
            var account = await _platformDbContext.Accounts
                .FirstOrDefaultAsync(a => a.UserId == platformUser.Id && a.ProviderId == "credential", cancellationToken);
            
            var passwordHash = account?.Password ?? string.Empty;

            if (string.Equals(platformUser.TipoUsuario, "cliente", StringComparison.OrdinalIgnoreCase))
            {
                elohimUser = Usuario.CrearCliente(
                    platformUser.Email,
                    platformUser.Name,
                    passwordHash,
                    "particular",
                    telefono: platformUser.Telefono);
            }
            else
            {
                elohimUser = Usuario.CrearAdministrador(
                    platformUser.Email,
                    platformUser.Name,
                    passwordHash,
                    platformUser.RolStaff ?? "cajero",
                    telefono: platformUser.Telefono);
            }

            _dbContext.Usuarios.Add(elohimUser);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        // Invalidar códigos anteriores del usuario
        var codigosAnteriores = await _dbContext.CodigosRecuperacion
            .Where(c => c.UsuarioId == elohimUser.Id && !c.Usado)
            .ToListAsync(cancellationToken);

        foreach (var cod in codigosAnteriores)
        {
            cod.Consumir();
        }

        // Generar 8 códigos nuevos (10 chars alfanuméricos)
        const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
        var codigosPlanos = new List<string>();

        for (int i = 0; i < 8; i++)
        {
            var bytes = RandomNumberGenerator.GetBytes(10);
            var codigo = new StringBuilder();
            foreach (var b in bytes)
            {
                codigo.Append(chars[b % chars.Length]);
            }
            var codigoPlano = codigo.ToString();
            codigosPlanos.Add(codigoPlano);

            var hash = Convert.ToBase64String(
                SHA256.HashData(Encoding.UTF8.GetBytes(codigoPlano)));

            _dbContext.CodigosRecuperacion.Add(
                CodigoRecuperacion.Crear(elohimUser.Id, hash, diasValidez: 365));
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Ok(new
        {
            usuarioId = id,
            correo = elohimUser.Correo,
            nombre = elohimUser.Nombre,
            codigos = codigosPlanos
        });
    }

    private bool EsAdministrador()
    {
        var tipoUsuario = User.FindFirstValue("tipo_usuario") ?? User.FindFirstValue("tipoUsuario");
        if (string.Equals(tipoUsuario, "administrador", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(tipoUsuario, "admin", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(tipoUsuario, "staff", StringComparison.OrdinalIgnoreCase))
        {
            var rol = User.FindFirstValue("rol") ?? User.FindFirstValue("rol_staff");
            return rol is null or "administrador" or "admin" or "superadmin";
        }
        return false;
    }

    private bool EsSuperAdmin()
    {
        var rol = User.FindFirstValue("rol") ?? User.FindFirstValue("rol_staff");
        if (string.Equals(rol, "superadmin", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (string.Equals(
                User.FindFirstValue("es_super_admin"),
                "true",
                StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        var email = User.FindFirstValue(JwtRegisteredClaimNames.Email)
            ?? User.FindFirstValue(ClaimTypes.Email);

        return SuperAdminHelper.IsSuperAdminEmail(email, _configuration["SuperAdmin:Email"]);
    }
}

public class CambiarEstadoRequestDto
{
    public bool Estado { get; init; }
}
