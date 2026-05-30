using Asp.Versioning;
using LynxPmCore.Application.Features.Sync.Commands.TriggerFullSync;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LynxPmCore.Api.Controllers.v1;

[ApiController]
[ApiVersion(1)]
[Route("api/v{version:apiVersion}/sync")]
[Authorize]
public sealed class SynchronizationController(ISender sender) : ControllerBase
{
    [HttpPost("full")]
    public async Task<IActionResult> TriggerFull(CancellationToken ct)
    {
        var result = await sender.Send(new TriggerFullSyncCommand(), ct);
        return result.IsSuccess ? Accepted() : BadRequest(result.Error);
    }
}
