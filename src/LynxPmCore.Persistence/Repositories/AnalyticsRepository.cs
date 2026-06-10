using LynxPmCore.Application.Common.Interfaces;
using LynxPmCore.Application.DTOs;
using LynxPmCore.Domain.Enums;
using LynxPmCore.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace LynxPmCore.Persistence.Repositories;

internal sealed class AnalyticsRepository(LynxPmDbContext db) : IAnalyticsRepository
{
    public async Task<NoticeKpisDto> GetNoticeKpisAsync(CancellationToken ct = default)
    {
        var notices = await db.Notices
            .Where(n => !n.IsDeleted)
            .Select(n => new { n.Status, n.IsApproved, n.RejectionReason, n.CreatedAt, n.UpdatedAt })
            .ToListAsync(ct);

        var byStatus = notices
            .GroupBy(n => n.Status)
            .ToDictionary(g => g.Key.ToString(), g => g.Count());

        var completed = notices
            .Where(n => n.Status == NoticeStatus.Completed || n.Status == NoticeStatus.Closed)
            .ToList();

        var avgResolution = completed.Count > 0
            ? completed.Average(n => (n.UpdatedAt - n.CreatedAt).TotalHours)
            : 0;

        var pendingApproval = notices.Count(n => !n.IsApproved && n.RejectionReason == null
            && n.Status != NoticeStatus.Cancelled && n.Status != NoticeStatus.Closed);

        return new NoticeKpisDto
        {
            Total = notices.Count,
            ByStatus = byStatus,
            AvgResolutionHours = Math.Round(avgResolution, 1),
            PendingApproval = pendingApproval
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
