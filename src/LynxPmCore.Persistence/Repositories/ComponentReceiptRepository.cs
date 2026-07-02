using LynxPmCore.Domain.Aggregates.Components;
using LynxPmCore.Domain.Repositories;
using LynxPmCore.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace LynxPmCore.Persistence.Repositories;

internal sealed class ComponentReceiptRepository(LynxPmDbContext db) : IComponentReceiptRepository
{
    public async Task<ComponentReceipt?> FindDuplicateAsync(
        string componentId,
        DateTime receivedAt,
        string receivedBy,
        CancellationToken ct = default)
        => await db.ComponentReceipts
            .FirstOrDefaultAsync(r =>
                r.ComponentId == componentId &&
                r.ReceivedAt == receivedAt &&
                r.ReceivedBy == receivedBy &&
                !r.IsDeleted, ct);

    public async Task AddAsync(ComponentReceipt receipt, CancellationToken ct = default)
        => await db.ComponentReceipts.AddAsync(receipt, ct);
}
