namespace ElohimShop.Application.Inventario;

public record InventarioQuery(
    string? Q,
    string? CategoriaId,
    string? Estado,
    string? OrderBy,
    string? Order,
    int Page = 1,
    int Limit = 20);
