using Asp.Versioning;
using LynxPmCore.Application.Features.Notices.Commands.ChangeNoticeStatus;
using LynxPmCore.Application.Features.Notices.Commands.CompleteOperation;
using LynxPmCore.Application.Features.Notices.Commands.CreateNotice;
using LynxPmCore.Application.Features.Notices.Commands.StartOperation;
using LynxPmCore.Application.Features.Notices.Commands.SynchronizeNotice;
using LynxPmCore.Application.Features.Notices.Queries.GetNoticeById;
using LynxPmCore.Application.Features.Notices.Queries.GetNotices;
using LynxPmCore.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LynxPmCore.Api.Controllers.v1;

[ApiController]
[ApiVersion(1)]
[Route("api/v{version:apiVersion}/notices")]
[Authorize]
public sealed class NoticesController(ISender sender) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] NoticeStatus? status = null,
        [FromQuery] string? equipmentCode = null,
        CancellationToken ct = default)
    {
        var result = await sender.Send(new GetNoticesQuery(page, pageSize, status, equipmentCode), ct);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await sender.Send(new GetNoticeByIdQuery(id), ct);
        return result.IsSuccess ? Ok(result.Value) : NotFound(result.Error);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateNoticeCommand command, CancellationToken ct)
    {
        var result = await sender.Send(command, ct);
        return result.IsSuccess
            ? CreatedAtAction(nameof(GetById), new { id = result.Value.Id }, result.Value)
            : BadRequest(result.Error);
    }

    [HttpPatch("{id:guid}/status")]
    public async Task<IActionResult> ChangeStatus(Guid id, [FromBody] ChangeNoticeStatusRequest request, CancellationToken ct)
    {
        var result = await sender.Send(new ChangeNoticeStatusCommand(id, request.NewStatus), ct);
        return result.IsSuccess ? NoContent() : BadRequest(result.Error);
    }

    [HttpPost("{id:guid}/sync")]
    public async Task<IActionResult> Synchronize(Guid id, CancellationToken ct)
    {
        var result = await sender.Send(new SynchronizeNoticeCommand(id), ct);
        return result.IsSuccess ? Ok() : BadRequest(result.Error);
    }

    [HttpPost("{noticeId:guid}/operations/{operationId:guid}/start")]
    public async Task<IActionResult> StartOperation(
        Guid noticeId, Guid operationId,
        [FromBody] StartOperationRequest request,
        CancellationToken ct)
    {
        var result = await sender.Send(
            new StartOperationCommand(noticeId, operationId, request.ScannedEquipmentCode), ct);
        return result.IsSuccess ? NoContent() : BadRequest(result.Error);
    }

    [HttpPost("{noticeId:guid}/operations/{operationId:guid}/complete")]
    public async Task<IActionResult> CompleteOperation(
        Guid noticeId, Guid operationId,
        [FromBody] CompleteOperationRequest request,
        CancellationToken ct)
    {
        var result = await sender.Send(
            new CompleteOperationCommand(noticeId, operationId, request.Notes, request.PhotoConfirmed), ct);
        return result.IsSuccess ? NoContent() : BadRequest(result.Error);
    }
}

public sealed record ChangeNoticeStatusRequest(NoticeStatus NewStatus);
public sealed record StartOperationRequest(string? ScannedEquipmentCode);
public sealed record CompleteOperationRequest(string? Notes, bool PhotoConfirmed = false);
