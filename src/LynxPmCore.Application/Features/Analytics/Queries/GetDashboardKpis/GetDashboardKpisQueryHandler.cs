using LynxPmCore.Application.Common.Interfaces;
using LynxPmCore.Application.DTOs;
using LynxPmCore.Shared.Abstractions;
using LynxPmCore.Shared.Common;

namespace LynxPmCore.Application.Features.Analytics.Queries.GetDashboardKpis;

internal sealed class GetDashboardKpisQueryHandler(
    IAnalyticsRepository analyticsRepository) : IQueryHandler<GetDashboardKpisQuery, DashboardKpisDto>
{
    public async Task<Result<DashboardKpisDto>> Handle(GetDashboardKpisQuery request, CancellationToken ct)
    {
        var noticesTask = analyticsRepository.GetNoticeKpisAsync(ct);
        var equipmentTask = analyticsRepository.GetEquipmentKpisAsync(ct);
        var syncTask = analyticsRepository.GetSyncKpisAsync(ct);

        await Task.WhenAll(noticesTask, equipmentTask, syncTask);

        return Result.Success(new DashboardKpisDto
        {
            Notices = await noticesTask,
            Equipment = await equipmentTask,
            Sync = await syncTask
        });
    }
}
