using ElohimShop.Application.Usuario;
using ElohimShop.Domain.Entities;
using ElohimShop.Infrastructure.Persistence;
using ElohimShop.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;

namespace ElohimShop.Infrastructure.User;

public class UsuarioService : IUsuarioService
{
    private readonly ElohimShopDbContext _dbContext;
    private readonly IPasswordHashing _passwordHashing;

    public UsuarioService(ElohimShopDbContext dbContext, IPasswordHashing passwordHashing)
    {
        _dbContext = dbContext;
        _passwordHashing = passwordHashing;
    }

    public async Task<UsuarioPerfilDto?> ObtenerPerfilAsync(string usuarioId, CancellationToken cancellationToken)
    {
        var usuario = await _dbContext.Usuarios
            .AsNoTracking()
            .Include(u => u.ClientePerfil)
            .Include(u => u.AdministradorPerfil)
            .FirstOrDefaultAsync(u => u.Id == usuarioId, cancellationToken);

        if (usuario is null)
        {
            return null;
        }

        return new UsuarioPerfilDto
        {
            UsuarioId = usuario.Id,
            Nombre = usuario.Nombre,
            Apellido = usuario.Apellido,
            Correo = usuario.Correo,
            Telefono = usuario.Telefono,
            TipoUsuario = usuario.TipoUsuario,
            TipoCliente = usuario.ClientePerfil?.TipoCliente,
            Direccion = usuario.ClientePerfil?.Direccion,
            Rol = usuario.AdministradorPerfil?.Rol,
            FechaRegistro = usuario.FechaCreacion
        };
    }

    public async Task<PerfilActualizadoDto> ActualizarPerfilAsync(string usuarioId, ActualizarPerfilDto dto, CancellationToken cancellationToken)
    {
        var usuario = await _dbContext.Usuarios
            .Include(u => u.ClientePerfil)
            .FirstOrDefaultAsync(u => u.Id == usuarioId, cancellationToken);

        if (usuario is null)
        {
            throw new InvalidOperationException("Usuario no encontrado.");
        }

        string? nuevoCorreo = null;
        if (!string.IsNullOrWhiteSpace(dto.Correo) && dto.Correo != usuario.Correo)
        {
            var correoExiste = await _dbContext.Usuarios
                .AnyAsync(u => u.Correo == dto.Correo.Trim() && u.Id != usuarioId, cancellationToken);

            if (correoExiste)
            {
                throw new InvalidOperationException("El correo ya está en uso por otra cuenta.");
            }

            nuevoCorreo = dto.Correo.Trim();
        }

        usuario.ActualizarPerfil(nuevoCorreo, dto.Nombre, dto.Apellido, dto.Telefono);

        if (usuario.ClientePerfil is not null && dto.Direccion is not null)
        {
            usuario.ClientePerfil.Direccion = string.IsNullOrWhiteSpace(dto.Direccion) ? null : dto.Direccion.Trim();
        }

        if (!string.IsNullOrWhiteSpace(dto.Contrasena))
        {
            var hashedPassword = _passwordHashing.HashPassword(dto.Contrasena);
            usuario.ActualizarContrasena(hashedPassword);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new PerfilActualizadoDto
        {
            UsuarioId = usuario.Id,
            Nombre = usuario.Nombre,
            Apellido = usuario.Apellido,
            Correo = usuario.Correo,
            Telefono = usuario.Telefono
        };
    }
}
