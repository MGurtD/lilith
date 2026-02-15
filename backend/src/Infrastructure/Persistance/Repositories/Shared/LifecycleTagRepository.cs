using Domain.Entities;
using Domain.Entities.Shared;
using Application.Contracts;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistance.Repositories;

public class LifecycleTagRepository(ApplicationDbContext context) : Repository<LifecycleTag, Guid>(context), ILifecycleTagRepository
{
    public async Task<List<LifecycleTag>> GetByLifecycleId(Guid lifecycleId)
    {
        return await dbSet
            .AsNoTracking()
            .Where(lt => lt.LifecycleId == lifecycleId && !lt.Disabled)
            .OrderBy(lt => lt.Name)
            .ToListAsync();
    }

    public async Task<List<Status>> GetStatusesByTag(Guid lifecycleTagId)
    {
        return await context.Set<StatusLifecycleTag>()
            .AsNoTracking()
            .Include(slt => slt.Status)
            .Where(slt => slt.LifecycleTagId == lifecycleTagId && !slt.Disabled && slt.Status != null && !slt.Status.Disabled)
            .Select(slt => slt.Status!)
            .ToListAsync();
    }

    public async Task<List<Status>> GetStatusesByTagName(string tagName, Guid lifecycleId)
    {
        return await context.Set<StatusLifecycleTag>()
            .AsNoTracking()
            .Include(slt => slt.Status)
            .Include(slt => slt.LifecycleTag)
            .Where(slt => 
                slt.LifecycleTag!.Name == tagName && 
                slt.LifecycleTag.LifecycleId == lifecycleId && 
                !slt.Disabled && 
                !slt.LifecycleTag.Disabled && 
                slt.Status != null && 
                !slt.Status.Disabled)
            .Select(slt => slt.Status!)
            .ToListAsync();
    }

    public async Task<bool> ExistsInLifecycle(string tagName, Guid lifecycleId, Guid? excludeId = null)
    {
        var query = dbSet.Where(lt => 
            lt.LifecycleId == lifecycleId && 
            lt.Name == tagName && 
            !lt.Disabled);

        if (excludeId.HasValue)
        {
            query = query.Where(lt => lt.Id != excludeId.Value);
        }

        return await query.AnyAsync();
    }

    public async Task<List<LifecycleTag>> GetTagsByStatus(Guid statusId)
    {
        return await context.Set<StatusLifecycleTag>()
            .AsNoTracking()
            .Include(slt => slt.LifecycleTag)
            .Where(slt => slt.StatusId == statusId && !slt.Disabled)
            .Select(slt => slt.LifecycleTag!)
            .ToListAsync();
    }
}
