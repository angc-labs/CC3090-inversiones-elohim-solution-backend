namespace ElohimShop.Domain.Entities;

public class TokenRevocado
{
    private TokenRevocado()
    {
    }

    public string Id { get; private set; } = Guid.NewGuid().ToString();

    public string Jti { get; private set; } = string.Empty;

    public string ClienteId { get; private set; } = string.Empty;

    public DateTime ExpiraEn { get; private set; }

    public DateTime RevocadoEn { get; private set; }

    public Cliente? Cliente { get; private set; }

    public static TokenRevocado Crear(string jti, string clienteId, DateTime expiraEn)
    {
        return new TokenRevocado
        {
            Jti = jti,
            ClienteId = clienteId,
            ExpiraEn = expiraEn,
            RevocadoEn = DateTime.UtcNow
        };
    }
}