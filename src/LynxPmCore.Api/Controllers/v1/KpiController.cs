using Asp.Versioning;
using LynxPmCore.Application.Features.Analytics.Queries.GetDashboardKpis;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LynxPmCore.Api.Controllers.v1;

[ApiController]
[ApiVersion(1)]
[Route("api/v{version:apiVersion}/kpi")]
[Authorize]
public sealed class KpiController(ISender sender) : ControllerBase
{
    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboard(
        [FromQuery] int? year,
        [FromQuery] int? month,
        CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        var query = new GetDashboardKpisQuery(year ?? now.Year, month ?? now.Month);
        var result = await sender.Send(query, ct);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }
}
