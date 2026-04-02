namespace ElohimShop.Domain.Entities;

public class MetodoPago
{
    public string IdMetodoPago { get; private set; } = Guid.NewGuid().ToString();
    public string NombreMetodo { get; private set; } = string.Empty;
    public string? Descripcion { get; private set; }
    public bool Activo { get; private set; }
    public ICollection<Reservacion> Reservaciones { get; private set; } = new List<Reservacion>();
}