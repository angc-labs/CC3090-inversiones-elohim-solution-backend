using System.Threading;
using System.Threading.Tasks;
using ElohimShop.Application.Inventario.Dtos;

namespace ElohimShop.Application.Inventario;

public interface IGetInventarioUseCase
{
    Task<InventarioResponseDto> EjecutarAsync(InventarioQuery query, CancellationToken cancellationToken);
}

public class GetInventarioUseCase : IGetInventarioUseCase
{
    private readonly IInventarioRepository _repository;

    public GetInventarioUseCase(IInventarioRepository repository)
    {
        _repository = repository;
    }

    public Task<InventarioResponseDto> EjecutarAsync(InventarioQuery query, CancellationToken cancellationToken)
    {
        return _repository.GetInventarioAsync(query, cancellationToken);
    }
}
