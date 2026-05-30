using LynxPmCore.Application.Common.Interfaces;
using LynxPmCore.Domain.Errors;
using LynxPmCore.Domain.Repositories;
using LynxPmCore.Shared.Abstractions;
using LynxPmCore.Shared.Common;

namespace LynxPmCore.Application.Features.Notices.Commands.StartOperation;

internal sealed class StartOperationCommandHandler(
    INoticeRepository noticeRepository,
    IUnitOfWork unitOfWork) : ICommandHandler<StartOperationCommand>
{
    public async Task<Result> Handle(StartOperationCommand request, CancellationToken ct)
    {
        var notice = await noticeRepository.GetByIdAsync(request.NoticeId, ct);
        if (notice is null) return Result.Failure(DomainErrors.Notice.NotFound);

        var operation = notice.Operations.FirstOrDefault(o => o.Id == request.OperationId);
        if (operation is null) return Result.Failure(DomainErrors.Operation.NotFound);

        var result = operation.Start(request.ScannedEquipmentCode);
        if (result.IsFailure) return result;

        await noticeRepository.UpdateAsync(notice, ct);
        await unitOfWork.SaveChangesAsync(ct);
        return Result.Success();
    }
}
