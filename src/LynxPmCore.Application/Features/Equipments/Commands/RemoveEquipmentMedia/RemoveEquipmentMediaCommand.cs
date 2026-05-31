using LynxPmCore.Shared.Abstractions;

namespace LynxPmCore.Application.Features.Equipments.Commands.RemoveEquipmentMedia;

public sealed record RemoveEquipmentMediaCommand(Guid MediaId) : ICommand;
