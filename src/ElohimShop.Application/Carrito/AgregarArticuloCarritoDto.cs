namespace ElohimShop.Application.Carrito;

public class AgregarArticuloCarritoDto
{
    public string ProductoId { get; set; } = string.Empty;
    public int Cantidad { get; set; }
}