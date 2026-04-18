namespace ElohimShop.Application.Carrito;

public class CarritoDto
{
    public string CarritoId { get; set; } = string.Empty;
    public List<ArticuloCarritoDto> Items { get; set; } = new();
    public decimal Total { get; set; }
}

public class ArticuloCarritoDto
{
    public string ArticuloId { get; set; } = string.Empty;
    public string ProductoId { get; set; } = string.Empty;
    public string NombreProducto { get; set; } = string.Empty;
    public int Cantidad { get; set; }
    public int PrecioUnitario { get; set; }
    public decimal Subtotal { get; set; }
}