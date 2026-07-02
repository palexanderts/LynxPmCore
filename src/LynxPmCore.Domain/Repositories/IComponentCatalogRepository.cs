using LynxPmCore.Domain.Aggregates.Components;

namespace LynxPmCore.Domain.Repositories;

public interface IComponentCatalogRepository
{
    Task<IReadOnlyList<ComponentStockLocation>> GetAllStockAsync(CancellationToken ct = default);
    Task<IReadOnlyList<ComponentUnit>> GetAllUnitsAsync(CancellationToken ct = default);
    Task<IReadOnlyList<ComponentMaster>> GetAllMastersAsync(CancellationToken ct = default);
}
