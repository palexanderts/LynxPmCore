using LynxPmCore.Domain.Enums;
using LynxPmCore.Domain.Events;
using LynxPmCore.Domain.Primitives;
using LynxPmCore.Shared.Common;

namespace LynxPmCore.Domain.Aggregates.Notices;

public sealed class Operation : BaseEntity
{
    private readonly List<OperationCause> _causes = [];
    private Operation() { }

    public Guid NoticeId { get; private set; }
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

    // Falla: la registra el técnico al completar la operación (diagnóstico).
    public string? Failure { get; private set; }

    // Causa(s): un aviso puede acumular varias causas a través de sus operaciones.
    public IReadOnlyList<OperationCause> Causes => _causes;

    // ID de la fila origen en LYNX_PM_AVISO_OPERATIONS (sistema legacy) cuando esta
    // operación fue importada por el job de sincronización — null si se creó nativamente.
    public string? LegacySourceId { get; private set; }

    public static Operation Create(
        Guid noticeId,
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

    // Import histórico desde el sistema legacy (LYNX_PM_AVISO_OPERATIONS): a diferencia de
    // Create(), el estado inicial ya se conoce (puede llegar Completed/InProgress con fechas
    // reales) — no se debe replayear vía Start()/Complete() porque esos disparan domain events
    // de negocio que no aplican a un import de datos históricos.
    public static Operation CreateFromLegacyImport(
        Guid noticeId,
        string code,
        string description,
        OperationStatus status,
        int position,
        DateTime? startedAt,
        DateTime? completedAt,
        string? assignedTechnician,
        string legacySourceId)
    {
        return new Operation
        {
            NoticeId = noticeId,
            Code = code,
            Description = description,
            Type = OperationType.Standard,
            Status = status,
            Position = position,
            StartedAt = startedAt,
            CompletedAt = completedAt,
            AssignedTechnician = assignedTechnician,
            LegacySourceId = legacySourceId
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
        string? failure = null,
        IReadOnlyList<(string Code, string? Text)>? causes = null)
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

        if (causes is not null)
        {
            foreach (var cause in causes)
            {
                _causes.Add(OperationCause.Create(Id, cause.Code, cause.Text));
            }
        }

        MarkUpdated();

        RaiseDomainEvent(new OperationCompletedDomainEvent(Id, NoticeId, notes));
        return Result.Success();
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
