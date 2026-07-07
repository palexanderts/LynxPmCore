using LynxPmCore.Domain.Enums;
using LynxPmCore.Domain.Primitives;

namespace LynxPmCore.Domain.Events;

public sealed record NoticeApprovalChangedDomainEvent(
    Guid Id,
    DateTime OccurredOnUtc,
    int NoticeId,
    NoticeApprovalStatus ApprovalStatus,
    string? By,
    string? Reason) : DomainEvent(Id, OccurredOnUtc)
{
    public NoticeApprovalChangedDomainEvent(
        int noticeId, NoticeApprovalStatus status, string? by, string? reason)
        : this(Guid.NewGuid(), DateTime.UtcNow, noticeId, status, by, reason) { }
}
