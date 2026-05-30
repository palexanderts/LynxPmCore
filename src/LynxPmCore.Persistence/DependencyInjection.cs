using LynxPmCore.Application.Common.Interfaces;
using LynxPmCore.Domain.Repositories;
using LynxPmCore.Persistence.Context;
using LynxPmCore.Persistence.Processors;
using LynxPmCore.Persistence.Repositories;
using LynxPmCore.Persistence.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LynxPmCore.Persistence;

public static class DependencyInjection
{
    public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration config)
    {
        var connectionString = config.GetConnectionString("Oracle")
            ?? throw new InvalidOperationException("Oracle connection string not configured.");

        services.AddDbContext<LynxPmDbContext>(opts =>
            opts.UseOracle(connectionString, o =>
            {
                o.CommandTimeout(60);
                o.UseOracleSQLCompatibility(OracleSQLCompatibility.DatabaseVersion21);
            }));

        services.AddScoped<INoticeRepository, NoticeRepository>();
        services.AddScoped<IEquipmentRepository, EquipmentRepository>();
        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<IUnitOfWork, LynxPmCore.Persistence.UnitOfWork.UnitOfWork>();
        services.AddScoped<OutboxProcessor>();

        return services;
    }
}
