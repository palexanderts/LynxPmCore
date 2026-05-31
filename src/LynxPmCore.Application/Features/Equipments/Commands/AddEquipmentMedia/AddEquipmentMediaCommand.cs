using LynxPmCore.Application.DTOs;
using LynxPmCore.Shared.Abstractions;

namespace LynxPmCore.Application.Features.Equipments.Commands.AddEquipmentMedia;

public sealed record AddEquipmentMediaCommand(
    string EquipmentCode,
    string MediaType,
    string Url,
    string? ThumbnailUrl,
    string? Title,
    int Position = 0) : ICommand<EquipmentMediaDto>;
