using LynxPmCore.Application.Common.Interfaces;
using LynxPmCore.Integration.ApexRouter;
using LynxPmCore.Integration.OrdsClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LynxPmCore.Integration;

public static class DependencyInjection
{
    public static IServiceCollection AddIntegration(this IServiceCollection services, IConfiguration config)
    {
        services.Configure<OrdsClientOptions>(config.GetSection(OrdsClientOptions.SectionName));

        services.AddHttpClient<IOrdsClient, LynxPmCore.Integration.OrdsClient.OrdsClient>()
            .AddStandardResilienceHandler();

        services.AddScoped<IApexServiceRouter, ApexServiceRouter>();

        return services;
    }
}
