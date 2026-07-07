using LynxPmCore.Application.Common.Interfaces;
using LynxPmCore.Domain.Errors;
using LynxPmCore.Domain.Repositories;
using LynxPmCore.Shared.Abstractions;
using LynxPmCore.Shared.Common;

namespace LynxPmCore.Application.Features.Notices.Commands.NotifyOperation;

internal sealed class NotifyOperationCommandHandler(
    INoticeRepository noticeRepository,
    IUnitOfWork unitOfWork) : ICommandHandler<NotifyOperationCommand>
{
    public async Task<Result> Handle(NotifyOperationCommand request, CancellationToken ct)
    {
        var notice = await noticeRepository.GetByIdAsync(request.NoticeId, ct);
        if (notice is null) return Result.Failure(DomainErrors.Notice.NotFound);

        var operation = notice.Operations.FirstOrDefault(o => o.Id == request.OperationId);
        if (operation is null) return Result.Failure(DomainErrors.Operation.NotFound);

        var result = operation.Notify(request.Failure);
        if (result.IsFailure) return result;

        if (request.Causes is not null)
        {
            foreach (var cause in request.Causes)
                notice.AddCause(cause.Code, cause.Text);
        }

        if (request.Parts is not null)
        {
            foreach (var part in request.Parts)
                operation.AddPart(part.Code, part.Text);
        }

        await noticeRepository.UpdateAsync(notice, ct);
        await unitOfWork.SaveChangesAsync(ct);
        return Result.Success();
    }
}
