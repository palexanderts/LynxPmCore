using Asp.Versioning;
using LynxPmCore.Application.Features.Equipments.Queries.GetEquipmentHistory;
using LynxPmCore.Application.Features.Equipments.Queries.GetEquipments;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LynxPmCore.Api.Controllers.v1;

[ApiController]
[ApiVersion(1)]
[Route("api/v{version:apiVersion}/equipments")]
[Authorize]
public sealed class EquipmentsController(ISender sender) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        [FromQuery] string? search = null,
        [FromQuery] string? customer = null,
        CancellationToken ct = default)
    {
        var result = await sender.Send(new GetEquipmentsQuery(page, pageSize, search, customer), ct);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }

    [HttpGet("{code}/history")]
    public async Task<IActionResult> GetHistory(string code, CancellationToken ct)
    {
        var result = await sender.Send(new GetEquipmentHistoryQuery(code), ct);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }
}
