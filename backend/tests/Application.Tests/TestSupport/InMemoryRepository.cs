using System.Linq.Expressions;
using Application.Contracts;
using Domain.Entities;

namespace Application.Tests.TestSupport;

/// <summary>
/// Generic in-memory IRepository implementation for tests that genuinely
/// need to assert state (items in the store, add/update tracking, queries).
/// </summary>
public sealed class InMemoryRepository<TEntity>(IEnumerable<TEntity>? seed = null)
    : IRepository<TEntity, Guid>
    where TEntity : Entity
{
    public List<TEntity> Items { get; } = seed?.ToList() ?? [];

    // Explicit tracking lists — useful when tests assert which entities were added/updated.
    public List<TEntity> Added { get; } = [];
    public List<TEntity> Updated { get; } = [];

    public Task<TEntity?> Get(Guid id) =>
        Task.FromResult(Items.FirstOrDefault(e => e.Id == id));

    public Task<IEnumerable<TEntity>> GetAll() =>
        Task.FromResult<IEnumerable<TEntity>>(Items.ToList());

    public Task<List<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate) =>
        Task.FromResult(Items.AsQueryable().Where(predicate).ToList());

    public Task<List<TEntity>> FindAsyncWithQueryParams(
        Expression<Func<TEntity, bool>> predicate,
        Func<IQueryable<TEntity>, IQueryable<TEntity>>? includeFunc)
    {
        var query = Items.AsQueryable().Where(predicate);
        return Task.FromResult((includeFunc?.Invoke(query) ?? query).ToList());
    }

    public IEnumerable<TEntity> Find(Expression<Func<TEntity, bool>> predicate) =>
        Items.AsQueryable().Where(predicate);

    public Task<bool> Exists(Guid id) =>
        Task.FromResult(Items.Any(e => e.Id == id));

    public Task Add(TEntity entity)
    {
        Items.Add(entity);
        Added.Add(entity);
        return Task.CompletedTask;
    }

    public Task AddWithoutSave(TEntity entity)
    {
        Items.Add(entity);
        Added.Add(entity);
        return Task.CompletedTask;
    }

    public Task AddRange(IEnumerable<TEntity> entities)
    {
        var list = entities.ToList();
        Items.AddRange(list);
        Added.AddRange(list);
        return Task.CompletedTask;
    }

    public Task AddRangeWithoutSave(IEnumerable<TEntity> entities)
    {
        var list = entities.ToList();
        Items.AddRange(list);
        Added.AddRange(list);
        return Task.CompletedTask;
    }

    public Task Update(TEntity entity)
    {
        Replace(entity);
        Updated.Add(entity);
        return Task.CompletedTask;
    }

    public bool UpdateWithoutSave(TEntity entity)
    {
        Replace(entity);
        Updated.Add(entity);
        return true;
    }

    public Task Remove(TEntity entity)
    {
        Items.Remove(entity);
        return Task.CompletedTask;
    }

    public Task RemoveRange(IEnumerable<TEntity> entities)
    {
        Items.RemoveAll(entities.Contains);
        return Task.CompletedTask;
    }

    public Task SaveChanges() => Task.CompletedTask;

    private void Replace(TEntity entity)
    {
        var index = Items.FindIndex(e => e.Id == entity.Id);
        if (index >= 0) Items[index] = entity;
    }
}
