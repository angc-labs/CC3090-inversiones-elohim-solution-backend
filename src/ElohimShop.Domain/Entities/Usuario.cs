namespace ElohimShop.Domain.Entities;

public class Usuario
{
    private Usuario()
    {
    }

    public string Id { get; private set; } = Guid.NewGuid().ToString();
    public string Correo { get; private set; } = string.Empty;
    public string Nombre { get; private set; } = string.Empty;
    public string? Apellido { get; private set; }
    public string? Telefono { get; private set; }
    public string Contrasena { get; private set; } = string.Empty;
    public string TipoUsuario { get; private set; } = string.Empty;
    public bool Estado { get; private set; } = true;
    public DateTime FechaCreacion { get; private set; } = DateTime.UtcNow;
    public string? StripeCustomerId { get; private set; }
    public ClientePerfil? ClientePerfil { get; private set; }
    public AdministradorPerfil? AdministradorPerfil { get; private set; }
    public ICollection<Consulta> ConsultasCliente { get; private set; } = new List<Consulta>();
    public ICollection<Consulta> ConsultasAdministrador { get; private set; } = new List<Consulta>();
    public ICollection<Reservacion> Reservaciones { get; private set; } = new List<Reservacion>();
    public ICollection<Venta> Ventas { get; private set; } = new List<Venta>();

    public static Usuario CrearCliente(
        string correo,
        string nombre,
        string contrasenaHash,
        string tipoCliente,
        string? apellido = null,
        string? telefono = null,
        string? direccion = null)
    {
        var usuario = new Usuario
        {
            Correo = correo.Trim(),
            Nombre = nombre.Trim(),
            Contrasena = contrasenaHash,
            TipoUsuario = "cliente",
            Apellido = string.IsNullOrWhiteSpace(apellido) ? null : apellido.Trim(),
            Telefono = string.IsNullOrWhiteSpace(telefono) ? null : telefono.Trim(),
            Estado = true,
            FechaCreacion = DateTime.UtcNow
        };

        usuario.ClientePerfil = new ClientePerfil
        {
            UsuarioId = usuario.Id,
            TipoCliente = tipoCliente.Trim(),
            Direccion = string.IsNullOrWhiteSpace(direccion) ? null : direccion.Trim()
        };

        return usuario;
    }

    public static Usuario CrearAdministrador(
        string correo,
        string nombre,
        string contrasenaHash,
        string rol,
        string? apellido = null,
        string? telefono = null)
    {
        var usuario = new Usuario
        {
            Correo = correo.Trim(),
            Nombre = nombre.Trim(),
            Contrasena = contrasenaHash,
            TipoUsuario = "administrador",
            Apellido = string.IsNullOrWhiteSpace(apellido) ? null : apellido.Trim(),
            Telefono = string.IsNullOrWhiteSpace(telefono) ? null : telefono.Trim(),
            Estado = true,
            FechaCreacion = DateTime.UtcNow
        };

        usuario.AdministradorPerfil = new AdministradorPerfil
        {
            UsuarioId = usuario.Id,
            Rol = rol.Trim()
        };

        return usuario;
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

    public void CambiarEstado(bool nuevoEstado)
    {
        Estado = nuevoEstado;
    }

    public void ActualizarContrasena(string contrasenaHash)
    {
        Contrasena = contrasenaHash;
    }

    public void AsignarStripeCustomerId(string stripeCustomerId)
    {
        StripeCustomerId = stripeCustomerId;
    }
}