using LynxPmCore.Application.DTOs;
using LynxPmCore.Domain.Enums;
using LynxPmCore.Shared.Abstractions;
using LynxPmCore.Shared.Common;

namespace LynxPmCore.Application.Features.Notices.Queries.GetNotices;

public sealed record GetNoticesQuery(
    int Page = 1,
    int PageSize = 20,
    NoticeStatus? Status = null,
    string? EquipmentCode = null,
    string? CreatedBy = null) : IQuery<PagedResult<NoticeListDto>>;
