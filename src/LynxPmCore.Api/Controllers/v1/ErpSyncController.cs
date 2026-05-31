using Asp.Versioning;
using LynxPmCore.Api.Extensions;
using LynxPmCore.Application.Features.ErpSync.Commands.UpdateErpSyncConfig;
using LynxPmCore.Application.Features.ErpSync.Queries.GetErpSyncConfigs;
using LynxPmCore.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LynxPmCore.Api.Controllers.v1;

[ApiController]
[ApiVersion(1)]
[Route("api/v{version:apiVersion}/erp-sync")]
[Authorize]
public sealed class ErpSyncController(ISender sender) : ControllerBase
{
    /// <summary>Devuelve la configuración ERP de todos los procesos para el cliente actual.</summary>
    [HttpGet("config")]
    public async Task<IActionResult> GetConfigs(CancellationToken ct)
    {
        var result = await sender.Send(new GetErpSyncConfigsQuery(), ct);
        return result.ToActionResult(this);
    }

    /// <summary>Crea o actualiza la configuración de un proceso específico.</summary>
    [HttpPut("config/{process}")]
    public async Task<IActionResult> UpdateConfig(
        ErpSyncProcess process,
        [FromBody] UpdateErpSyncConfigRequest request,
        CancellationToken ct)
    {
        var command = new UpdateErpSyncConfigCommand(
            process,
            request.IsEnabled,
            request.ErpUrl,
            request.AuthHeader,
            request.RetryMax,
            request.RetryDelaySeconds,
            request.Priority);

        var result = await sender.Send(command, ct);
        return result.ToActionResult(this);
    }
}

public sealed record UpdateErpSyncConfigRequest(
    bool IsEnabled,
    string? ErpUrl,
    string? AuthHeader,
    int RetryMax = 3,
    int RetryDelaySeconds = 60,
    int Priority = 10);
