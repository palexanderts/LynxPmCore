using LynxPmCore.Application.Common.Interfaces;
using LynxPmCore.Domain.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Text;

namespace LynxPmCore.BackgroundJobs.Jobs;

public sealed class ErpSynchronizationJob(
    IServiceScopeFactory scopeFactory,
    IHttpClientFactory httpClientFactory,
    ILogger<ErpSynchronizationJob> logger)
{
    public async Task ExecuteAsync()
    {
        using var scope = scopeFactory.CreateScope();
        var outboxRepo = scope.ServiceProvider.GetRequiredService<IErpSyncOutboxRepository>();
        var configRepo = scope.ServiceProvider.GetRequiredService<IErpSyncConfigRepository>();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        var pending = await outboxRepo.GetPendingAsync(batchSize: 50);
        if (pending.Count == 0) return;

        logger.LogInformation("ERP sync job: processing {Count} pending entries", pending.Count);

        // Ordenar por prioridad de config (menor número = mayor prioridad)
        var configCache = new Dictionary<(string, int), Domain.Aggregates.ErpSync.ErpSyncConfig?>();

        foreach (var entry in pending)
        {
            var cacheKey = (entry.ClientCode, (int)entry.Process);
            if (!configCache.TryGetValue(cacheKey, out var config))
            {
                config = await configRepo.GetAsync(entry.ClientCode, entry.Process);
                configCache[cacheKey] = config;
            }

            if (config is null || !config.IsEnabled || string.IsNullOrWhiteSpace(config.ErpUrl))
            {
                logger.LogDebug("ERP sync skipped for {Process}/{Entity} — disabled or no URL", entry.Process, entry.EntityId);
                continue;
            }

            entry.StartProcessing();
            await outboxRepo.UpdateAsync(entry);
            await unitOfWork.SaveChangesAsync();

            try
            {
                var http = httpClientFactory.CreateClient("ErpClient");
                var request = new HttpRequestMessage(HttpMethod.Post, config.ErpUrl)
                {
                    Content = new StringContent(entry.Payload, Encoding.UTF8, "application/json")
                };

                if (!string.IsNullOrWhiteSpace(config.AuthHeader))
                    request.Headers.TryAddWithoutValidation("Authorization", config.AuthHeader);

                var response = await http.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    entry.MarkCompleted();
                    logger.LogInformation("ERP sync completed: {Process}/{Entity}", entry.Process, entry.EntityId);
                }
                else
                {
                    var body = await response.Content.ReadAsStringAsync();
                    entry.MarkFailed($"HTTP {(int)response.StatusCode}: {body}", config.RetryDelaySeconds, config.RetryMax);
                    logger.LogWarning("ERP sync failed: {Process}/{Entity} — {Status}", entry.Process, entry.EntityId, response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                entry.MarkFailed(ex.Message, config.RetryDelaySeconds, config.RetryMax);
                logger.LogError(ex, "ERP sync error: {Process}/{Entity}", entry.Process, entry.EntityId);
            }

            await outboxRepo.UpdateAsync(entry);
            await unitOfWork.SaveChangesAsync();
        }
    }
}
