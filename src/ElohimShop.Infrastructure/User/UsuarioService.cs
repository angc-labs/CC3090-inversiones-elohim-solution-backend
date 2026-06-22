using ElohimShop.Application.Usuario;
using ElohimShop.Infrastructure.Persistence;
using ElohimShop.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;

namespace ElohimShop.Infrastructure.User;

public class UsuarioService : IUsuarioService
{
    private readonly PlatformDbContext _dbContext;
    private readonly IPasswordHashing _passwordHashing;

    public UsuarioService(PlatformDbContext dbContext, IPasswordHashing passwordHashing)
    {
        _dbContext = dbContext;
        _passwordHashing = passwordHashing;
    }

    public async Task<UsuarioPerfilDto?> ObtenerPerfilAsync(string usuarioId, CancellationToken cancellationToken)
    {
        var user = await _dbContext.Users
            .IgnoreQueryFilters()
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == usuarioId, cancellationToken);

        if (user is null)
        {
            return null;
        }

        var nameParts = user.Name.Split(new[] { ' ' }, 2);
        var nombre = nameParts[0];
        var apellido = nameParts.Length > 1 ? nameParts[1] : string.Empty;

        return new UsuarioPerfilDto
        {
            UsuarioId = user.Id,
            Nombre = nombre,
            Apellido = apellido,
            Correo = user.Email,
            Telefono = user.Telefono,
            TipoUsuario = user.TipoUsuario == "staff" ? "administrador" : "cliente",
            TipoCliente = user.TipoUsuario == "cliente" ? "particular" : null,
            Direccion = null,
            Rol = user.RolStaff,
            FechaRegistro = user.CreatedAt
        };
    }

    public async Task<PerfilActualizadoDto> ActualizarPerfilAsync(string usuarioId, ActualizarPerfilDto dto, CancellationToken cancellationToken)
    {
        var user = await _dbContext.Users
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Id == usuarioId, cancellationToken);

        if (user is null)
        {
            throw new InvalidOperationException("Usuario no encontrado.");
        }

        string? nuevoCorreo = null;
        if (!string.IsNullOrWhiteSpace(dto.Correo) && dto.Correo != user.Email)
        {
            var correoExiste = await _dbContext.Users
                .IgnoreQueryFilters()
                .AnyAsync(u => u.Email == dto.Correo.Trim() && u.Id != usuarioId, cancellationToken);

            if (correoExiste)
            {
                throw new InvalidOperationException("El correo ya está en uso por otra cuenta.");
            }

            nuevoCorreo = dto.Correo.Trim();
        }

        user.Name = $"{dto.Nombre} {dto.Apellido}".Trim();
        if (nuevoCorreo != null)
        {
            user.Email = nuevoCorreo;
        }
        user.Telefono = dto.Telefono;
        user.UpdatedAt = DateTime.UtcNow;

        if (!string.IsNullOrWhiteSpace(dto.Contrasena))
        {
            var hashedPassword = ElohimShop.Infrastructure.Security.PasswordHashing.Hash(dto.Contrasena);
            var account = await _dbContext.Accounts
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(a => a.UserId == usuarioId && a.ProviderId == "credential", cancellationToken);

            if (account is null)
            {
                account = new ElohimShop.Domain.Platform.Account
                {
                    Id = Guid.NewGuid().ToString(),
                    UserId = usuarioId,
                    ProviderId = "credential",
                    AccountId = user.Email,
                    Password = hashedPassword,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                _dbContext.Accounts.Add(account);
            }
            else
            {
                account.Password = hashedPassword;
                account.AccountId = user.Email;
                account.UpdatedAt = DateTime.UtcNow;
            }
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        var nameParts = user.Name.Split(new[] { ' ' }, 2);
        var nombreRes = nameParts[0];
        var apellidoRes = nameParts.Length > 1 ? nameParts[1] : string.Empty;

        return new PerfilActualizadoDto
        {
            UsuarioId = user.Id,
            Nombre = nombreRes,
            Apellido = apellidoRes,
            Correo = user.Email,
            Telefono = user.Telefono
        };
    }
}
