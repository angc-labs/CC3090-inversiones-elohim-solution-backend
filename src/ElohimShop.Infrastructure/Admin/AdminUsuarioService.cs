using ElohimShop.Application.Admin;
using ElohimShop.Domain.Entities;
using ElohimShop.Infrastructure.Persistence;
using ElohimShop.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;

namespace ElohimShop.Infrastructure.Admin;

public class AdminUsuarioService : IAdminUsuarioService
{
    private readonly ElohimShopDbContext _dbContext;

    public AdminUsuarioService(ElohimShopDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    private static UsuarioAdminDto MapToDto(Usuario u) => new()
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
    };

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

        return usuarios.Select(MapToDto);
    }

    public async Task<UsuarioAdminDto> ObtenerPorIdAsync(string id, CancellationToken cancellationToken)
    {
        var usuario = await _dbContext.Usuarios
            .AsNoTracking()
            .Include(u => u.AdministradorPerfil)
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken)
            ?? throw new InvalidOperationException("Usuario no encontrado.");

        return MapToDto(usuario);
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

        return MapToDto(usuario);
    }

    public async Task<UsuarioAdminDto> CrearAsync(CrearUsuarioAdminDto dto, CancellationToken cancellationToken)
    {
        if (dto.TipoUsuario != "empleado" && dto.TipoUsuario != "administrador")
            throw new ArgumentException("Solo se pueden crear usuarios de tipo 'empleado' o 'administrador'.");

        var correo = dto.Correo.Trim();
        if (await _dbContext.Usuarios.AnyAsync(u => u.Correo == correo, cancellationToken))
            throw new InvalidOperationException("El correo ya está registrado.");

        var hash = PasswordHashing.Hash(dto.Contrasena);

        Usuario usuario = dto.TipoUsuario == "administrador"
            ? Usuario.CrearAdministrador(correo, dto.Nombre, hash, dto.Rol ?? "administrador", dto.Apellido, dto.Telefono)
            : Usuario.CrearEmpleado(correo, dto.Nombre, hash, dto.Apellido, dto.Telefono);

        _dbContext.Usuarios.Add(usuario);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return MapToDto(usuario);
    }

    public async Task<UsuarioAdminDto> ActualizarAsync(string id, ActualizarUsuarioAdminDto dto, CancellationToken cancellationToken)
    {
        var usuario = await _dbContext.Usuarios
            .Include(u => u.AdministradorPerfil)
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken)
            ?? throw new InvalidOperationException("Usuario no encontrado.");

        if (usuario.TipoUsuario == "cliente")
            throw new InvalidOperationException("No se puede editar usuarios de tipo cliente.");

        if (!string.IsNullOrWhiteSpace(dto.Correo) && dto.Correo.Trim() != usuario.Correo)
        {
            var correoEnUso = await _dbContext.Usuarios
                .AnyAsync(u => u.Correo == dto.Correo.Trim() && u.Id != id, cancellationToken);
            if (correoEnUso)
                throw new InvalidOperationException("El correo ya está en uso.");
        }

        usuario.ActualizarPerfil(dto.Correo, dto.Nombre, dto.Apellido, dto.Telefono);

        if (dto.Rol is not null && usuario.AdministradorPerfil is not null)
            usuario.AdministradorPerfil.Rol = dto.Rol.Trim();

        await _dbContext.SaveChangesAsync(cancellationToken);

        return MapToDto(usuario);
    }

    public async Task EliminarAsync(string id, CancellationToken cancellationToken)
    {
        var usuario = await _dbContext.Usuarios
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken)
            ?? throw new InvalidOperationException("Usuario no encontrado.");

        if (usuario.TipoUsuario == "cliente")
            throw new InvalidOperationException("No se puede eliminar usuarios de tipo cliente.");

        _dbContext.Usuarios.Remove(usuario);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
