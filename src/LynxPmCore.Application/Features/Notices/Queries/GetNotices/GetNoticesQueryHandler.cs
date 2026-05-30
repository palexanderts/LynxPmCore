using AutoMapper;
using LynxPmCore.Application.DTOs;
using LynxPmCore.Domain.Repositories;
using LynxPmCore.Shared.Abstractions;
using LynxPmCore.Shared.Common;

namespace LynxPmCore.Application.Features.Notices.Queries.GetNotices;

internal sealed class GetNoticesQueryHandler(
    INoticeRepository noticeRepository,
    IMapper mapper) : IQueryHandler<GetNoticesQuery, PagedResult<NoticeListDto>>
{
    public async Task<Result<PagedResult<NoticeListDto>>> Handle(GetNoticesQuery request, CancellationToken ct)
    {
        var (items, total) = await noticeRepository.GetPagedAsync(
            request.Page, request.PageSize, request.Status, request.EquipmentCode, request.CreatedBy, ct);

        var dtos = mapper.Map<IReadOnlyList<NoticeListDto>>(items);
        return Result.Success(new PagedResult<NoticeListDto>(dtos, request.Page, request.PageSize, total));
    }
}
