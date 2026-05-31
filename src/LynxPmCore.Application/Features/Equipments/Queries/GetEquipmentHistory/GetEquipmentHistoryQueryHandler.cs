using LynxPmCore.Application.DTOs;
using LynxPmCore.Domain.Enums;
using LynxPmCore.Domain.Repositories;
using LynxPmCore.Shared.Abstractions;
using LynxPmCore.Shared.Common;

namespace LynxPmCore.Application.Features.Equipments.Queries.GetEquipmentHistory;

internal sealed class GetEquipmentHistoryQueryHandler(
    INoticeRepository noticeRepository) : IQueryHandler<GetEquipmentHistoryQuery, IReadOnlyList<EquipmentHistoryDto>>
{
    public async Task<Result<IReadOnlyList<EquipmentHistoryDto>>> Handle(GetEquipmentHistoryQuery request, CancellationToken ct)
    {
        var (notices, _) = await noticeRepository.GetPagedAsync(1, 500, null, request.EquipmentCode, null, ct);

        var items = notices.Select(n => new EquipmentHistoryDto
        {
            NoticeNumber   = n.Number,
            EquipmentCode  = n.EquipmentCode,
            Description    = n.Description,
            Status         = n.Status.ToString(),
            Date           = n.CreatedAt,
            Technician     = n.ApprovedBy ?? n.CreatedBy
        }).ToList();

        return Result.Success<IReadOnlyList<EquipmentHistoryDto>>(items);
    }
}
