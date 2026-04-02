namespace ElohimShop.Domain.Entities;

public class Cliente
{
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
}