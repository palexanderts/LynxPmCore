using LynxPmCore.Domain.Primitives;

namespace LynxPmCore.Domain.Events;

public sealed record OperationStartedDomainEvent(
    Guid Id,
    DateTime OccurredOnUtc,
    Guid OperationId,
    Guid NoticeId) : DomainEvent(Id, OccurredOnUtc)
{
    public OperationStartedDomainEvent(Guid operationId, Guid noticeId)
        : this(Guid.NewGuid(), DateTime.UtcNow, operationId, noticeId) { }
}
