using LynxPmCore.Shared.Abstractions;

namespace LynxPmCore.Application.Features.Notices.Commands.SynchronizeNotice;

public sealed record SynchronizeNoticeCommand(Guid NoticeId) : ICommand, ITransactional;
