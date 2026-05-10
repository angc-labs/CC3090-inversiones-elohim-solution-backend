using System.ComponentModel.DataAnnotations.Schema;

namespace ElohimShop.Domain.Entities;

public class Consulta
{
    public string IdConsulta { get; private set; } = Guid.NewGuid().ToString();
    public string IdCliente { get; private set; } = string.Empty;
    public string IdUsuario { get; private set; } = string.Empty;
    public DateTime FechaConsulta { get; private set; }

    [InverseProperty(nameof(Usuario.ConsultasCliente))]
    public Usuario? Cliente { get; private set; }

    [InverseProperty(nameof(Usuario.ConsultasAdministrador))]
    public Usuario? Administrador { get; private set; }
}
