using LynxPmCore.Domain.Enums;
using LynxPmCore.Domain.Primitives;

namespace LynxPmCore.Domain.Aggregates.ErpSync;

public sealed class ErpSyncOutboxEntry : BaseEntity
{
    private ErpSyncOutboxEntry() { }

    public string ClientCode { get; private set; } = string.Empty;
    public ErpSyncProcess Process { get; private set; }
    public string EntityId { get; private set; } = string.Empty;
    public string Payload { get; private set; } = string.Empty;
    public ErpSyncStatus Status { get; private set; } = ErpSyncStatus.Pending;
    public int AttemptCount { get; private set; }
    public string? LastError { get; private set; }
    public DateTime ScheduledAt { get; private set; } = DateTime.UtcNow;
    public DateTime? ProcessedAt { get; private set; }

    public static ErpSyncOutboxEntry Create(
        string clientCode,
        ErpSyncProcess process,
        string entityId,
        string payload) => new()
    {
        ClientCode  = clientCode,
        Process     = process,
        EntityId    = entityId,
        Payload     = payload,
        ScheduledAt = DateTime.UtcNow
    };

    public void StartProcessing()
    {
        Status = ErpSyncStatus.Processing;
        MarkUpdated();
    }

    public void MarkCompleted()
    {
        Status      = ErpSyncStatus.Completed;
        ProcessedAt = DateTime.UtcNow;
        MarkUpdated();
    }

    public void MarkFailed(string error, int retryDelaySeconds, int retryMax)
    {
        AttemptCount++;
        LastError = error;

        if (AttemptCount >= retryMax)
        {
            Status = ErpSyncStatus.Failed;
        }
        else
        {
            Status      = ErpSyncStatus.Pending;
            ScheduledAt = DateTime.UtcNow.AddSeconds(retryDelaySeconds);
        }

        MarkUpdated();
    }
}
