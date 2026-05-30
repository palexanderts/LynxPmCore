using LynxPmCore.Domain.Aggregates.Equipments;
using LynxPmCore.Domain.Repositories;
using LynxPmCore.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace LynxPmCore.Persistence.Repositories;

internal sealed class EquipmentRepository(LynxPmDbContext db) : IEquipmentRepository
{
    public async Task<Equipment?> GetByCodeAsync(string code, CancellationToken ct = default)
        => await db.Equipments.FirstOrDefaultAsync(e => e.Code == code.ToUpperInvariant() && !e.IsDeleted, ct);

    public async Task<(IReadOnlyList<Equipment> Items, int TotalCount)> GetPagedAsync(
        int page, int pageSize, string? search, string? customer, CancellationToken ct = default)
    {
        var query = db.Equipments.AsNoTracking().Where(e => e.IsActive && !e.IsDeleted);

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(e => e.Code.Contains(search) || e.Description.Contains(search));
        if (!string.IsNullOrWhiteSpace(customer))
            query = query.Where(e => e.Customer == customer);

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderBy(e => e.Code)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items, total);
    }

    public async Task<IReadOnlyList<Equipment>> GetByCustomerAsync(string customerCode, CancellationToken ct = default)
        => await db.Equipments
            .Where(e => e.Customer == customerCode && e.IsActive && !e.IsDeleted)
            .OrderBy(e => e.Code)
            .ToListAsync(ct);

    public async Task UpsertAsync(Equipment equipment, CancellationToken ct = default)
    {
        var existing = await db.Equipments.FirstOrDefaultAsync(e => e.Code == equipment.Code, ct);
        if (existing is null)
            await db.Equipments.AddAsync(equipment, ct);
        else
            db.Equipments.Update(equipment);
    }

    public async Task UpsertManyAsync(IEnumerable<Equipment> equipments, CancellationToken ct = default)
    {
        foreach (var equipment in equipments)
            await UpsertAsync(equipment, ct);
    }
}
