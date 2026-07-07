using LynxPmCore.Domain.Enums;
using LynxPmCore.Domain.Events;
using LynxPmCore.Domain.Primitives;
using LynxPmCore.Shared.Common;

namespace LynxPmCore.Domain.Aggregates.Notices;

public sealed class Operation : IntEntity
{
    private readonly List<OperationPart> _parts = [];
    private Operation() { }

    public int NoticeId { get; private set; }
    public string Code { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public OperationType Type { get; private set; }
    public OperationStatus Status { get; private set; }
    public int Position { get; private set; }
    public DateTime? StartedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public string? Notes { get; private set; }
    public string? ScannedEquipmentCode { get; private set; }
    public bool PhotoConfirmed { get; private set; }
    public string? AssignedTechnician { get; private set; }

    // Falla: la registra el técnico al completar o notificar la operación (diagnóstico).
    public string? Failure { get; private set; }

    // Partes objeto reportadas al notificar la operación (varias por notificación).
    public IReadOnlyList<OperationPart> Parts => _parts;

    public static Operation Create(
        int noticeId,
        string code,
        string description,
        OperationType type,
        int position,
        string? assignedTechnician = null)
    {
        return new Operation
        {
            NoticeId = noticeId,
            Code = code,
            Description = description,
            Type = type,
            Position = position,
            Status = OperationStatus.Pending,
            AssignedTechnician = assignedTechnician
        };
    }

    public Result Start(string? scannedEquipmentCode = null)
    {
        if (Status == OperationStatus.InProgress)
            return Result.Failure(DomainErrors.Operation.AlreadyStarted);
        if (Status == OperationStatus.Completed)
            return Result.Failure(DomainErrors.Operation.AlreadyCompleted);

        if (Type == OperationType.Relocate && string.IsNullOrWhiteSpace(scannedEquipmentCode))
            return Result.Failure(DomainErrors.Operation.RequiresEquipmentScan);

        Status = OperationStatus.InProgress;
        StartedAt = DateTime.UtcNow;
        ScannedEquipmentCode = scannedEquipmentCode;
        MarkUpdated();

        RaiseDomainEvent(new OperationStartedDomainEvent(Id, NoticeId));
        return Result.Success();
    }

    public Result Pause()
    {
        if (Type == OperationType.Relocate)
            return Result.Failure(DomainErrors.Operation.CannotPauseRelocate);
        if (Status != OperationStatus.InProgress)
            return Result.Failure(new Error("Operation.NotInProgress", "Operation is not in progress."));

        Status = OperationStatus.Paused;
        MarkUpdated();
        return Result.Success();
    }

    public Result Complete(
        string? notes,
        bool photoConfirmed = false,
        string? failure = null)
    {
        if (Status == OperationStatus.Completed)
            return Result.Failure(DomainErrors.Operation.AlreadyCompleted);

        if (Type == OperationType.Relocate && !photoConfirmed)
            return Result.Failure(DomainErrors.Operation.RequiresPhotoConfirmation);

        Status = OperationStatus.Completed;
        CompletedAt = DateTime.UtcNow;
        Notes = notes;
        PhotoConfirmed = photoConfirmed;
        Failure = failure;

        MarkUpdated();

        RaiseDomainEvent(new OperationCompletedDomainEvent(Id, NoticeId, notes));
        return Result.Success();
    }

    // Reporte intermedio mientras la operación sigue en curso -- a diferencia de
    // Complete(), no cambia Status ni CompletedAt, y se puede llamar varias veces.
    public Result Notify(string? failure)
    {
        if (Status == OperationStatus.Completed)
            return Result.Failure(DomainErrors.Operation.AlreadyCompleted);

        Failure = failure;
        MarkUpdated();

        RaiseDomainEvent(new OperationNotifiedDomainEvent(Id, NoticeId, failure));
        return Result.Success();
    }

    public void AddPart(string code, string? text)
    {
        _parts.Add(OperationPart.Create(Id, code, text));
        MarkUpdated();
    }
}

// forward reference — same namespace
file static class DomainErrors
{
    public static class Operation
    {
        public static readonly Error AlreadyStarted = new("Operation.AlreadyStarted", "Operation already started.");
        public static readonly Error AlreadyCompleted = new("Operation.AlreadyCompleted", "Operation already completed.");
        public static readonly Error CannotPauseRelocate = new("Operation.CannotPause", "Relocate operations cannot be paused.");
        public static readonly Error RequiresEquipmentScan = new("Operation.RequiresScan", "This operation requires equipment scan.");
        public static readonly Error RequiresPhotoConfirmation = new("Operation.RequiresPhoto", "Relocate operations require photo confirmation.");
    }
}
