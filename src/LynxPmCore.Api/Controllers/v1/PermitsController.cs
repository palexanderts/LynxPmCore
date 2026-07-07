using Asp.Versioning;
using LynxPmCore.Application.Features.Permits.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LynxPmCore.Api.Controllers.v1;

[ApiController]
[ApiVersion(1)]
[Route("api/v{version:apiVersion}/permits")]
[Authorize]
public sealed class PermitsController(ISender sender) : ControllerBase
{
    [HttpGet("{description}")]
    public async Task<IActionResult> HasPermit(string description, CancellationToken ct)
    {
        var result = await sender.Send(new HasPermitQuery(description), ct);
        return result.IsSuccess ? Ok(new { allowed = result.Value }) : BadRequest(result.Error);
    }
}
