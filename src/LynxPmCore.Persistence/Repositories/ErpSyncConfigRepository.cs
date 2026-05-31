using LynxPmCore.Domain.Aggregates.ErpSync;
using LynxPmCore.Domain.Enums;
using LynxPmCore.Domain.Repositories;
using LynxPmCore.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace LynxPmCore.Persistence.Repositories;

internal sealed class ErpSyncConfigRepository(LynxPmDbContext db) : IErpSyncConfigRepository
{
    public async Task<ErpSyncConfig?> GetAsync(string clientCode, ErpSyncProcess process, CancellationToken ct = default)
        => await db.ErpSyncConfigs
            .FirstOrDefaultAsync(c => c.ClientCode == clientCode && c.Process == process, ct);

    public async Task<IReadOnlyList<ErpSyncConfig>> GetAllByClientAsync(string clientCode, CancellationToken ct = default)
        => await db.ErpSyncConfigs
            .Where(c => c.ClientCode == clientCode)
            .OrderBy(c => c.Priority)
            .ThenBy(c => c.Process)
            .AsNoTracking()
            .ToListAsync(ct);

    public async Task AddAsync(ErpSyncConfig config, CancellationToken ct = default)
        => await db.ErpSyncConfigs.AddAsync(config, ct);

    public Task UpdateAsync(ErpSyncConfig config, CancellationToken ct = default)
    {
        db.ErpSyncConfigs.Update(config);
        return Task.CompletedTask;
    }
}
