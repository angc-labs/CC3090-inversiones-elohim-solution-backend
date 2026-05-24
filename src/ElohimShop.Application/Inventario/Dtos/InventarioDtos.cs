using System;
using System.Collections.Generic;

namespace ElohimShop.Application.Inventario.Dtos;

public class InventarioCategoriaDto
{
    public string? Id { get; set; }
    public string? Nombre { get; set; }
}

public class InventarioMarcaDto
{
    public string? Id { get; set; }
    public string? Nombre { get; set; }
}

public class InventarioProductoDto
{
    public string IdProducto { get; set; } = string.Empty;
    public string CodigoProducto { get; set; } = string.Empty;
    public string NombreProducto { get; set; } = string.Empty;
    public InventarioCategoriaDto? Categoria { get; set; }
    public InventarioMarcaDto? Marca { get; set; }
    public int Precio { get; set; }
    public int StockActual { get; set; }
    public int StockMinimo { get; set; }
    public string Estado { get; set; } = string.Empty;
    public long ValorStock { get; set; }
    public bool DescuentoActivo { get; set; }
    public DateTime? FechaFinOferta { get; set; }
    public DateTime FechaVencimiento { get; set; }
    public string? ImagenPrincipal { get; set; }
}

public class InventarioResponseDto
{
    public InventarioResumenDto Resumen { get; set; } = new InventarioResumenDto();
    public IReadOnlyList<InventarioProductoDto> Productos { get; set; } = new List<InventarioProductoDto>();
    public int Total { get; set; }
    public int Pagina { get; set; }
    public int Limite { get; set; }
}

public class InventarioResumenDto
{
    public int TotalProductos { get; set; }
    public int StockNormal { get; set; }
    public int StockCritico { get; set; }
    public long ValorInventario { get; set; }
}
