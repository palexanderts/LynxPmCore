using Asp.Versioning;
using LynxPmCore.Api.Extensions;
using LynxPmCore.Application.Features.Equipments.Commands.AddEquipmentMedia;
using LynxPmCore.Application.Features.Equipments.Commands.RemoveEquipmentMedia;
using LynxPmCore.Application.Features.Equipments.Queries.GetEquipmentHistory;
using LynxPmCore.Application.Features.Equipments.Queries.GetEquipmentMedia;
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

    [HttpGet("{code}/media")]
    public async Task<IActionResult> GetMedia(string code, CancellationToken ct)
    {
        var result = await sender.Send(new GetEquipmentMediaQuery(code), ct);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }

    [HttpPost("{code}/media")]
    public async Task<IActionResult> AddMedia(string code, [FromBody] AddEquipmentMediaRequest request, CancellationToken ct)
    {
        var result = await sender.Send(
            new AddEquipmentMediaCommand(code, request.MediaType, request.Url, request.ThumbnailUrl, request.Title, request.Position), ct);
        return result.ToActionResult(this);
    }

    [HttpDelete("{code}/media/{id:guid}")]
    public async Task<IActionResult> RemoveMedia(string code, Guid id, CancellationToken ct)
    {
        var result = await sender.Send(new RemoveEquipmentMediaCommand(id), ct);
        return result.ToActionResult(this);
    }
}

public sealed record AddEquipmentMediaRequest(
    string MediaType,
    string Url,
    string? ThumbnailUrl = null,
    string? Title = null,
    int Position = 0);
