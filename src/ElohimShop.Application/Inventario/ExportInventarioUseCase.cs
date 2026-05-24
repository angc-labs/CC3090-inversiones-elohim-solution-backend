using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ElohimShop.Application.Inventario.Dtos;

namespace ElohimShop.Application.Inventario;

public interface IExportInventarioUseCase
{
    Task<IReadOnlyList<InventarioProductoDto>> EjecutarAsync(InventarioQuery query, CancellationToken cancellationToken);
}

public class ExportInventarioUseCase : IExportInventarioUseCase
{
    private readonly IInventarioRepository _repository;

    public ExportInventarioUseCase(IInventarioRepository repository)
    {
        _repository = repository;
    }

    public Task<IReadOnlyList<InventarioProductoDto>> EjecutarAsync(InventarioQuery query, CancellationToken cancellationToken)
    {
        return _repository.GetInventarioProductosAsync(query, cancellationToken);
    }
}
