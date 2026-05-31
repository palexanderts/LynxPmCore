using LynxPmCore.Domain.Aggregates.ErpSync;

namespace LynxPmCore.Domain.Repositories;

public interface IErpSyncOutboxRepository
{
    Task AddAsync(ErpSyncOutboxEntry entry, CancellationToken ct = default);
    Task<IReadOnlyList<ErpSyncOutboxEntry>> GetPendingAsync(int batchSize = 50, CancellationToken ct = default);
    Task UpdateAsync(ErpSyncOutboxEntry entry, CancellationToken ct = default);
    Task<IReadOnlyList<ErpSyncOutboxEntry>> GetByEntityAsync(string entityId, CancellationToken ct = default);
}
