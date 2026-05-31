using LynxPmCore.Domain.Enums;
using LynxPmCore.Domain.Primitives;

namespace LynxPmCore.Domain.Aggregates.ErpSync;

public sealed class ErpSyncConfig : BaseEntity
{
    private ErpSyncConfig() { }

    public string ClientCode { get; private set; } = string.Empty;
    public ErpSyncProcess Process { get; private set; }
    public string ProcessName { get; private set; } = string.Empty;
    public bool IsEnabled { get; private set; }
    public string? ErpUrl { get; private set; }
    public string? AuthHeader { get; private set; }
    public int RetryMax { get; private set; } = 3;
    public int RetryDelaySeconds { get; private set; } = 60;
    public int Priority { get; private set; } = 10;

    public static ErpSyncConfig Create(
        string clientCode,
        ErpSyncProcess process,
        string processName,
        bool isEnabled = false,
        string? erpUrl = null,
        string? authHeader = null,
        int retryMax = 3,
        int retryDelaySeconds = 60,
        int priority = 10) => new()
    {
        ClientCode        = clientCode,
        Process           = process,
        ProcessName       = processName,
        IsEnabled         = isEnabled,
        ErpUrl            = erpUrl,
        AuthHeader        = authHeader,
        RetryMax          = retryMax,
        RetryDelaySeconds = retryDelaySeconds,
        Priority          = priority
    };

    public void Update(
        bool isEnabled,
        string? erpUrl,
        string? authHeader,
        int retryMax,
        int retryDelaySeconds,
        int priority)
    {
        IsEnabled         = isEnabled;
        ErpUrl            = erpUrl;
        AuthHeader        = authHeader;
        RetryMax          = retryMax;
        RetryDelaySeconds = retryDelaySeconds;
        Priority          = priority;
        MarkUpdated();
    }
}
