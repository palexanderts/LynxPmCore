using LynxPmCore.Domain.Primitives;

namespace LynxPmCore.Domain.Events;

public sealed record OperationCompletedDomainEvent(
    Guid Id,
    DateTime OccurredOnUtc,
    int OperationId,
    int NoticeId,
    string? Notes) : DomainEvent(Id, OccurredOnUtc)
{
    public OperationCompletedDomainEvent(int operationId, int noticeId, string? notes)
        : this(Guid.NewGuid(), DateTime.UtcNow, operationId, noticeId, notes) { }
}
