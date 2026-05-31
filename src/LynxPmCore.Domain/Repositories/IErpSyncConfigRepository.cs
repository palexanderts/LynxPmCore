using LynxPmCore.Domain.Aggregates.ErpSync;
using LynxPmCore.Domain.Enums;

namespace LynxPmCore.Domain.Repositories;

public interface IErpSyncConfigRepository
{
    Task<ErpSyncConfig?> GetAsync(string clientCode, ErpSyncProcess process, CancellationToken ct = default);
    Task<IReadOnlyList<ErpSyncConfig>> GetAllByClientAsync(string clientCode, CancellationToken ct = default);
    Task AddAsync(ErpSyncConfig config, CancellationToken ct = default);
    Task UpdateAsync(ErpSyncConfig config, CancellationToken ct = default);
}
