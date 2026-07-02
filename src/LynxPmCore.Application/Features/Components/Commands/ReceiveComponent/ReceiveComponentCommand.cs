using LynxPmCore.Application.DTOs;
using LynxPmCore.Shared.Abstractions;

namespace LynxPmCore.Application.Features.Components.Commands.ReceiveComponent;

public sealed record ReceiveComponentCommand(
    string ComponentId,
    int Quantity,
    string? Observations,
    string ReceivedBy,
    DateTime ReceivedAt) : ICommand<ComponentReceiptDto>;
