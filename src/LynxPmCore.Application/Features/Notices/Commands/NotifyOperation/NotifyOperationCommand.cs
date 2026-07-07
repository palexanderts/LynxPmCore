using LynxPmCore.Shared.Abstractions;

namespace LynxPmCore.Application.Features.Notices.Commands.NotifyOperation;

public sealed record NotifyOperationCommand(
    int NoticeId,
    int OperationId,
    string? Failure,
    IReadOnlyList<NotifyOperationCauseInput>? Causes = null,
    IReadOnlyList<NotifyOperationPartInput>? Parts = null) : ICommand, ITransactional;

public sealed record NotifyOperationCauseInput(string Code, string? Text);

public sealed record NotifyOperationPartInput(string Code, string? Text);
