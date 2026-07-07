using LynxPmCore.Domain.Primitives;

namespace LynxPmCore.Domain.Events;

public sealed record OperationNotifiedDomainEvent(
    Guid Id,
    DateTime OccurredOnUtc,
    int OperationId,
    int NoticeId,
    string? Failure) : DomainEvent(Id, OccurredOnUtc)
{
    public OperationNotifiedDomainEvent(int operationId, int noticeId, string? failure)
        : this(Guid.NewGuid(), DateTime.UtcNow, operationId, noticeId, failure) { }
}
