using LynxPmCore.Domain.Aggregates.Equipments;

namespace LynxPmCore.Domain.Repositories;

public interface IEquipmentRepository
{
    Task<Equipment?> GetByCodeAsync(string code, CancellationToken ct = default);
    Task<(IReadOnlyList<Equipment> Items, int TotalCount)> GetPagedAsync(
        int page, int pageSize,
        string? search = null,
        string? customer = null,
        CancellationToken ct = default);
    Task<IReadOnlyList<Equipment>> GetByCustomerAsync(string customerCode, CancellationToken ct = default);
    Task UpsertAsync(Equipment equipment, CancellationToken ct = default);
    Task UpsertManyAsync(IEnumerable<Equipment> equipments, CancellationToken ct = default);

    Task<IReadOnlyList<EquipmentMedia>> GetMediaByCodeAsync(string code, CancellationToken ct = default);
    Task<EquipmentMedia?> GetMediaByIdAsync(Guid id, CancellationToken ct = default);
    Task AddMediaAsync(EquipmentMedia media, CancellationToken ct = default);
}
