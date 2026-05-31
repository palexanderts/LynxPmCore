using LynxPmCore.Domain.Aggregates.ErpSync;
using LynxPmCore.Domain.Enums;
using LynxPmCore.Domain.Repositories;
using LynxPmCore.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace LynxPmCore.Persistence.Repositories;

internal sealed class ErpSyncOutboxRepository(LynxPmDbContext db) : IErpSyncOutboxRepository
{
    public async Task AddAsync(ErpSyncOutboxEntry entry, CancellationToken ct = default)
        => await db.ErpSyncOutbox.AddAsync(entry, ct);

    public async Task<IReadOnlyList<ErpSyncOutboxEntry>> GetPendingAsync(int batchSize = 50, CancellationToken ct = default)
        => await db.ErpSyncOutbox
            .Where(e => e.Status == ErpSyncStatus.Pending && e.ScheduledAt <= DateTime.UtcNow)
            .OrderBy(e => e.ScheduledAt)
            .Take(batchSize)
            .ToListAsync(ct);

    public Task UpdateAsync(ErpSyncOutboxEntry entry, CancellationToken ct = default)
    {
        db.ErpSyncOutbox.Update(entry);
        return Task.CompletedTask;
    }

    public async Task<IReadOnlyList<ErpSyncOutboxEntry>> GetByEntityAsync(string entityId, CancellationToken ct = default)
        => await db.ErpSyncOutbox
            .Where(e => e.EntityId == entityId)
            .OrderByDescending(e => e.CreatedAt)
            .AsNoTracking()
            .ToListAsync(ct);
}
