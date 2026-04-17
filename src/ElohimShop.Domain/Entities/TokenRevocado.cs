namespace ElohimShop.Domain.Entities;

public class TokenRevocado
{
    private TokenRevocado()
    {
    }

    public string Id { get; private set; } = Guid.NewGuid().ToString();

    public string Jti { get; private set; } = string.Empty;

    public string UsuarioId { get; private set; } = string.Empty;

    public DateTime ExpiraEn { get; private set; }

    public DateTime RevocadoEn { get; private set; }

    public Usuario? Usuario { get; private set; }

    public static TokenRevocado Crear(string jti, string usuarioId, DateTime expiraEn)
    {
        return new TokenRevocado
        {
            Jti = jti,
            UsuarioId = usuarioId,
            ExpiraEn = expiraEn,
            RevocadoEn = DateTime.UtcNow
        };
    }
}
