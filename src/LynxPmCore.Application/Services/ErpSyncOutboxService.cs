using LynxPmCore.Application.Common.Interfaces;
using LynxPmCore.Domain.Aggregates.ErpSync;
using LynxPmCore.Domain.Enums;
using LynxPmCore.Domain.Repositories;
using LynxPmCore.Shared.Context;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace LynxPmCore.Application.Services;

internal sealed class ErpSyncOutboxService(
    IErpSyncConfigRepository configRepo,
    IErpSyncOutboxRepository outboxRepo,
    IUnitOfWork unitOfWork,
    ILynxClientContext clientCtx,
    ILogger<ErpSyncOutboxService> logger) : IErpSyncOutboxService
{
    public async Task EnqueueAsync(ErpSyncProcess process, string entityId, object payload, CancellationToken ct = default)
    {
        var config = await configRepo.GetAsync(clientCtx.Client, process, ct);

        if (config is null || !config.IsEnabled)
        {
            logger.LogDebug("ERP sync skipped for {Process}/{Entity} — not configured or disabled", process, entityId);
            return;
        }

        var entry = ErpSyncOutboxEntry.Create(
            clientCtx.Client,
            process,
            entityId,
            JsonSerializer.Serialize(payload));

        await outboxRepo.AddAsync(entry, ct);
        await unitOfWork.SaveChangesAsync(ct);

        logger.LogInformation("ERP sync enqueued: {Process}/{Entity}", process, entityId);
    }
}
