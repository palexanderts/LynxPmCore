using LynxPmCore.Application.DTOs;

namespace LynxPmCore.Application.Common.Interfaces;

public interface IAnalyticsRepository
{
    Task<NoticeKpisDto> GetNoticeKpisAsync(CancellationToken ct = default);
    Task<EquipmentKpisDto> GetEquipmentKpisAsync(CancellationToken ct = default);
    Task<SyncKpisDto> GetSyncKpisAsync(CancellationToken ct = default);
}
