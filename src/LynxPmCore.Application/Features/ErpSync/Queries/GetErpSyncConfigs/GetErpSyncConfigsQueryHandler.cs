using AutoMapper;
using LynxPmCore.Application.DTOs;
using LynxPmCore.Domain.Repositories;
using LynxPmCore.Shared.Abstractions;
using LynxPmCore.Shared.Common;
using LynxPmCore.Shared.Context;

namespace LynxPmCore.Application.Features.ErpSync.Queries.GetErpSyncConfigs;

internal sealed class GetErpSyncConfigsQueryHandler(
    IErpSyncConfigRepository configRepo,
    ILynxClientContext clientCtx,
    IMapper mapper) : IQueryHandler<GetErpSyncConfigsQuery, IReadOnlyList<ErpSyncConfigDto>>
{
    public async Task<Result<IReadOnlyList<ErpSyncConfigDto>>> Handle(GetErpSyncConfigsQuery request, CancellationToken ct)
    {
        var configs = await configRepo.GetAllByClientAsync(clientCtx.Client, ct);
        return Result.Success(mapper.Map<IReadOnlyList<ErpSyncConfigDto>>(configs));
    }
}
