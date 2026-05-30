using LynxPmCore.Domain.Enums;
using LynxPmCore.Domain.Primitives;

namespace LynxPmCore.Domain.Events;

public sealed record NoticeStatusChangedDomainEvent(
    Guid Id,
    DateTime OccurredOnUtc,
    Guid NoticeId,
    NoticeStatus OldStatus,
    NoticeStatus NewStatus) : DomainEvent(Id, OccurredOnUtc)
{
    public NoticeStatusChangedDomainEvent(Guid noticeId, NoticeStatus oldStatus, NoticeStatus newStatus)
        : this(Guid.NewGuid(), DateTime.UtcNow, noticeId, oldStatus, newStatus) { }
}
