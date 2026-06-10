using System.ComponentModel;
using System.Text.Json;
using LynxPmCore.Application.Features.Equipments.Queries.GetEquipmentMedia;
using LynxPmCore.Application.Features.Equipments.Queries.GetEquipments;
using MediatR;
using ModelContextProtocol.Server;

namespace LynxPmCore.Mcp.Tools;

[McpServerToolType]
public sealed class EquipmentTools(ISender sender)
{
    [McpServerTool]
    [Description("List equipment with optional search and customer filter. Returns a paginated list.")]
    public async Task<string> list_equipment(
        [Description("Search term (matches code or description)")] string? search = null,
        [Description("Filter by customer code")] string? customer = null,
        [Description("Page number (1-based)")] int page = 1,
        [Description("Items per page")] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await sender.Send(new GetEquipmentsQuery(page, pageSize, search, customer), ct);
        return result.IsSuccess
            ? JsonSerializer.Serialize(result.Value)
            : JsonSerializer.Serialize(new { error = result.Error.Description });
    }

    [McpServerTool]
    [Description("Get media (photos, documents) attached to a specific piece of equipment.")]
    public async Task<string> get_equipment_media(
        [Description("Equipment code")] string code,
        CancellationToken ct = default)
    {
        var result = await sender.Send(new GetEquipmentMediaQuery(code), ct);
        return result.IsSuccess
            ? JsonSerializer.Serialize(result.Value)
            : JsonSerializer.Serialize(new { error = result.Error.Description });
    }
}
