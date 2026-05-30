using LynxPmCore.Application.Common.Interfaces;
using LynxPmCore.Shared.Abstractions;
using LynxPmCore.Shared.Common;
using Microsoft.Extensions.Logging;

namespace LynxPmCore.Application.Features.Sync.Commands.TriggerFullSync;

internal sealed class TriggerFullSyncCommandHandler(
    IApexServiceRouter apexRouter,
    INotificationService notificationService,
    ICurrentUserService currentUser,
    ILogger<TriggerFullSyncCommandHandler> logger) : ICommandHandler<TriggerFullSyncCommand>
{
    public async Task<Result> Handle(TriggerFullSyncCommand request, CancellationToken ct)
    {
        logger.LogInformation("Full sync triggered by {User}", currentUser.UserCode);
        await notificationService.BroadcastAsync("SyncStarted", new { Type = "Full" }, ct);

        var response = await apexRouter.CallAsync<object, object>(
            "LYNX_PM_GET_AVISOS", new { SyncType = "FULL" }, currentUser.UserCode, ct);

        await notificationService.BroadcastAsync("SyncCompleted", new { Type = "Full", Success = true }, ct);
        return Result.Success();
    }
}
