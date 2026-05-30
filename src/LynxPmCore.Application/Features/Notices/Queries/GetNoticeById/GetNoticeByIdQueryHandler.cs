using AutoMapper;
using LynxPmCore.Application.DTOs;
using LynxPmCore.Domain.Errors;
using LynxPmCore.Domain.Repositories;
using LynxPmCore.Shared.Abstractions;
using LynxPmCore.Shared.Common;

namespace LynxPmCore.Application.Features.Notices.Queries.GetNoticeById;

internal sealed class GetNoticeByIdQueryHandler(
    INoticeRepository noticeRepository,
    IMapper mapper) : IQueryHandler<GetNoticeByIdQuery, NoticeDto>
{
    public async Task<Result<NoticeDto>> Handle(GetNoticeByIdQuery request, CancellationToken ct)
    {
        var notice = await noticeRepository.GetByIdAsync(request.NoticeId, ct);
        if (notice is null)
            return Result.Failure<NoticeDto>(DomainErrors.Notice.NotFound);

        return Result.Success(mapper.Map<NoticeDto>(notice));
    }
}
