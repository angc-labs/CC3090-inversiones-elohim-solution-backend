using System.Threading;
using System.Threading.Tasks;
using ElohimShop.Application.Inventario.Dtos;

namespace ElohimShop.Application.Inventario;

public interface IInventarioRepository
{
    Task<InventarioResponseDto> GetInventarioAsync(InventarioQuery query, CancellationToken cancellationToken);
}
