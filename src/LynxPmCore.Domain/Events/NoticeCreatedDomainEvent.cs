using LynxPmCore.Domain.Primitives;

namespace LynxPmCore.Domain.Events;

public sealed record NoticeCreatedDomainEvent(
    Guid Id,
    DateTime OccurredOnUtc,
    Guid NoticeId,
    string NoticeNumber,
    string EquipmentCode) : DomainEvent(Id, OccurredOnUtc)
{
    public NoticeCreatedDomainEvent(Guid noticeId, string noticeNumber, string equipmentCode)
        : this(Guid.NewGuid(), DateTime.UtcNow, noticeId, noticeNumber, equipmentCode) { }
}
