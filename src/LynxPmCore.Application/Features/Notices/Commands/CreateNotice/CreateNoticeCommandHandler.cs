using AutoMapper;
using LynxPmCore.Application.Common.Interfaces;
using LynxPmCore.Application.DTOs;
using LynxPmCore.Domain.Aggregates.Notices;
using LynxPmCore.Domain.Errors;
using LynxPmCore.Domain.Repositories;
using LynxPmCore.Shared.Abstractions;
using LynxPmCore.Shared.Common;

namespace LynxPmCore.Application.Features.Notices.Commands.CreateNotice;

internal sealed class CreateNoticeCommandHandler(
    INoticeRepository noticeRepository,
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUser,
    IMapper mapper) : ICommandHandler<CreateNoticeCommand, NoticeDto>
{
    public async Task<Result<NoticeDto>> Handle(CreateNoticeCommand request, CancellationToken ct)
    {
        var existing = await noticeRepository.GetByNumberAsync(request.Number, ct);
        if (existing is not null)
            return Result.Failure<NoticeDto>(DomainErrors.Notice.NotFound);

        var noticeResult = Notice.Create(
            request.Number,
            request.EquipmentCode,
            request.Description,
            currentUser.UserCode ?? "SYSTEM",
            request.Location,
            request.Customer,
            request.Priority,
            request.PriorityCode,
            request.PriorityText,
            request.NoticeTypeCode,
            request.NoticeTypeText,
            request.Center);

        if (noticeResult.IsFailure)
            return Result.Failure<NoticeDto>(noticeResult.Error);

        await noticeRepository.AddAsync(noticeResult.Value, ct);
        await unitOfWork.SaveChangesAsync(ct);

        return Result.Success(mapper.Map<NoticeDto>(noticeResult.Value));
    }
}
