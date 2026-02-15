using Domain.Entities;
using System.Linq.Expressions;

namespace Application.Contracts
{
    public interface IRepository<TEntity, TId> where TEntity : class
    {
        Task<TEntity?> Get(TId id);
        Task<IEnumerable<TEntity>> GetAll();

        Task<List<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate);

        Task<List<TEntity>> FindAsyncWithQueryParams(Expression<Func<TEntity, bool>> predicate, Func<IQueryable<TEntity>, IQueryable<TEntity>>? includeFunc);


        IEnumerable<TEntity> Find(Expression<Func<TEntity, bool>> predicate);
        Task<bool> Exists(Guid id);

        Task Add(TEntity entity);
        Task AddWithoutSave(TEntity entity);
        Task AddRange(IEnumerable<TEntity> entities);
        Task AddRangeWithoutSave(IEnumerable<TEntity> entities);

        Task Update(TEntity entity);
        bool UpdateWithoutSave(TEntity entity);

        Task Remove(TEntity entity);
        Task RemoveRange(IEnumerable<TEntity> entities);

        Task SaveChanges();

    }
}
