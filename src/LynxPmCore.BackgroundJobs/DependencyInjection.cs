using Hangfire;
using Hangfire.InMemory;
using LynxPmCore.BackgroundJobs.Jobs;
using Microsoft.Extensions.DependencyInjection;

namespace LynxPmCore.BackgroundJobs;

public static class DependencyInjection
{
    public static IServiceCollection AddBackgroundJobs(this IServiceCollection services)
    {
        services.AddHangfire(cfg => cfg
            .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
            .UseSimpleAssemblyNameTypeSerializer()
            .UseRecommendedSerializerSettings()
            .UseInMemoryStorage());

        services.AddHangfireServer();

        services.AddSingleton<OutboxProcessorJob>();
        services.AddSingleton<NoticeSynchronizationJob>();

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
    }
}
