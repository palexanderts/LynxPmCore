using LynxPmCore.Application.DTOs;
using LynxPmCore.Shared.Abstractions;

namespace LynxPmCore.Application.Features.Equipments.Queries.GetEquipmentHistory;

public sealed record GetEquipmentHistoryQuery(string EquipmentCode) : IQuery<IReadOnlyList<EquipmentHistoryDto>>;
