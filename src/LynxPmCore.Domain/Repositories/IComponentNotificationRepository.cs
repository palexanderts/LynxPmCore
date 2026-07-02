using LynxPmCore.Domain.Aggregates.Components;

namespace LynxPmCore.Domain.Repositories;

public interface IComponentNotificationRepository
{
    Task<int> GetNextIdAsync(CancellationToken ct = default);
    Task AddAsync(ComponentNotification notification, CancellationToken ct = default);
}
