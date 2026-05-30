using LynxPmCore.Application.Common.Interfaces;
using LynxPmCore.Persistence.Context;
using Microsoft.EntityFrameworkCore.Storage;

namespace LynxPmCore.Persistence.UnitOfWork;

internal sealed class UnitOfWork(LynxPmDbContext db) : IUnitOfWork
{
    private IDbContextTransaction? _transaction;

    public Task<int> SaveChangesAsync(CancellationToken ct = default)
        => db.SaveChangesAsync(ct);

    public async Task BeginTransactionAsync(CancellationToken ct = default)
        => _transaction = await db.Database.BeginTransactionAsync(ct);

    public async Task CommitTransactionAsync(CancellationToken ct = default)
    {
        if (_transaction is null) return;
        await _transaction.CommitAsync(ct);
        await _transaction.DisposeAsync();
        _transaction = null;
    }

    public async Task RollbackTransactionAsync(CancellationToken ct = default)
    {
        if (_transaction is null) return;
        await _transaction.RollbackAsync(ct);
        await _transaction.DisposeAsync();
        _transaction = null;
    }
}
