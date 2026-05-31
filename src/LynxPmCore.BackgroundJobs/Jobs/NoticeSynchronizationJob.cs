using LynxPmCore.Domain.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace LynxPmCore.BackgroundJobs.Jobs;

// ERP sync pendiente — este job marcará los avisos como sincronizados
// cuando se defina la integración con el sistema ERP correspondiente.
public sealed class NoticeSynchronizationJob(
    IServiceScopeFactory scopeFactory,
    ILogger<NoticeSynchronizationJob> logger)
{
    public async Task ExecuteAsync()
    {
        logger.LogInformation("Notice sync job running — ERP integration pending");
        using var scope = scopeFactory.CreateScope();
        var noticeRepo = scope.ServiceProvider.GetRequiredService<INoticeRepository>();

        var pending = await noticeRepo.GetNotSynchronizedAsync();
        logger.LogInformation("Found {Count} notices pending ERP sync", pending.Count);

        await Task.CompletedTask;
    }
}
