using LynxPmCore.Application.Common.Interfaces;
using LynxPmCore.Domain.Errors;
using LynxPmCore.Domain.Repositories;
using LynxPmCore.Shared.Abstractions;
using LynxPmCore.Shared.Common;

namespace LynxPmCore.Application.Features.Notices.Commands.ChangeNoticeStatus;

internal sealed class ChangeNoticeStatusCommandHandler(
    INoticeRepository noticeRepository,
    IUnitOfWork unitOfWork) : ICommandHandler<ChangeNoticeStatusCommand>
{
    public async Task<Result> Handle(ChangeNoticeStatusCommand request, CancellationToken ct)
    {
        var notice = await noticeRepository.GetByIdAsync(request.NoticeId, ct);
        if (notice is null)
            return Result.Failure(DomainErrors.Notice.NotFound);

        var result = notice.ChangeStatus(request.NewStatus);
        if (result.IsFailure) return result;

        await noticeRepository.UpdateAsync(notice, ct);
        await unitOfWork.SaveChangesAsync(ct);
        return Result.Success();
    }
}
