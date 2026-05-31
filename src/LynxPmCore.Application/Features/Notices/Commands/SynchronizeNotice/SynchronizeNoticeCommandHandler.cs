using LynxPmCore.Application.Common.Interfaces;
using LynxPmCore.Domain.Errors;
using LynxPmCore.Domain.Repositories;
using LynxPmCore.Shared.Abstractions;
using LynxPmCore.Shared.Common;

namespace LynxPmCore.Application.Features.Notices.Commands.SynchronizeNotice;

// ERP sync pendiente — el flujo de sincronización con el sistema externo
// se definirá cuando se indiquen los procesos ERP correspondientes.
internal sealed class SynchronizeNoticeCommandHandler(
    INoticeRepository noticeRepository,
    IUnitOfWork unitOfWork) : ICommandHandler<SynchronizeNoticeCommand>
{
    public async Task<Result> Handle(SynchronizeNoticeCommand request, CancellationToken ct)
    {
        var notice = await noticeRepository.GetByIdAsync(request.NoticeId, ct);
        if (notice is null) return Result.Failure(DomainErrors.Notice.NotFound);

        notice.MarkSynchronized();
        await noticeRepository.UpdateAsync(notice, ct);
        await unitOfWork.SaveChangesAsync(ct);
        return Result.Success();
    }
}
