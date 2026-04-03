namespace ElohimShop.Domain.Entities;

public class Cliente
{
    private Cliente()
    {}

    public string IdCliente { get; private set; } = Guid.NewGuid().ToString();

    public string Correo { get; private set; } = string.Empty;

    public string Nombre { get; private set; } = string.Empty;

    public string? Apellido { get; private set; }

    public string? Telefono { get; private set; }

    public string Contrasena { get; private set; } = string.Empty;

    public string? Direccion { get; private set; }

    public string? TipoClienteId { get; private set; }

    public DateTime FechaRegistro { get; private set; }

    public bool EstadoCuenta { get; private set; }

    public TipoCliente? TipoCliente { get; private set; }

    public ICollection<Consulta> Consultas { get; private set; } = new List<Consulta>();

    public ICollection<Reservacion> Reservaciones { get; private set; } = new List<Reservacion>();

    public static Cliente Registrar(
        string correo,
        string nombre,
        string contrasena,
        string? apellido = null,
        string? telefono = null,
        string? direccion = null,
        string? tipoClienteId = null)
    {
        return new Cliente
        {
            Correo = correo.Trim(),
            Nombre = nombre.Trim(),
            Contrasena = contrasena,
            Apellido = apellido?.Trim(),
            Telefono = telefono?.Trim(),
            Direccion = direccion?.Trim(),
            TipoClienteId = tipoClienteId,
            FechaRegistro = DateTime.UtcNow,
            EstadoCuenta = true
        };
    }
}