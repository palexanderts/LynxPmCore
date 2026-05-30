using LynxPmCore.Application.Common.Interfaces;
using LynxPmCore.Domain.Errors;
using LynxPmCore.Domain.Repositories;
using LynxPmCore.Shared.Abstractions;
using LynxPmCore.Shared.Common;
using System.Text.Json.Serialization;

namespace LynxPmCore.Application.Features.Notices.Commands.SynchronizeNotice;

internal sealed class SynchronizeNoticeCommandHandler(
    INoticeRepository noticeRepository,
    IApexServiceRouter apexRouter,
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUser) : ICommandHandler<SynchronizeNoticeCommand>
{
    public async Task<Result> Handle(SynchronizeNoticeCommand request, CancellationToken ct)
    {
        var notice = await noticeRepository.GetByIdAsync(request.NoticeId, ct);
        if (notice is null) return Result.Failure(DomainErrors.Notice.NotFound);

        var payload = new
        {
            notice.Number,
            notice.EquipmentCode,
            notice.Description,
            Status = notice.Status.ToString(),
            notice.IsApproved,
            notice.Priority
        };

        var response = await apexRouter.CallAsync<object, SyncResponse>(
            "LYNX_PM_GO_TO_SERVER", payload, currentUser.UserCode, ct);

        notice.MarkSynchronized(response?.ApexId);
        await noticeRepository.UpdateAsync(notice, ct);
        await unitOfWork.SaveChangesAsync(ct);
        return Result.Success();
    }

    private sealed class SyncResponse
    {
        [JsonPropertyName("apexId")]
        public string? ApexId { get; set; }
    }
}
