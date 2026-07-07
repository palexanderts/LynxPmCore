using LynxPmCore.Domain.Aggregates.Notices;
using LynxPmCore.Domain.Enums;
using LynxPmCore.Domain.Repositories;
using LynxPmCore.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace LynxPmCore.Persistence.Repositories;

internal sealed class NoticeRepository(LynxPmDbContext db) : INoticeRepository
{
    public async Task<Notice?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var notice = await db.Notices.Include("Causes").FirstOrDefaultAsync(n => n.Id == id && !n.IsDeleted, ct);
        if (notice is not null)
            notice.HydrateOperations(await GetOperationsAsync(notice.Id, ct));
        return notice;
    }

    public async Task<Notice?> GetByNumberAsync(string number, CancellationToken ct = default)
    {
        var notice = await db.Notices.Include("Causes")
            .FirstOrDefaultAsync(n => n.Number == number.ToUpperInvariant() && !n.IsDeleted, ct);
        if (notice is not null)
            notice.HydrateOperations(await GetOperationsAsync(notice.Id, ct));
        return notice;
    }

    // AVISOID en LYNX_PM_AVISO_OPERATIONS es VARCHAR2 sin FK real y con valores
    // históricos inconsistentes (no siempre numéricos) — comparar columna=parámetro
    // aquí es seguro (Oracle nunca necesita convertir la columna); un Include() vía
    // join sí forzaría convertir toda la columna y truena con esas filas viejas.
    private async Task<List<Operation>> GetOperationsAsync(int noticeId, CancellationToken ct)
        => await db.Set<Operation>()
            .Include(o => o.Parts)
            .Where(o => o.NoticeId == noticeId && !o.IsDeleted)
            .OrderBy(o => o.Position)
            .ToListAsync(ct);

    public async Task<(IReadOnlyList<Notice> Items, int TotalCount)> GetPagedAsync(
        int page, int pageSize, NoticeStatus? status, string? equipmentCode, string? createdBy, CancellationToken ct = default)
    {
        var query = db.Notices.AsNoTracking().Where(n => !n.IsDeleted);

        if (status.HasValue) query = query.Where(n => n.Status == status.Value);
        if (!string.IsNullOrWhiteSpace(equipmentCode)) query = query.Where(n => n.EquipmentCode == equipmentCode.ToUpperInvariant());
        if (!string.IsNullOrWhiteSpace(createdBy)) query = query.Where(n => n.CreatedBy == createdBy);

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(n => n.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items, total);
    }

    public async Task<IReadOnlyList<Notice>> GetNotSynchronizedAsync(CancellationToken ct = default)
    {
        var notices = await db.Notices
            .Where(n => !n.IsSynchronized && !n.IsDeleted)
            .ToListAsync(ct);

        foreach (var notice in notices)
            notice.HydrateOperations(await GetOperationsAsync(notice.Id, ct));

        return notices;
    }

    public async Task AddAsync(Notice notice, CancellationToken ct = default)
        => await db.Notices.AddAsync(notice, ct);

    public Task UpdateAsync(Notice notice, CancellationToken ct = default)
    {
        // notice llega siempre trackeado (viene de un Get* en el mismo DbContext), por lo que
        // EF ya detecta los cambios de escalares e hijos nuevos/modificados vía DetectChanges.
        // Llamar a Update() aquí forzaría TODO el grafo a Modified, incluyendo hijos recién
        // agregados en memoria (aún no existen en la tabla) -> UPDATE en vez de INSERT ->
        // DbUpdateConcurrencyException ("0 rows affected").
        if (db.Entry(notice).State == EntityState.Detached)
            db.Notices.Update(notice);

        return Task.CompletedTask;
    }
}
