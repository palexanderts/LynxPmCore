using LynxPmCore.Domain.Enums;
using LynxPmCore.Shared.Abstractions;

namespace LynxPmCore.Application.Features.ErpSync.Commands.UpdateErpSyncConfig;

public sealed record UpdateErpSyncConfigCommand(
    ErpSyncProcess Process,
    bool IsEnabled,
    string? ErpUrl,
    string? AuthHeader,
    int RetryMax,
    int RetryDelaySeconds,
    int Priority) : ICommand;
