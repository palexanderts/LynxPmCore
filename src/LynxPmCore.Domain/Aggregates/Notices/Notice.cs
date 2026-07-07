using LynxPmCore.Domain.Enums;
using LynxPmCore.Domain.Events;
using LynxPmCore.Domain.Primitives;
using LynxPmCore.Domain.ValueObjects;
using LynxPmCore.Shared.Common;

namespace LynxPmCore.Domain.Aggregates.Notices;

public sealed class Notice : IntAggregateRoot
{
    private readonly List<Operation> _operations = [];
    private readonly List<NoticeCause> _causes = [];
    private Notice() { }

    public string Number { get; private set; } = string.Empty;
    public string EquipmentCode { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public NoticeStatus Status { get; private set; }
    public bool IsApproved { get; private set; }
    public bool IsSynchronized { get; private set; }
    public DateTime? SynchronizedAt { get; private set; }
    public string CreatedBy { get; private set; } = string.Empty;
    public string? ApprovedBy { get; private set; }
    public string? RejectionReason { get; private set; }
    public string? Location { get; private set; }
    public string? Customer { get; private set; }
    public string? Center { get; private set; }
    public int Priority { get; private set; }
    public string? PriorityCode { get; private set; }
    public string? PriorityText { get; private set; }
    public string? NoticeTypeCode { get; private set; }
    public string? NoticeTypeText { get; private set; }

    public NoticeApprovalStatus ApprovalStatus =>
        IsApproved ? NoticeApprovalStatus.Approved :
        RejectionReason is not null ? NoticeApprovalStatus.Rejected :
        NoticeApprovalStatus.Pending;

    public IReadOnlyList<Operation> Operations => _operations;
    public IReadOnlyList<NoticeCause> Causes => _causes;

    public static Result<Notice> Create(
        string number,
        string equipmentCode,
        string description,
        string createdBy,
        string? location = null,
        string? customer = null,
        int priority = 1,
        string? priorityCode = null,
        string? priorityText = null,
        string? noticeTypeCode = null,
        string? noticeTypeText = null,
        string? center = null)
    {
        var notice = new Notice
        {
            Number = number.Trim().ToUpperInvariant(),
            EquipmentCode = equipmentCode.Trim().ToUpperInvariant(),
            Description = description,
            Status = NoticeStatus.Open,
            CreatedBy = createdBy,
            Location = location,
            Customer = customer,
            Priority = priority,
            PriorityCode = priorityCode,
            PriorityText = priorityText,
            NoticeTypeCode = noticeTypeCode,
            NoticeTypeText = noticeTypeText,
            Center = center
        };

        notice.RaiseDomainEvent(new NoticeCreatedDomainEvent(notice.Id, notice.Number, notice.EquipmentCode));
        return Result.Success(notice);
    }

    public Result ChangeStatus(NoticeStatus newStatus)
    {
        if (Status == NoticeStatus.Closed)
            return Result.Failure(Errors.Notice.AlreadyClosed);

        var oldStatus = Status;
        Status = newStatus;
        MarkUpdated();
        RaiseDomainEvent(new NoticeStatusChangedDomainEvent(Id, oldStatus, newStatus));
        return Result.Success();
    }

    public Result Approve(string approvedBy)
    {
        if (IsApproved)
            return Result.Failure(Errors.Notice.AlreadyApproved);
        if (Status == NoticeStatus.Closed || Status == NoticeStatus.Cancelled)
            return Result.Failure(Errors.Notice.CannotModifyWhenClosed);

        IsApproved = true;
        ApprovedBy = approvedBy;
        RejectionReason = null;
        MarkUpdated();
        RaiseDomainEvent(new NoticeApprovalChangedDomainEvent(Id, NoticeApprovalStatus.Approved, approvedBy, null));
        return Result.Success();
    }

    public Result Reject(string rejectedBy, string reason)
    {
        if (Status == NoticeStatus.Closed || Status == NoticeStatus.Cancelled)
            return Result.Failure(Errors.Notice.CannotModifyWhenClosed);

        IsApproved = false;
        ApprovedBy = null;
        RejectionReason = reason;
        MarkUpdated();
        RaiseDomainEvent(new NoticeApprovalChangedDomainEvent(Id, NoticeApprovalStatus.Rejected, rejectedBy, reason));
        return Result.Success();
    }

    public Result AddOperation(Operation operation)
    {
        if (Status == NoticeStatus.Closed || Status == NoticeStatus.Cancelled)
            return Result.Failure(Errors.Notice.CannotModifyWhenClosed);

        _operations.Add(operation);
        MarkUpdated();
        return Result.Success();
    }

    // Usado solo por NoticeRepository: AVISOID en LYNX_PM_AVISO_OPERATIONS es
    // VARCHAR2 sin FK real y con valores históricos inconsistentes (no siempre
    // numéricos) — un Include() vía join fuerza a Oracle a convertir toda la
    // columna a número y truena con esas filas. Se hidrata Operations con una
    // consulta separada (columna = parámetro, sin join) en vez de Include().
    public void HydrateOperations(IReadOnlyList<Operation> operations)
    {
        _operations.Clear();
        _operations.AddRange(operations);
    }

    public void MarkSynchronized()
    {
        IsSynchronized = true;
        SynchronizedAt = DateTime.UtcNow;
        MarkUpdated();
    }

    public void AddCause(string code, string? text)
    {
        _causes.Add(NoticeCause.Create(Id, code, text));
        MarkUpdated();
    }
}

file static class Errors
{
    public static class Notice
    {
        public static readonly Error AlreadyClosed = new("Notice.AlreadyClosed", "Notice is already closed.");
        public static readonly Error AlreadyApproved = new("Notice.AlreadyApproved", "Notice is already approved.");
        public static readonly Error CannotModifyWhenClosed = new("Notice.CannotModify", "Notice cannot be modified when closed or cancelled.");
    }
}
