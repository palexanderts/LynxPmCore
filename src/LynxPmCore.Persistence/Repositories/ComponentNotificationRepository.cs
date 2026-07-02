using LynxPmCore.Domain.Aggregates.Components;
using LynxPmCore.Domain.Repositories;
using LynxPmCore.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace LynxPmCore.Persistence.Repositories;

internal sealed class ComponentNotificationRepository(LynxPmDbContext db) : IComponentNotificationRepository
{
    // LYNX_PM_COMPONENT_NOTIFICATIONS no tiene trigger/identity para ID (tabla legacy nunca
    // conectada); se usa la secuencia NOTIFY_OPERATION_SEQ, ya presente en la BD para este fin.
    public async Task<int> GetNextIdAsync(CancellationToken ct = default)
    {
        var result = await db.Database
            .SqlQuery<decimal>($"SELECT NOTIFY_OPERATION_SEQ.NEXTVAL AS \"Value\" FROM DUAL")
            .ToListAsync(ct);
        return (int)result[0];
    }

    public async Task AddAsync(ComponentNotification notification, CancellationToken ct = default)
        => await db.ComponentNotifications.AddAsync(notification, ct);
}
