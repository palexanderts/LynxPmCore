using LynxPmCore.Shared.Abstractions;

namespace LynxPmCore.Application.Features.Notices.Commands.StartOperation;

public sealed record StartOperationCommand(
    int NoticeId,
    int OperationId,
    string? ScannedEquipmentCode = null) : ICommand, ITransactional;
