using ElohimShop.Application.Admin;
using ElohimShop.Domain.Entities;
using ElohimShop.Infrastructure.Persistence;
using ElohimShop.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace ElohimShop.Infrastructure.Admin;

public class AdminUsuarioService : IAdminUsuarioService
{
    private readonly ElohimShopDbContext _dbContext;
    private readonly IConfiguration _configuration;

    public AdminUsuarioService(ElohimShopDbContext dbContext, IConfiguration configuration)
    {
        _dbContext = dbContext;
        _configuration = configuration;
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

        return usuarios.Select(MapToDto);
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

    public async Task<UsuarioAdminDto> CrearAsync(
        CrearUsuarioAdminRequestDto request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Correo) ||
            string.IsNullOrWhiteSpace(request.Nombre) ||
            string.IsNullOrWhiteSpace(request.Contrasena) ||
            string.IsNullOrWhiteSpace(request.TipoUsuario))
        {
            throw new ArgumentException("Correo, nombre, contraseña y tipo de usuario son obligatorios.");
        }

        if (request.Contrasena.Length < 8)
        {
            throw new ArgumentException("La contraseña debe tener al menos 8 caracteres.");
        }

        var correo = request.Correo.Trim();
        var correoExistente = await _dbContext.Usuarios
            .AnyAsync(u => u.Correo == correo, cancellationToken);

        if (correoExistente)
        {
            throw new InvalidOperationException("El correo ya está registrado.");
        }

        var hashedPassword = PasswordHashing.Hash(request.Contrasena);
        Usuario usuario;

        if (request.TipoUsuario == "cliente")
        {
            if (string.IsNullOrWhiteSpace(request.TipoCliente))
            {
                throw new ArgumentException("El tipo de cliente es obligatorio.");
            }

            usuario = Usuario.CrearCliente(
                correo,
                request.Nombre,
                hashedPassword,
                request.TipoCliente,
                request.Apellido,
                request.Telefono,
                request.Direccion);
        }
        else if (request.TipoUsuario == "administrador")
        {
            var rol = string.IsNullOrWhiteSpace(request.Rol) ? "cajero" : request.Rol.Trim();
            if (rol is not ("cajero" or "administrador"))
            {
                throw new ArgumentException("El rol debe ser cajero o administrador.");
            }

            usuario = Usuario.CrearAdministrador(
                correo,
                request.Nombre,
                hashedPassword,
                rol,
                request.Apellido,
                request.Telefono);
        }
        else
        {
            throw new ArgumentException("Tipo de usuario no válido. Use cliente o administrador.");
        }

        _dbContext.Usuarios.Add(usuario);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return MapToDto(usuario);
    }

    public async Task<UsuarioAdminDto> CambiarRolAsync(
        string usuarioId,
        CambiarRolUsuarioRequestDto request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Rol))
        {
            throw new ArgumentException("Debe indicar el rol destino.");
        }

        var rolDestino = request.Rol.Trim().ToLowerInvariant();
        if (rolDestino is not ("cliente" or "cajero" or "administrador"))
        {
            throw new ArgumentException("Rol no válido. Use cliente, cajero o administrador.");
        }

        var usuario = await _dbContext.Usuarios
            .Include(u => u.ClientePerfil)
            .Include(u => u.AdministradorPerfil)
            .FirstOrDefaultAsync(u => u.Id == usuarioId, cancellationToken)
            ?? throw new InvalidOperationException("Usuario no encontrado.");

        if (SuperAdminHelper.IsSuperAdminEmail(
                usuario.Correo,
                _configuration["SuperAdmin:Email"]))
        {
            throw new InvalidOperationException(
                "No se puede modificar el rol del super administrador.");
        }

        if (rolDestino == "cliente")
        {
            var tipoCliente = string.IsNullOrWhiteSpace(request.TipoCliente)
                ? "particular"
                : request.TipoCliente.Trim();

            if (tipoCliente is not ("mayorista" or "minorista" or "particular"))
            {
                throw new ArgumentException("Tipo de cliente no válido.");
            }

            if (usuario.AdministradorPerfil is not null)
            {
                _dbContext.AdministradoresPerfil.Remove(usuario.AdministradorPerfil);
            }

            usuario.AsignarPerfilCliente(tipoCliente, request.Direccion);
        }
        else
        {
            var rolStaff = rolDestino == "cajero" ? "cajero" : "administrador";

            if (usuario.ClientePerfil is not null)
            {
                _dbContext.ClientesPerfil.Remove(usuario.ClientePerfil);
            }

            usuario.AsignarPerfilAdministrador(rolStaff);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return MapToDto(usuario);
    }

    private static UsuarioAdminDto MapToDto(Usuario usuario) =>
        new()
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
