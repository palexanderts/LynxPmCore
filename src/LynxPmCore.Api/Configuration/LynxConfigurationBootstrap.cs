using LynxPmCore.Infrastructure.Context;
using LynxPmCore.Shared.Context;

namespace LynxPmCore.Api.Configuration;

public static class LynxConfigurationBootstrap
{
    public const string ClientEnvVar = "LYNX_CLIENT";
    public const string EnvironmentEnvVar = "LYNX_ENVIROMENT";

    public static (string client, string environment) ReadFromEnv()
    {
        var client = Environment.GetEnvironmentVariable(ClientEnvVar)
                     ?? "default";
        var env = Environment.GetEnvironmentVariable(EnvironmentEnvVar)
                  ?? Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
                  ?? "Production";
        return (client.ToLowerInvariant(), env);
    }

    /// <summary>
    /// Adds config files in priority order (last wins):
    ///   appsettings.json
    ///   appsettings.{env}.json
    ///   appsettings.{client}.json
    ///   appsettings.{client}.{env}.json
    /// </summary>
    public static IHostApplicationBuilder AddLynxConfiguration(
        this IHostApplicationBuilder builder,
        string client,
        string environment)
    {
        var basePath = builder.Environment.ContentRootPath;

        builder.Configuration.Sources.Clear();

        builder.Configuration
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"appsettings.{environment}.json", optional: true, reloadOnChange: true)
            .AddJsonFile($"appsettings.{client}.json", optional: true, reloadOnChange: true)
            .AddJsonFile($"appsettings.{client}.{environment}.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables();

        return builder;
    }

    public static IServiceCollection AddLynxClientContext(
        this IServiceCollection services,
        string client,
        string environment)
    {
        services.AddSingleton<ILynxClientContext>(new LynxClientContext(client, environment));
        return services;
    }
}
