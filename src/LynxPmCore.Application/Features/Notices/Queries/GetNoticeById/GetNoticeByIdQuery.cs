using LynxPmCore.Application.DTOs;
using LynxPmCore.Shared.Abstractions;

namespace LynxPmCore.Application.Features.Notices.Queries.GetNoticeById;

public sealed record GetNoticeByIdQuery(int NoticeId) : IQuery<NoticeDto>;
