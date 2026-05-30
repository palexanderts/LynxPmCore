using LynxPmCore.Shared.Abstractions;

namespace LynxPmCore.Application.Features.Notices.Commands.StartOperation;

public sealed record StartOperationCommand(
    Guid NoticeId,
    Guid OperationId,
    string? ScannedEquipmentCode = null) : ICommand, ITransactional;
