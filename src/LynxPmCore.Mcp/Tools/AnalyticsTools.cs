using System.ComponentModel;
using System.Text.Json;
using LynxPmCore.Application.Features.Analytics.Queries.GetDashboardKpis;
using MediatR;
using ModelContextProtocol.Server;

namespace LynxPmCore.Mcp.Tools;

[McpServerToolType]
public sealed class AnalyticsTools(ISender sender)
{
    [McpServerTool]
    [Description("Get dashboard KPIs: notice counts by status, average resolution time, pending approvals, top failing equipment, and sync metrics.")]
    public async Task<string> get_dashboard_kpis(CancellationToken ct = default)
    {
        var result = await sender.Send(new GetDashboardKpisQuery(), ct);
        return result.IsSuccess
            ? JsonSerializer.Serialize(result.Value)
            : JsonSerializer.Serialize(new { error = result.Error.Description });
    }

    [McpServerTool]
    [Description("Get the top 5 equipment pieces with the most maintenance notices (failure-prone equipment).")]
    public async Task<string> get_top_failing_equipment(CancellationToken ct = default)
    {
        var result = await sender.Send(new GetDashboardKpisQuery(), ct);
        return result.IsSuccess
            ? JsonSerializer.Serialize(result.Value.Equipment.TopFailing)
            : JsonSerializer.Serialize(new { error = result.Error.Description });
    }
}
