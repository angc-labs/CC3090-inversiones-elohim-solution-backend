namespace ElohimShop.Domain.Entities;

public class MetodoPago
{
    public string IdMetodoPago { get; private set; } = Guid.NewGuid().ToString();
    public string NombreMetodo { get; private set; } = string.Empty;
    public string? Descripcion { get; private set; }
    public bool Activo { get; private set; } = true;
    public string? UsuarioId { get; private set; }
    public string? StripePaymentMethodId { get; private set; }
    public string? UltimosDigitos { get; private set; }
    public string? MarcaTarjeta { get; private set; }
    public int? ExpiraMes { get; private set; }
    public int? ExpiraAnio { get; private set; }
    public string? Alias { get; private set; }
    public ICollection<Reservacion> Reservaciones { get; private set; } = new List<Reservacion>();

    private MetodoPago()
    {
    }

    public static MetodoPago Crear(string? usuarioId, string nombreMetodo)
    {
        var nombre = TruncarNombreMetodo(nombreMetodo);
        return new MetodoPago
        {
            UsuarioId = usuarioId,
            NombreMetodo = nombre,
            Activo = true
        };
    }

    public static MetodoPago CrearDesdeStripe(
        string usuarioId,
        string stripePaymentMethodId,
        string nombreMetodo,
        string? descripcion,
        string marcaTarjeta,
        string ultimosDigitos,
        int expiraMes,
        int expiraAnio,
        string? alias)
    {
        return new MetodoPago
        {
            UsuarioId = usuarioId,
            NombreMetodo = TruncarNombreMetodo(string.IsNullOrWhiteSpace(nombreMetodo) ? "Tarjeta" : nombreMetodo),
            Descripcion = descripcion,
            Activo = true,
            StripePaymentMethodId = stripePaymentMethodId,
            MarcaTarjeta = marcaTarjeta,
            UltimosDigitos = ultimosDigitos,
            ExpiraMes = expiraMes,
            ExpiraAnio = expiraAnio,
            Alias = alias
        };
    }

    public void Desactivar()
    {
        Activo = false;
    }

    private static string TruncarNombreMetodo(string nombreMetodo)
    {
        var n = nombreMetodo.Trim();
        return n.Length <= 15 ? n : n[..15];
    }
}