namespace LynxPmCore.Integration.OrdsClient;

public interface IOrdsClient
{
    Task<TResponse?> PostAsync<TRequest, TResponse>(
        string service,
        TRequest request,
        string? userCode = null,
        CancellationToken ct = default)
        where TRequest : class
        where TResponse : class;
}
