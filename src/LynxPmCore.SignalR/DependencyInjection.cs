using LynxPmCore.Application.Common.Interfaces;
using LynxPmCore.SignalR.Services;
using Microsoft.Extensions.DependencyInjection;

namespace LynxPmCore.SignalR;

public static class DependencyInjection
{
    public static IServiceCollection AddSignalRServices(this IServiceCollection services)
    {
        services.AddSignalR();
        services.AddScoped<INotificationService, SignalRNotificationService>();
        return services;
    }
}
