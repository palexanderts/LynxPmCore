using LynxPmCore.Domain.Aggregates.Components;
using LynxPmCore.Domain.Repositories;
using LynxPmCore.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace LynxPmCore.Persistence.Repositories;

internal sealed class ComponentCatalogRepository(LynxPmDbContext db) : IComponentCatalogRepository
{
    public async Task<IReadOnlyList<ComponentStockLocation>> GetAllStockAsync(CancellationToken ct = default)
        => await db.ComponentStockLocations.AsNoTracking().ToListAsync(ct);

    public async Task<IReadOnlyList<ComponentUnit>> GetAllUnitsAsync(CancellationToken ct = default)
        => await db.ComponentUnits.AsNoTracking().ToListAsync(ct);

    public async Task<IReadOnlyList<ComponentMaster>> GetAllMastersAsync(CancellationToken ct = default)
        => await db.ComponentMasters.AsNoTracking().ToListAsync(ct);
}
