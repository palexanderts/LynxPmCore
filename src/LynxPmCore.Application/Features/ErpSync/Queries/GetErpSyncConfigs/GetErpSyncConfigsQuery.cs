using LynxPmCore.Application.DTOs;
using LynxPmCore.Shared.Abstractions;

namespace LynxPmCore.Application.Features.ErpSync.Queries.GetErpSyncConfigs;

public sealed record GetErpSyncConfigsQuery : IQuery<IReadOnlyList<ErpSyncConfigDto>>;
