using ElohimShop.Application.Carrito;
using ElohimShop.Domain.Entities;
using ElohimShop.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ElohimShop.Infrastructure.Catalog;

public class CarritoService : ICarritoService
{
    private readonly ElohimShopDbContext _dbContext;

    public CarritoService(ElohimShopDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<CarritoDto?> ObtenerCarritoActivoAsync(string clienteId, CancellationToken ct = default)
    {
        var carrito = await _dbContext.Carritos
            .Include(c => c.Articulos)
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.ClienteId == clienteId && c.Activo, ct);

        if (carrito is null)
        {
            return null;
        }

        return new CarritoDto
        {
            CarritoId = carrito.IdCarrito,
            Items = carrito.Articulos.Select(a => new ArticuloCarritoDto
            {
                ArticuloId = a.IdArticulo,
                ProductoId = a.ProductoId,
                NombreProducto = a.NombreProducto,
                Cantidad = a.Cantidad,
                PrecioUnitario = a.PrecioUnitario,
                Subtotal = a.Subtotal
            }).ToList(),
            Total = carrito.Articulos.Sum(a => a.Subtotal)
        };
    }

    public async Task<ArticuloCarritoDto?> AgregarArticuloAsync(string clienteId, AgregarArticuloCarritoDto dto, CancellationToken ct = default)
    {
        var producto = await _dbContext.Productos
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.IdProducto == dto.ProductoId, ct);

        if (producto is null)
        {
            return null;
        }

        if (producto.StockActual < dto.Cantidad)
        {
            throw new InvalidOperationException($"Stock insuficiente. Disponible: {producto.StockActual}.");
        }

        var carrito = await _dbContext.Carritos
            .Include(c => c.Articulos)
            .FirstOrDefaultAsync(c => c.ClienteId == clienteId && c.Activo, ct);

        if (carrito is null)
        {
            carrito = Carrito.Crear(clienteId);
            _dbContext.Carritos.Add(carrito);
        }

        var articuloExistente = carrito.Articulos.FirstOrDefault(a => a.ProductoId == dto.ProductoId);

        if (articuloExistente is not null)
        {
            articuloExistente.ActualizarCantidad(articuloExistente.Cantidad + dto.Cantidad);
        }
        else
        {
            var nuevoArticulo = ArticuloCarrito.Crear(
                carrito.IdCarrito,
                dto.ProductoId,
                producto.NombreProducto,
                dto.Cantidad,
                producto.Precio);

            _dbContext.ArticulosCarrito.Add(nuevoArticulo);
            articuloExistente = nuevoArticulo;
        }

        await _dbContext.SaveChangesAsync(ct);

        return new ArticuloCarritoDto
        {
            ArticuloId = articuloExistente.IdArticulo,
            ProductoId = articuloExistente.ProductoId,
            NombreProducto = articuloExistente.NombreProducto,
            Cantidad = articuloExistente.Cantidad,
            PrecioUnitario = articuloExistente.PrecioUnitario,
            Subtotal = articuloExistente.Subtotal
        };
    }

    public async Task<ArticuloCarritoDto?> ActualizarCantidadArticuloAsync(string clienteId, string articuloId, ActualizarCantidadArticuloDto dto, CancellationToken ct = default)
    {
        var articulo = await _dbContext.ArticulosCarrito
            .Include(a => a.Carrito)
            .FirstOrDefaultAsync(a => a.IdArticulo == articuloId && a.Carrito!.ClienteId == clienteId && a.Carrito.Activo, ct);

        if (articulo is null)
        {
            return null;
        }

        if (dto.Cantidad <= 0)
        {
            _dbContext.ArticulosCarrito.Remove(articulo);
        }
        else
        {
            articulo.ActualizarCantidad(dto.Cantidad);
        }

        await _dbContext.SaveChangesAsync(ct);

        return new ArticuloCarritoDto
        {
            ArticuloId = articulo.IdArticulo,
            ProductoId = articulo.ProductoId,
            Cantidad = articulo.Cantidad,
            Subtotal = articulo.Subtotal
        };
    }

    public async Task<bool> EliminarArticuloAsync(string clienteId, string articuloId, CancellationToken ct = default)
    {
        var articulo = await _dbContext.ArticulosCarrito
            .Include(a => a.Carrito)
            .FirstOrDefaultAsync(a => a.IdArticulo == articuloId && a.Carrito!.ClienteId == clienteId && a.Carrito.Activo, ct);

        if (articulo is null)
        {
            return false;
        }

        _dbContext.ArticulosCarrito.Remove(articulo);
        await _dbContext.SaveChangesAsync(ct);

        return true;
    }
}