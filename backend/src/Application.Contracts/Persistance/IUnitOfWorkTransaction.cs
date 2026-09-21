namespace Application.Contracts;

public interface IUnitOfWorkTransaction : IAsyncDisposable
{
    Task CommitAsync();
    Task RollbackAsync();
}
