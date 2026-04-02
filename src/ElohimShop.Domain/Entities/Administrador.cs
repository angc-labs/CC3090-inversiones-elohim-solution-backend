using ElohimShop.Domain.Enums;

namespace ElohimShop.Domain.Entities;

public class Administrador
{
    public string IdUsuario { get; private set; } = Guid.NewGuid().ToString();
    public string Correo { get; private set; } = string.Empty;
    public string Nombre { get; private set; } = string.Empty;
    public string? Apellido { get; private set; }
    public string? Telefono { get; private set; }
    public string Contrasena { get; private set; } = string.Empty;
    public string? IdRol { get; private set; }
    public EstadoAdministrador? Estado { get; private set; }
    public DateTime FechaCreacion { get; private set; }
    public Rol? Rol { get; private set; }
    public ICollection<Consulta> Consultas { get; private set; } = new List<Consulta>();
    public ICollection<Venta> Ventas { get; private set; } = new List<Venta>();
}