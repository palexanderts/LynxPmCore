using LynxPmCore.Domain.Enums;
using LynxPmCore.Shared.Abstractions;

namespace LynxPmCore.Application.Features.Notices.Commands.ChangeNoticeStatus;

public sealed record ChangeNoticeStatusCommand(int NoticeId, NoticeStatus NewStatus) : ICommand, ITransactional;
