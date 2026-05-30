using LynxPmCore.Domain.Aggregates.Customers;

namespace LynxPmCore.Domain.Repositories;

public interface ICustomerRepository
{
    Task<Customer?> GetByCodeAsync(string code, CancellationToken ct = default);
    Task<(IReadOnlyList<Customer> Items, int TotalCount)> GetPagedAsync(
        int page, int pageSize,
        string? search = null,
        CancellationToken ct = default);
    Task UpsertManyAsync(IEnumerable<Customer> customers, CancellationToken ct = default);
}
