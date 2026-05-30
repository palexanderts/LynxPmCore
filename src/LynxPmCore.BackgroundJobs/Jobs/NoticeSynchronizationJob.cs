using LynxPmCore.Application.Common.Interfaces;
using LynxPmCore.Domain.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace LynxPmCore.BackgroundJobs.Jobs;

public sealed class NoticeSynchronizationJob(
    IServiceScopeFactory scopeFactory,
    ILogger<NoticeSynchronizationJob> logger)
{
    public async Task ExecuteAsync()
    {
        logger.LogInformation("Running notice synchronization job...");
        using var scope = scopeFactory.CreateScope();
        var noticeRepo = scope.ServiceProvider.GetRequiredService<INoticeRepository>();
        var apexRouter = scope.ServiceProvider.GetRequiredService<IApexServiceRouter>();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        var pending = await noticeRepo.GetNotSynchronizedAsync();
        logger.LogInformation("Found {Count} notices to sync", pending.Count);

        foreach (var notice in pending)
        {
            try
            {
                var payload = new { notice.Number, notice.EquipmentCode, Status = notice.Status.ToString() };
                await apexRouter.CallAsync<object, object>("LYNX_PM_GO_TO_SERVER", payload, ct: default);
                notice.MarkSynchronized();
                await noticeRepo.UpdateAsync(notice);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to sync notice {Number}", notice.Number);
            }
        }

        await unitOfWork.SaveChangesAsync();
    }
}
