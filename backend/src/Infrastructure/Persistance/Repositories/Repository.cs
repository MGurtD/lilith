using Application.Contracts;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using SkiaSharp;
using System.Linq.Expressions;

namespace Infrastructure.Persistance.Repositories
{
    public class Repository<Entity, Id> : IRepository<Entity, Id> where Entity : Domain.Entities.Entity
    {
        protected ApplicationDbContext context;
        internal DbSet<Entity> dbSet;

        public Repository(ApplicationDbContext context)
        {
            this.context = context;
            dbSet = context.Set<Entity>();
        }

        public virtual async Task<Entity?> Get(Id id)
        {
            return await dbSet.FindAsync(id);
        }

        public virtual async Task<IEnumerable<Entity>> GetAll()
        {
            return await dbSet.AsNoTracking().ToListAsync();
        }

        public async Task<List<Entity>> FindAsync(Expression<Func<Entity, bool>> predicate)
        {
            return await dbSet.AsNoTracking().Where(predicate).ToListAsync();
        }

        public async Task<List<Entity>> FindAsyncWithQueryParams(Expression<Func<Entity, bool>> predicate, Func<IQueryable<Entity>, IQueryable<Entity>>? includeFunc)
        {
            IQueryable<Entity> query = dbSet.Where(predicate);

            if (includeFunc != null)
            {
                query = includeFunc(query);
            }

            return await query.ToListAsync();
        }

        public virtual IEnumerable<Entity> Find(Expression<Func<Entity, bool>> predicate)
        {
            var entities = dbSet.AsNoTracking().Where(predicate);
            return entities;
        }

        public virtual async Task<bool> Exists(Guid Id)
        {
            return await dbSet.AnyAsync(e => e.Id == Id);
        }

        public async Task Add(Entity entity)
        {
            await dbSet.AddAsync(entity);
            await context.SaveChangesAsync();

            context.Entry(entity).State = EntityState.Detached;
        }
        public async Task AddWithoutSave(Entity entity)
        {
            await dbSet.AddAsync(entity);
        }

        public async Task AddRange(IEnumerable<Entity> entities)
        {
            await dbSet.AddRangeAsync(entities);
            await context.SaveChangesAsync();
        }

        public async Task AddRangeWithoutSave(IEnumerable<Entity> entities)
        {
            await dbSet.AddRangeAsync(entities);
        }

        public async Task Update(Entity entity)
        {
            entity.UpdatedOn = DateTime.Now;
            context.Entry(entity).State = EntityState.Modified;
            dbSet.Update(entity);            
            await context.SaveChangesAsync();

            context.Entry(entity).State = EntityState.Detached;
        }

        public bool UpdateWithoutSave(Entity entity)
        {
            entity.UpdatedOn = DateTime.Now;

            dbSet.Update(entity);
            return true;
        }

        public async Task Remove(Entity entity)
        {
            dbSet.Remove(entity);
            await context.SaveChangesAsync();
        }

        public async Task RemoveRange(IEnumerable<Entity> entities)
        {
            dbSet.RemoveRange(entities);
            await context.SaveChangesAsync();
        }

        public async Task SaveChanges()
        {
            await context.SaveChangesAsync();
        }

    }
}
