using LynxPmCore.Application.Common.Interfaces;
using LynxPmCore.Shared.Abstractions;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LynxPmCore.Application.Common.Behaviors;

public sealed class TransactionBehavior<TRequest, TResponse>(
    IUnitOfWork unitOfWork,
    ILogger<TransactionBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
    {
        if (request is not ITransactional)
            return await next();

        logger.LogDebug("Starting transaction for {RequestName}", typeof(TRequest).Name);
        await unitOfWork.BeginTransactionAsync(ct);
        try
        {
            var response = await next();
            await unitOfWork.CommitTransactionAsync(ct);
            return response;
        }
        catch
        {
            await unitOfWork.RollbackTransactionAsync(ct);
            throw;
        }
    }
}
