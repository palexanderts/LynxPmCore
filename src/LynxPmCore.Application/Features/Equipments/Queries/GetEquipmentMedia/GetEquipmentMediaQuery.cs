using LynxPmCore.Application.DTOs;
using LynxPmCore.Shared.Abstractions;

namespace LynxPmCore.Application.Features.Equipments.Queries.GetEquipmentMedia;

public sealed record GetEquipmentMediaQuery(string EquipmentCode) : IQuery<IReadOnlyList<EquipmentMediaDto>>;
