using LynxPmCore.Domain.Aggregates.Customers;
using LynxPmCore.Domain.Repositories;
using LynxPmCore.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace LynxPmCore.Persistence.Repositories;

internal sealed class CustomerRepository(LynxPmDbContext db) : ICustomerRepository
{
    public async Task<Customer?> GetByCodeAsync(string code, CancellationToken ct = default)
        => await db.Customers.FirstOrDefaultAsync(c => c.Code == code.ToUpperInvariant() && !c.IsDeleted, ct);

    public async Task<(IReadOnlyList<Customer> Items, int TotalCount)> GetPagedAsync(
        int page, int pageSize, string? search, CancellationToken ct = default)
    {
        var query = db.Customers.AsNoTracking().Where(c => c.IsActive && !c.IsDeleted);

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(c => c.Code.Contains(search) || c.Name.Contains(search));

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderBy(c => c.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items, total);
    }

    public async Task UpsertManyAsync(IEnumerable<Customer> customers, CancellationToken ct = default)
    {
        foreach (var customer in customers)
        {
            var existing = await db.Customers.FirstOrDefaultAsync(c => c.Code == customer.Code, ct);
            if (existing is null)
                await db.Customers.AddAsync(customer, ct);
            else
                db.Customers.Update(customer);
        }
    }
}
