using Asp.Versioning;
using LynxPmCore.Application.Features.Components.Commands.NotifyComponentConsumption;
using LynxPmCore.Application.Features.Components.Commands.ReceiveComponent;
using LynxPmCore.Application.Features.Components.Queries.GetComponentCatalog;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LynxPmCore.Api.Controllers.v1;

[ApiController]
[ApiVersion(1)]
[Route("api/v{version:apiVersion}/components")]
[Authorize]
public sealed class ComponentsController : ControllerBase
{
    private readonly ISender _sender;

    public ComponentsController(ISender sender)
    {
        _sender = sender;
    }

    public sealed record ReceiveComponentRequest(
        int Quantity,
        string? Observations,
        string ReceivedBy,
        DateTime ReceivedAt);

    public sealed record ConsumeComponentRequest(
        string? AvisoId,
        int? OperationPosition,
        int Quantity,
        string? Observations,
        string ConsumedBy);

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        [FromQuery] string? search = null,
        CancellationToken ct = default)
    {
        var result = await _sender.Send(new GetComponentCatalogQuery(page, pageSize, search), ct);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }

    [HttpPost("{componentId}/receive")]
    public async Task<IActionResult> Receive(
        string componentId,
        [FromBody] ReceiveComponentRequest request,
        CancellationToken ct)
    {
        // La validación (Quantity, ReceivedBy, ReceivedAt, ComponentId) se aplica
        // en ReceiveComponentCommandValidator vía el ValidationBehavior del pipeline.
        var command = new ReceiveComponentCommand(
            componentId,
            request.Quantity,
            request.Observations,
            request.ReceivedBy,
            request.ReceivedAt);

        var result = await _sender.Send(command, ct);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }

    [HttpPost("{componentId}/consume")]
    public async Task<IActionResult> Consume(
        string componentId,
        [FromBody] ConsumeComponentRequest request,
        CancellationToken ct)
    {
        // La validación (Quantity, ConsumedBy, ComponentId) se aplica
        // en NotifyComponentConsumptionCommandValidator vía el ValidationBehavior del pipeline.
        var command = new NotifyComponentConsumptionCommand(
            componentId,
            request.AvisoId,
            request.OperationPosition,
            request.Quantity,
            request.Observations,
            request.ConsumedBy);

        var result = await _sender.Send(command, ct);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }
}
