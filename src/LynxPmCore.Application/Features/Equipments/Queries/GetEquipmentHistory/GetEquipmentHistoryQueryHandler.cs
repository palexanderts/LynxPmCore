using LynxPmCore.Application.Common.Interfaces;
using LynxPmCore.Application.DTOs;
using LynxPmCore.Domain.Errors;
using LynxPmCore.Shared.Abstractions;
using LynxPmCore.Shared.Common;
using System.Text.Json.Serialization;

namespace LynxPmCore.Application.Features.Equipments.Queries.GetEquipmentHistory;

internal sealed class GetEquipmentHistoryQueryHandler(
    IApexServiceRouter apexRouter,
    ICurrentUserService currentUser) : IQueryHandler<GetEquipmentHistoryQuery, IReadOnlyList<EquipmentHistoryDto>>
{
    public async Task<Result<IReadOnlyList<EquipmentHistoryDto>>> Handle(GetEquipmentHistoryQuery request, CancellationToken ct)
    {
        var payload = new { EquipmentCode = request.EquipmentCode };
        var response = await apexRouter.CallAsync<object, HistoryResponse>(
            "LYNX_PM_GET_EQUIPMENT_HISTORY", payload, currentUser.UserCode, ct);

        var items = response?.Items ?? [];
        return Result.Success<IReadOnlyList<EquipmentHistoryDto>>(items);
    }

    private sealed class HistoryResponse
    {
        [JsonPropertyName("items")]
        public List<EquipmentHistoryDto> Items { get; set; } = [];
    }
}
