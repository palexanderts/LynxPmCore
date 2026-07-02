using LynxPmCore.Application.Common.Interfaces;
using LynxPmCore.Application.DTOs;
using LynxPmCore.Domain.Enums;
using LynxPmCore.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace LynxPmCore.Persistence.Repositories;

internal sealed class AnalyticsRepository(LynxPmDbContext db) : IAnalyticsRepository
{
    public async Task<NoticeKpisDto> GetNoticeKpisAsync(int year, int month, CancellationToken ct = default)
    {
        var monthStart = new DateTime(year, month, 1, 0, 0, 0, DateTimeKind.Utc);
        var monthEnd = monthStart.AddMonths(1);
        var yearStart = new DateTime(year, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var yearEnd = yearStart.AddYears(1);

        var total = await db.Notices.CountAsync(n => !n.IsDeleted, ct);

        var pendingApproval = await db.Notices
            .CountAsync(n => !n.IsDeleted && n.Status == NoticeStatus.Open, ct);

        var byStatusRaw = await db.Notices
            .Where(n => !n.IsDeleted && n.CreatedAt >= monthStart && n.CreatedAt < monthEnd)
            .Select(n => new { n.Status })
            .ToListAsync(ct);

        var byStatus = byStatusRaw
            .GroupBy(n => (int)n.Status)
            .ToDictionary(g => g.Key.ToString(), g => g.Count());

        var byMonthRaw = await db.Notices
            .Where(n => !n.IsDeleted && n.CreatedAt >= yearStart && n.CreatedAt < yearEnd)
            .Select(n => new { n.CreatedAt.Month })
            .ToListAsync(ct);

        var byMonth = Enumerable.Range(1, 12).ToDictionary(m => m.ToString(), _ => 0);
        foreach (var item in byMonthRaw)
            byMonth[item.Month.ToString()] += 1;

        var closedNotices = await db.Notices
            .Where(n => !n.IsDeleted && (n.Status == NoticeStatus.Completed || n.Status == NoticeStatus.Closed))
            .Select(n => new { n.CreatedAt, n.UpdatedAt })
            .ToListAsync(ct);

        var avgResolution = closedNotices.Count > 0
            ? closedNotices.Average(n => (n.UpdatedAt - n.CreatedAt).TotalHours)
            : 0;

        return new NoticeKpisDto
        {
            Total = total,
            PendingApproval = pendingApproval,
            AvgResolutionHours = Math.Round(avgResolution, 1),
            ByStatus = byStatus,
            ByMonth = byMonth
        };
    }

    public async Task<EquipmentKpisDto> GetEquipmentKpisAsync(CancellationToken ct = default)
    {
        var totalEquipment = await db.Equipments.CountAsync(e => !e.IsDeleted, ct);

        var topCodes = await db.Notices
            .Where(n => !n.IsDeleted)
            .GroupBy(n => n.EquipmentCode)
            .Select(g => new { Code = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .Take(5)
            .ToListAsync(ct);

        var codes = topCodes.Select(x => x.Code).ToList();
        var equipmentDescs = await db.Equipments
            .Where(e => codes.Contains(e.Code))
            .Select(e => new { e.Code, e.Description })
            .ToDictionaryAsync(e => e.Code, e => e.Description, ct);

        var topFailing = topCodes.Select(x => new EquipmentFailureKpiDto
        {
            Code = x.Code,
            Description = equipmentDescs.GetValueOrDefault(x.Code, "—"),
            NoticeCount = x.Count
        }).ToList();

        return new EquipmentKpisDto
        {
            Total = totalEquipment,
            TopFailing = topFailing
        };
    }

    public async Task<SyncKpisDto> GetSyncKpisAsync(CancellationToken ct = default)
    {
        var cutoff = DateTime.UtcNow.AddHours(-24);

        var pendingSync = await db.Notices
            .CountAsync(n => !n.IsDeleted && !n.IsSynchronized, ct);

        var syncedLast24h = await db.Notices
            .CountAsync(n => !n.IsDeleted && n.IsSynchronized && n.SynchronizedAt >= cutoff, ct);

        return new SyncKpisDto
        {
            PendingSync = pendingSync,
            SyncedLast24h = syncedLast24h
        };
    }
}
