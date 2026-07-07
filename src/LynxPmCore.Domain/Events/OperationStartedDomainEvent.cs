using LynxPmCore.Domain.Primitives;

namespace LynxPmCore.Domain.Events;

public sealed record OperationStartedDomainEvent(
    Guid Id,
    DateTime OccurredOnUtc,
    int OperationId,
    int NoticeId) : DomainEvent(Id, OccurredOnUtc)
{
    public OperationStartedDomainEvent(int operationId, int noticeId)
        : this(Guid.NewGuid(), DateTime.UtcNow, operationId, noticeId) { }
}
