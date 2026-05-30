using LynxPmCore.Application.Common.Interfaces;
using LynxPmCore.Integration.OrdsClient;
using Microsoft.Extensions.Logging;

namespace LynxPmCore.Integration.ApexRouter;

internal sealed class ApexServiceRouter(
    IOrdsClient ordsClient,
    ILogger<ApexServiceRouter> logger) : IApexServiceRouter
{
    public async Task<TResponse?> CallAsync<TRequest, TResponse>(
        string serviceName,
        TRequest request,
        string? userCode = null,
        CancellationToken ct = default)
        where TRequest : class
        where TResponse : class
    {
        logger.LogInformation("Routing to APEX service: {Service}", serviceName);
        return await ordsClient.PostAsync<TRequest, TResponse>(serviceName, request, userCode, ct);
    }
}
