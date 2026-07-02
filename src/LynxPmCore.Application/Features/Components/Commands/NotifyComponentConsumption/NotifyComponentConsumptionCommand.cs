using LynxPmCore.Application.DTOs;
using LynxPmCore.Shared.Abstractions;

namespace LynxPmCore.Application.Features.Components.Commands.NotifyComponentConsumption;

public sealed record NotifyComponentConsumptionCommand(
    string ComponentId,
    string? AvisoId,
    int? OperationPosition,
    int Quantity,
    string? Observations,
    string ConsumedBy) : ICommand<ComponentNotificationDto>;
