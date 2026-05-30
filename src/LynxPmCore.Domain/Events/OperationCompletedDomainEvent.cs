using LynxPmCore.Domain.Primitives;

namespace LynxPmCore.Domain.Events;

public sealed record OperationCompletedDomainEvent(
    Guid Id,
    DateTime OccurredOnUtc,
    Guid OperationId,
    Guid NoticeId,
    string? Notes) : DomainEvent(Id, OccurredOnUtc)
{
    public OperationCompletedDomainEvent(Guid operationId, Guid noticeId, string? notes)
        : this(Guid.NewGuid(), DateTime.UtcNow, operationId, noticeId, notes) { }
}
