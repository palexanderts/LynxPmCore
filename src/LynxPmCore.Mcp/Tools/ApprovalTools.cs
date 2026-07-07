using System.ComponentModel;
using System.Text.Json;
using LynxPmCore.Application.Features.Notices.Commands.ApproveNotice;
using LynxPmCore.Application.Features.Notices.Commands.RejectNotice;
using MediatR;
using ModelContextProtocol.Server;

namespace LynxPmCore.Mcp.Tools;

[McpServerToolType]
public sealed class ApprovalTools(ISender sender)
{
    [McpServerTool]
    [Description("Approve a maintenance notice. Records who approved it and notifies connected clients via SignalR.")]
    public async Task<string> approve_notice(
        [Description("Notice ID")] int noticeId,
        [Description("Name or code of the person approving")] string approvedBy,
        CancellationToken ct = default)
    {
        var result = await sender.Send(new ApproveNoticeCommand(noticeId, approvedBy), ct);
        return result.IsSuccess
            ? JsonSerializer.Serialize(result.Value)
            : JsonSerializer.Serialize(new { error = result.Error.Description });
    }

    [McpServerTool]
    [Description("Reject a maintenance notice with a reason. Records who rejected it and notifies connected clients.")]
    public async Task<string> reject_notice(
        [Description("Notice ID")] int noticeId,
        [Description("Name or code of the person rejecting")] string rejectedBy,
        [Description("Reason for rejection")] string reason,
        CancellationToken ct = default)
    {
        var result = await sender.Send(new RejectNoticeCommand(noticeId, rejectedBy, reason), ct);
        return result.IsSuccess
            ? JsonSerializer.Serialize(result.Value)
            : JsonSerializer.Serialize(new { error = result.Error.Description });
    }
}
