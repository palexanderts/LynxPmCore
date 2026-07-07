using Asp.Versioning;
using LynxPmCore.Api.Extensions;
using LynxPmCore.Application.Features.Notices.Commands.ApproveNotice;
using LynxPmCore.Application.Features.Notices.Commands.ChangeNoticeStatus;
using LynxPmCore.Application.Features.Notices.Commands.CompleteOperation;
using LynxPmCore.Application.Features.Notices.Commands.CreateNotice;
using LynxPmCore.Application.Features.Notices.Commands.RejectNotice;
using LynxPmCore.Application.Features.Notices.Commands.StartOperation;
using LynxPmCore.Application.Features.Notices.Commands.SynchronizeNotice;
using LynxPmCore.Application.Features.Notices.Queries.GetNoticeById;
using LynxPmCore.Application.Features.Notices.Queries.GetNotices;
using LynxPmCore.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

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

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
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

    [HttpPatch("{id:int}/status")]
    public async Task<IActionResult> ChangeStatus(int id, [FromBody] ChangeNoticeStatusRequest request, CancellationToken ct)
    {
        var result = await sender.Send(new ChangeNoticeStatusCommand(id, request.NewStatus), ct);
        return result.ToActionResult(this);
    }

    [HttpPost("{id:int}/approve")]
    public async Task<IActionResult> Approve(int id, [FromBody] ApproveNoticeRequest request, CancellationToken ct)
    {
        var result = await sender.Send(new ApproveNoticeCommand(id, request.ApprovedBy), ct);
        return result.ToActionResult(this);
    }

    [HttpPost("{id:int}/reject")]
    public async Task<IActionResult> Reject(int id, [FromBody] RejectNoticeRequest request, CancellationToken ct)
    {
        var result = await sender.Send(new RejectNoticeCommand(id, request.RejectedBy, request.Reason), ct);
        return result.ToActionResult(this);
    }

    [HttpPost("{id:int}/sync")]
    public async Task<IActionResult> Synchronize(int id, CancellationToken ct)
    {
        var result = await sender.Send(new SynchronizeNoticeCommand(id), ct);
        return result.IsSuccess ? Ok() : result.Error.Code.EndsWith(".NotFound") ? NotFound(result.Error) : BadRequest(result.Error);
    }

    [HttpPost("{noticeId:int}/operations/{operationId:int}/start")]
    public async Task<IActionResult> StartOperation(
        int noticeId, int operationId,
        [FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] StartOperationRequest? request,
        CancellationToken ct)
    {
        var result = await sender.Send(
            new StartOperationCommand(noticeId, operationId, request?.ScannedEquipmentCode), ct);
        return result.ToActionResult(this);
    }

    [HttpPost("{noticeId:int}/operations/{operationId:int}/complete")]
    public async Task<IActionResult> CompleteOperation(
        int noticeId, int operationId,
        [FromBody] CompleteOperationRequest request,
        CancellationToken ct)
    {
        var result = await sender.Send(
            new CompleteOperationCommand(
                noticeId,
                operationId,
                request.Notes,
                request.PhotoConfirmed,
                request.Failure,
                request.Causes?.Select(c => new CompleteOperationCauseInput(c.Code, c.Text)).ToList()),
            ct);
        return result.ToActionResult(this);
    }
}

public sealed record ApproveNoticeRequest(string ApprovedBy);
public sealed record RejectNoticeRequest(string RejectedBy, string Reason);
public sealed record ChangeNoticeStatusRequest(NoticeStatus NewStatus);
public sealed record StartOperationRequest(string? ScannedEquipmentCode);
public sealed record CompleteOperationRequest(
    string? Notes,
    bool PhotoConfirmed = false,
    string? Failure = null,
    IReadOnlyList<CompleteOperationCauseRequest>? Causes = null);
public sealed record CompleteOperationCauseRequest(string Code, string? Text);
