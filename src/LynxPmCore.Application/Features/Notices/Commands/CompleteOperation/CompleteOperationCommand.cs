using LynxPmCore.Shared.Abstractions;

namespace LynxPmCore.Application.Features.Notices.Commands.CompleteOperation;

public sealed record CompleteOperationCommand(
    Guid NoticeId,
    Guid OperationId,
    string? Notes,
    bool PhotoConfirmed = false,
    string? Failure = null,
    IReadOnlyList<CompleteOperationCauseInput>? Causes = null) : ICommand, ITransactional;

public sealed record CompleteOperationCauseInput(string Code, string? Text);
