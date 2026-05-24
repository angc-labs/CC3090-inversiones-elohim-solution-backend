using ElohimShop.Application.Admin;
using ElohimShop.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ElohimShop.Infrastructure.Admin;

public class AdminUsuarioService : IAdminUsuarioService
{
    private readonly ElohimShopDbContext _dbContext;

    public AdminUsuarioService(ElohimShopDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IEnumerable<UsuarioAdminDto>> ObtenerTodosAsync(
        string? busqueda,
        string? tipoUsuario,
        bool? estado,
        CancellationToken cancellationToken)
    {
        var query = _dbContext.Usuarios
            .AsNoTracking()
            .Include(u => u.AdministradorPerfil)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(busqueda))
        {
            var q = busqueda.Trim().ToLower();
            query = query.Where(u =>
                u.Nombre.ToLower().Contains(q) ||
                (u.Apellido != null && u.Apellido.ToLower().Contains(q)) ||
                u.Correo.ToLower().Contains(q));
        }

        if (!string.IsNullOrWhiteSpace(tipoUsuario))
        {
            query = query.Where(u => u.TipoUsuario == tipoUsuario.Trim());
        }

        if (estado.HasValue)
        {
            query = query.Where(u => u.Estado == estado.Value);
        }

        var usuarios = await query
            .OrderBy(u => u.FechaCreacion)
            .ToListAsync(cancellationToken);

        return usuarios.Select(u => new UsuarioAdminDto
        {
            Id = u.Id,
            Nombre = u.Nombre,
            Apellido = u.Apellido,
            Correo = u.Correo,
            Telefono = u.Telefono,
            TipoUsuario = u.TipoUsuario,
            Rol = u.AdministradorPerfil?.Rol,
            Estado = u.Estado,
            FechaCreacion = u.FechaCreacion
        });
    }

    public async Task<UsuarioAdminDto> CambiarEstadoAsync(
        string usuarioId,
        bool nuevoEstado,
        CancellationToken cancellationToken)
    {
        var usuario = await _dbContext.Usuarios
            .Include(u => u.AdministradorPerfil)
            .FirstOrDefaultAsync(u => u.Id == usuarioId, cancellationToken)
            ?? throw new InvalidOperationException("Usuario no encontrado.");

        usuario.CambiarEstado(nuevoEstado);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new UsuarioAdminDto
        {
            Id = usuario.Id,
            Nombre = usuario.Nombre,
            Apellido = usuario.Apellido,
            Correo = usuario.Correo,
            Telefono = usuario.Telefono,
            TipoUsuario = usuario.TipoUsuario,
            Rol = usuario.AdministradorPerfil?.Rol,
            Estado = usuario.Estado,
            FechaCreacion = usuario.FechaCreacion
        };
    }
}
