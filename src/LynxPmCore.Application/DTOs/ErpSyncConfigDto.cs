using LynxPmCore.Domain.Enums;

namespace LynxPmCore.Application.DTOs;

public sealed class ErpSyncConfigDto
{
    public Guid Id { get; set; }
    public string ClientCode { get; set; } = string.Empty;
    public ErpSyncProcess Process { get; set; }
    public string ProcessName { get; set; } = string.Empty;
    public bool IsEnabled { get; set; }
    public string? ErpUrl { get; set; }
    public int RetryMax { get; set; }
    public int RetryDelaySeconds { get; set; }
    public int Priority { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public sealed class ErpSyncOutboxDto
{
    public Guid Id { get; set; }
    public ErpSyncProcess Process { get; set; }
    public string EntityId { get; set; } = string.Empty;
    public ErpSyncStatus Status { get; set; }
    public int AttemptCount { get; set; }
    public string? LastError { get; set; }
    public DateTime ScheduledAt { get; set; }
    public DateTime? ProcessedAt { get; set; }
    public DateTime CreatedAt { get; set; }
}
