namespace ElohimShop.Application.Products;

public record CreateProductOfferRequestDto(
    int PrecioOferta,
    DateTime? FechaFinOferta);
