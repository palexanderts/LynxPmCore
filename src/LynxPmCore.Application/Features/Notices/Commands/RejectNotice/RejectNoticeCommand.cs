using LynxPmCore.Application.DTOs;
using LynxPmCore.Shared.Abstractions;

namespace LynxPmCore.Application.Features.Notices.Commands.RejectNotice;

public sealed record RejectNoticeCommand(int NoticeId, string RejectedBy, string Reason)
    : ICommand<NoticeDto>, ITransactional;
