using Application.Contracts;
using Microsoft.EntityFrameworkCore.Storage;

namespace Infrastructure.Persistance;

internal sealed class UnitOfWorkTransaction(IDbContextTransaction transaction) : IUnitOfWorkTransaction
{
    public Task CommitAsync() => transaction.CommitAsync();

    public Task RollbackAsync() => transaction.RollbackAsync();

    public ValueTask DisposeAsync() => transaction.DisposeAsync();
}
