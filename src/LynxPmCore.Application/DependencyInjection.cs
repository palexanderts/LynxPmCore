using System.Reflection;
using FluentValidation;
using LynxPmCore.Application.Common.Behaviors;
using LynxPmCore.Application.Common.Interfaces;
using LynxPmCore.Application.Services;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace LynxPmCore.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();

        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(assembly);
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(PerformanceBehavior<,>));
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(TransactionBehavior<,>));
        });

        services.AddValidatorsFromAssembly(assembly, includeInternalTypes: true);
        services.AddAutoMapper(assembly);
        services.AddScoped<IErpSyncOutboxService, ErpSyncOutboxService>();

        return services;
    }
}
