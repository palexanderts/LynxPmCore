using LynxPmCore.Shared.Abstractions;

namespace LynxPmCore.Application.Features.Notices.Commands.SynchronizeNotice;

public sealed record SynchronizeNoticeCommand(int NoticeId) : ICommand, ITransactional;
