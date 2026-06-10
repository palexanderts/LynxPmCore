using AutoMapper;
using LynxPmCore.Application.Common.Interfaces;
using LynxPmCore.Application.DTOs;
using LynxPmCore.Domain.Errors;
using LynxPmCore.Domain.Repositories;
using LynxPmCore.Shared.Abstractions;
using LynxPmCore.Shared.Common;

namespace LynxPmCore.Application.Features.Notices.Commands.RejectNotice;

internal sealed class RejectNoticeCommandHandler(
    INoticeRepository noticeRepository,
    IUnitOfWork unitOfWork,
    INotificationService notifications,
    IMapper mapper) : ICommandHandler<RejectNoticeCommand, NoticeDto>
{
    public async Task<Result<NoticeDto>> Handle(RejectNoticeCommand request, CancellationToken ct)
    {
        var notice = await noticeRepository.GetByIdAsync(request.NoticeId, ct);
        if (notice is null)
            return Result.Failure<NoticeDto>(DomainErrors.Notice.NotFound);

        var result = notice.Reject(request.RejectedBy, request.Reason);
        if (result.IsFailure)
            return Result.Failure<NoticeDto>(result.Error);

        await noticeRepository.UpdateAsync(notice, ct);
        await unitOfWork.SaveChangesAsync(ct);

        var dto = mapper.Map<NoticeDto>(notice);
        await notifications.SendToGroupAsync($"notice:{notice.Id}", "NoticeApprovalChanged", dto, ct);
        return Result.Success(dto);
    }
}
