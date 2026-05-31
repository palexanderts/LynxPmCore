using LynxPmCore.Application.Common.Interfaces;
using LynxPmCore.Domain.Aggregates.ErpSync;
using LynxPmCore.Domain.Repositories;
using LynxPmCore.Shared.Abstractions;
using LynxPmCore.Shared.Common;
using LynxPmCore.Shared.Context;

namespace LynxPmCore.Application.Features.ErpSync.Commands.UpdateErpSyncConfig;

internal sealed class UpdateErpSyncConfigCommandHandler(
    IErpSyncConfigRepository configRepo,
    IUnitOfWork unitOfWork,
    ILynxClientContext clientCtx) : ICommandHandler<UpdateErpSyncConfigCommand>
{
    public async Task<Result> Handle(UpdateErpSyncConfigCommand request, CancellationToken ct)
    {
        var config = await configRepo.GetAsync(clientCtx.Client, request.Process, ct);

        if (config is null)
        {
            config = ErpSyncConfig.Create(
                clientCtx.Client,
                request.Process,
                request.Process.ToString(),
                request.IsEnabled,
                request.ErpUrl,
                request.AuthHeader,
                request.RetryMax,
                request.RetryDelaySeconds,
                request.Priority);
            await configRepo.AddAsync(config, ct);
        }
        else
        {
            config.Update(
                request.IsEnabled,
                request.ErpUrl,
                request.AuthHeader,
                request.RetryMax,
                request.RetryDelaySeconds,
                request.Priority);
            await configRepo.UpdateAsync(config, ct);
        }

        await unitOfWork.SaveChangesAsync(ct);
        return Result.Success();
    }
}
