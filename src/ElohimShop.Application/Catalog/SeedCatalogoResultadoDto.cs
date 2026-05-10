namespace ElohimShop.Application.Catalog;

public record SeedCatalogoResultadoDto(
    int CantidadSolicitada,
    int CategoriasCreadas,
    int MarcasCreadas,
    int ProductosCreados,
    int ProductosOmitidos);