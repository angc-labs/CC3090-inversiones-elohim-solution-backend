namespace ElohimShop.Domain.Entities;

public class CodigoRecuperacion
{
    private CodigoRecuperacion() { }

    public string Id { get; private set; } = Guid.NewGuid().ToString();
    public string UsuarioId { get; private set; } = string.Empty;
    public string CodigoHash { get; private set; } = string.Empty;
    public bool Usado { get; private set; } = false;
    public DateTime FechaCreacion { get; private set; } = DateTime.UtcNow;
    public DateTime FechaExpiracion { get; private set; }

    public static CodigoRecuperacion Crear(string usuarioId, string codigoHash, int diasValidez = 90)
    {
        return new CodigoRecuperacion
        {
            UsuarioId = usuarioId,
            CodigoHash = codigoHash,
            FechaExpiracion = DateTime.UtcNow.AddDays(diasValidez)
        };
    }

    public void Consumir()
    {
        Usado = true;
    }
}
