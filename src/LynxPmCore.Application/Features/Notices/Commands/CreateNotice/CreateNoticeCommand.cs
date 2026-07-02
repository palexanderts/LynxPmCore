using LynxPmCore.Application.DTOs;
using LynxPmCore.Shared.Abstractions;

namespace LynxPmCore.Application.Features.Notices.Commands.CreateNotice;

public sealed record CreateNoticeCommand(
    string Number,
    string EquipmentCode,
    string Description,
    string? Location,
    string? Customer,
    int Priority = 1,
    string? PriorityCode = null,
    string? PriorityText = null,
    string? NoticeTypeCode = null,
    string? NoticeTypeText = null,
    string? Center = null) : ICommand<NoticeDto>, ITransactional;
