namespace ElohimShop.Domain.Entities;

public class Consulta
{
    public string IdConsulta { get; private set; } = Guid.NewGuid().ToString();
    public string IdCliente { get; private set; } = string.Empty;
    public string IdUsuario { get; private set; } = string.Empty;
    public DateTime FechaConsulta { get; private set; }
    public Cliente? Cliente { get; private set; }
    public Administrador? Administrador { get; private set; }
}