using System.ComponentModel;
using System.Text.Json;
using LynxPmCore.Application.Features.Notices.Commands.ChangeNoticeStatus;
using LynxPmCore.Application.Features.Notices.Commands.CreateNotice;
using LynxPmCore.Application.Features.Notices.Queries.GetNoticeById;
using LynxPmCore.Application.Features.Notices.Queries.GetNotices;
using LynxPmCore.Domain.Enums;
using MediatR;
using ModelContextProtocol.Server;

namespace LynxPmCore.Mcp.Tools;

[McpServerToolType]
public sealed class NoticeTools(ISender sender)
{
    [McpServerTool]
    [Description("List maintenance notices (avisos) with optional filters. Returns a paginated list.")]
    public async Task<string> list_notices(
        [Description("Filter by status: Open, InProgress, Paused, Completed, Closed, Cancelled")] string? status = null,
        [Description("Filter by equipment code")] string? equipmentCode = null,
        [Description("Page number (1-based)")] int page = 1,
        [Description("Items per page (max 50)")] int pageSize = 20,
        CancellationToken ct = default)
    {
        NoticeStatus? parsedStatus = null;
        if (status is not null && Enum.TryParse<NoticeStatus>(status, ignoreCase: true, out var s))
            parsedStatus = s;

        var result = await sender.Send(new GetNoticesQuery(page, pageSize, parsedStatus, equipmentCode), ct);
        return result.IsSuccess
            ? JsonSerializer.Serialize(result.Value)
            : JsonSerializer.Serialize(new { error = result.Error.Description });
    }

    [McpServerTool]
    [Description("Get full details of a single notice by its ID.")]
    public async Task<string> get_notice(
        [Description("Notice ID")] int id,
        CancellationToken ct = default)
    {
        var result = await sender.Send(new GetNoticeByIdQuery(id), ct);
        return result.IsSuccess
            ? JsonSerializer.Serialize(result.Value)
            : JsonSerializer.Serialize(new { error = result.Error.Description });
    }

    [McpServerTool]
    [Description("Create a new maintenance notice (aviso).")]
    public async Task<string> create_notice(
        [Description("Unique notice number (e.g. AV-2025-001)")] string number,
        [Description("Equipment code")] string equipmentCode,
        [Description("Description of the issue")] string description,
        [Description("Physical location")] string? location = null,
        [Description("Customer code")] string? customer = null,
        [Description("Priority (1=low, 5=high)")] int priority = 1,
        CancellationToken ct = default)
    {
        var result = await sender.Send(
            new CreateNoticeCommand(number, equipmentCode, description, location, customer, priority), ct);
        return result.IsSuccess
            ? JsonSerializer.Serialize(result.Value)
            : JsonSerializer.Serialize(new { error = result.Error.Description });
    }

    [McpServerTool]
    [Description("Change the status of a notice. Valid statuses: Open, InProgress, Paused, Completed, Closed, Cancelled.")]
    public async Task<string> change_notice_status(
        [Description("Notice ID")] int id,
        [Description("New status: Open, InProgress, Paused, Completed, Closed, Cancelled")] string newStatus,
        CancellationToken ct = default)
    {
        if (!Enum.TryParse<NoticeStatus>(newStatus, ignoreCase: true, out var status))
            return JsonSerializer.Serialize(new { error = $"Invalid status '{newStatus}'. Valid values: {string.Join(", ", Enum.GetNames<NoticeStatus>())}" });

        var result = await sender.Send(new ChangeNoticeStatusCommand(id, status), ct);
        return result.IsSuccess
            ? JsonSerializer.Serialize(new { success = true })
            : JsonSerializer.Serialize(new { error = result.Error.Description });
    }
}
