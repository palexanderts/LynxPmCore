namespace LynxPmCore.Application.Common.Interfaces;

public interface IApexServiceRouter
{
    Task<TResponse?> CallAsync<TRequest, TResponse>(
        string serviceName,
        TRequest request,
        string? userCode = null,
        CancellationToken ct = default)
        where TRequest : class
        where TResponse : class;
}
