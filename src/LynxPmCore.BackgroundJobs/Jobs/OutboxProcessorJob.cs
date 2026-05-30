using LynxPmCore.Persistence.Processors;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace LynxPmCore.BackgroundJobs.Jobs;

public sealed class OutboxProcessorJob(
    IServiceScopeFactory scopeFactory,
    ILogger<OutboxProcessorJob> logger)
{
    public async Task ExecuteAsync()
    {
        logger.LogDebug("Processing outbox messages...");
        using var scope = scopeFactory.CreateScope();
        var processor = scope.ServiceProvider.GetRequiredService<OutboxProcessor>();
        await processor.ProcessAsync();
    }
}
