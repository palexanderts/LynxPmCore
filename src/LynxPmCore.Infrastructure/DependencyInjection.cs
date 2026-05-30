using System.Text;
using LynxPmCore.Application.Common.Interfaces;
using LynxPmCore.Infrastructure.Authentication;
using LynxPmCore.Infrastructure.Caching;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using StackExchange.Redis;

namespace LynxPmCore.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, CurrentUserService>();

        // JWT
        var jwtSection = config.GetSection(JwtOptions.SectionName);
        services.Configure<JwtOptions>(jwtSection);
        services.AddSingleton<JwtService>();

        var jwtOpts = jwtSection.Get<JwtOptions>()!;
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(opts =>
            {
                opts.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtOpts.Issuer,
                    ValidAudience = jwtOpts.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwtOpts.SecretKey))
                };
            });
        services.AddAuthorization();

        // Redis (optional - graceful fallback)
        var redisConn = config.GetConnectionString("Redis");
        if (!string.IsNullOrWhiteSpace(redisConn))
        {
            services.AddSingleton<IConnectionMultiplexer>(
                ConnectionMultiplexer.Connect(redisConn));
            services.AddSingleton<ICacheService, RedisCacheService>();
        }

        // OpenTelemetry
        services.AddOpenTelemetry()
            .WithTracing(b => b
                .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("LynxPmCore"))
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation())
            .WithMetrics(b => b
                .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("LynxPmCore"))
                .AddAspNetCoreInstrumentation());

        return services;
    }

    public static void AddSerilog(this IServiceCollection services, IConfiguration config)
    {
        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(config)
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .WriteTo.File("logs/lynxpm-.log", rollingInterval: RollingInterval.Day)
            .CreateLogger();

        services.AddSerilog();
    }
}
