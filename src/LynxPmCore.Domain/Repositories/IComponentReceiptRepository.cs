using LynxPmCore.Domain.Aggregates.Components;

namespace LynxPmCore.Domain.Repositories;

public interface IComponentReceiptRepository
{
    Task<ComponentReceipt?> FindDuplicateAsync(string componentId, DateTime receivedAt, string receivedBy, CancellationToken ct = default);
    Task AddAsync(ComponentReceipt receipt, CancellationToken ct = default);
}
