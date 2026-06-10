using LynxPmCore.Application.DTOs;
using LynxPmCore.Shared.Abstractions;

namespace LynxPmCore.Application.Features.Notices.Commands.ApproveNotice;

public sealed record ApproveNoticeCommand(Guid NoticeId, string ApprovedBy)
    : ICommand<NoticeDto>, ITransactional;
