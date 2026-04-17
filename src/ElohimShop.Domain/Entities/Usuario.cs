namespace ElohimShop.Domain.Entities;

public class Usuario
{
    private readonly List<Consulta> _consultasCliente = new();
    private readonly List<Consulta> _consultasAdministrador = new();
    private readonly List<Reservacion> _reservaciones = new();
    private readonly List<Venta> _ventas = new();

    public string Id { get; private set; } = Guid.NewGuid().ToString();
    public string Correo { get; private set; } = string.Empty;
    public string Nombre { get; private set; } = string.Empty;
    public string? Apellido { get; private set; }
    public string? Telefono { get; private set; }
    public string Contrasena { get; private set; } = string.Empty;
    public string TipoUsuario { get; private set; } = string.Empty;
    public bool Estado { get; private set; }
    public DateTime FechaCreacion { get; private set; }
    public ClientePerfil? ClientePerfil { get; private set; }
    public AdministradorPerfil? AdministradorPerfil { get; private set; }

    public IReadOnlyCollection<Consulta> ConsultasCliente => _consultasCliente.AsReadOnly();
    public IReadOnlyCollection<Consulta> ConsultasAdministrador => _consultasAdministrador.AsReadOnly();
    public IReadOnlyCollection<Reservacion> Reservaciones => _reservaciones.AsReadOnly();
    public IReadOnlyCollection<Venta> Ventas => _ventas.AsReadOnly();

    private Usuario() { }

    public static Usuario CrearCliente(
        string correo,
        string nombre,
        string contrasena,
        string tipoCliente,
        string? apellido = null,
        string? telefono = null,
        string? direccion = null)
    {
        var usuario = new Usuario
        {
            Correo = correo.Trim(),
            Nombre = nombre.Trim(),
            Contrasena = contrasena,
            Apellido = apellido?.Trim(),
            Telefono = telefono?.Trim(),
            TipoUsuario = "cliente",
            Estado = true,
            FechaCreacion = DateTime.UtcNow
        };

        usuario.ClientePerfil = new ClientePerfil
        {
            UsuarioId = usuario.Id,
            Direccion = direccion?.Trim(),
            TipoCliente = tipoCliente
        };

        return usuario;
    }

    public static Usuario CrearAdministrador(
        string correo,
        string nombre,
        string contrasena,
        string rol,
        string? apellido = null,
        string? telefono = null)
    {
        var usuario = new Usuario
        {
            Correo = correo.Trim(),
            Nombre = nombre.Trim(),
            Contrasena = contrasena,
            Apellido = apellido?.Trim(),
            Telefono = telefono?.Trim(),
            TipoUsuario = "administrador",
            Estado = true,
            FechaCreacion = DateTime.UtcNow
        };

        usuario.AdministradorPerfil = new AdministradorPerfil
        {
            UsuarioId = usuario.Id,
            Rol = rol
        };

        return usuario;
    }

    public bool VerificarContrasena(string contrasenaPlana, Func<string, string, bool> verificar)
    {
        return verificar(contrasenaPlana, Contrasena);
    }

    public void ActualizarContrasena(string nuevaContrasenaHash)
    {
        Contrasena = nuevaContrasenaHash;
    }

    public void ActualizarPerfil(string? correo, string? nombre, string? apellido, string? telefono)
    {
        if (!string.IsNullOrWhiteSpace(correo))
        {
            Correo = correo.Trim();
        }

        if (!string.IsNullOrWhiteSpace(nombre))
        {
            Nombre = nombre.Trim();
        }

        if (apellido is not null)
        {
            Apellido = string.IsNullOrWhiteSpace(apellido) ? null : apellido.Trim();
        }

        if (telefono is not null)
        {
            Telefono = string.IsNullOrWhiteSpace(telefono) ? null : telefono.Trim();
        }
    }
}
