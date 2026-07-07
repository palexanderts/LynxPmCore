using Hangfire;
using Hangfire.InMemory;
using LynxPmCore.BackgroundJobs.Jobs;
using Microsoft.Extensions.DependencyInjection;

namespace LynxPmCore.BackgroundJobs;

public static class DependencyInjection
{
    public static IServiceCollection AddBackgroundJobs(this IServiceCollection services)
    {
        services.AddHttpClient("ErpClient")
            .ConfigureHttpClient(c => c.Timeout = TimeSpan.FromSeconds(30));

        services.AddHangfire(cfg => cfg
            .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
            .UseSimpleAssemblyNameTypeSerializer()
            .UseRecommendedSerializerSettings()
            .UseInMemoryStorage());

        services.AddHangfireServer();

        services.AddSingleton<OutboxProcessorJob>();
        services.AddSingleton<NoticeSynchronizationJob>();
        services.AddSingleton<ErpSynchronizationJob>();
        services.AddSingleton<LegacyAvisoSyncJob>();

        return services;
    }

    public static void RegisterRecurringJobs()
    {
        RecurringJob.AddOrUpdate<OutboxProcessorJob>(
            "outbox-processor",
            job => job.ExecuteAsync(),
            "*/1 * * * *");

        RecurringJob.AddOrUpdate<NoticeSynchronizationJob>(
            "notice-sync",
            job => job.ExecuteAsync(),
            "*/5 * * * *");

        RecurringJob.AddOrUpdate<ErpSynchronizationJob>(
            "erp-sync",
            job => job.ExecuteAsync(),
            "*/2 * * * *");

        RecurringJob.AddOrUpdate<LegacyAvisoSyncJob>(
            "legacy-aviso-sync",
            job => job.ExecuteAsync(),
            "*/10 * * * *");
    }
}
