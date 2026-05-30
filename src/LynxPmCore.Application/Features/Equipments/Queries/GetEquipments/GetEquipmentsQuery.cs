using LynxPmCore.Application.DTOs;
using LynxPmCore.Shared.Abstractions;
using LynxPmCore.Shared.Common;

namespace LynxPmCore.Application.Features.Equipments.Queries.GetEquipments;

public sealed record GetEquipmentsQuery(
    int Page = 1,
    int PageSize = 50,
    string? Search = null,
    string? Customer = null) : IQuery<PagedResult<EquipmentDto>>;
