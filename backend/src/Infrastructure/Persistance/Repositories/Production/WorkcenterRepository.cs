using Application.Contracts;
using System.Linq.Expressions;
using Domain.Entities;
using Domain.Entities.Production;
using Domain.Entities.Shared;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistance.Repositories.Production;

public class WorkcenterRepository : Repository<Workcenter, Guid>, IWorkcenterRepository
{
    private readonly ApplicationDbContext _context;

    public WorkcenterRepository(ApplicationDbContext context) : base(context) 
    { 
        _context = context;
    }

    public override async Task<IEnumerable<Workcenter>> GetAll()
    {
        return await dbSet.AsNoTracking().OrderBy(w => w.Description).ToListAsync();
    }

    public async Task<IEnumerable<Workcenter>> GetVisibleInPlant()
    {
        return await dbSet
            .Include(w => w.Area)
            .Include(w => w.WorkcenterType)
            .Include(w => w.Shift)
            .Where(w => !w.Disabled && w.Area != null && w.Area.IsVisibleInPlant)
            .OrderBy(w => w.Name)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<IEnumerable<WorkcenterLoadDto>> GetWorkcenterLoadBetweenDatesByWorkcenterType(DateTime startDate, DateTime endDate)
    {
        // Step 1: Get status IDs with "Available" tag
        var availableStatusIds = await (from status in _context.Set<Status>()
                                        join statusTag in _context.Set<StatusLifecycleTag>() 
                                            on status.Id equals statusTag.StatusId
                                        join tag in _context.Set<LifecycleTag>() 
                                            on statusTag.LifecycleTagId equals tag.Id
                                        where tag.Name == StatusConstants.LifecycleTags.Available
                                        select status.Id)
                                       .ToListAsync();

        if (availableStatusIds.Count == 0)
            return [];

        // Step 2: Query WorkOrders with those status IDs
        return await _context.Set<WorkOrder>()
            .Where(wo => availableStatusIds.Contains(wo.StatusId)
                      && wo.PlannedDate >= startDate 
                      && wo.PlannedDate <= endDate
                      && !wo.Disabled)
            .Include(wo => wo.Phases)
                .ThenInclude(p => p.Details)
            .Include(wo => wo.Phases)
                .ThenInclude(p => p.WorkcenterType)
            .AsNoTracking()
            .SelectMany(wo => wo.Phases
                .Where(wp => wp.WorkcenterType != null)  // Filter out phases without WorkcenterType
                .GroupBy(wp => new 
                { 
                    wo.Code,
                    wo.Order,
                    wo.PlannedDate,
                    wo.PlannedQuantity, 
                    PhaseCode = wp.Code, 
                    PhaseDescription = wp.Description, 
                    wp.WorkcenterTypeId,
                    WorkcenterTypeName = wp.WorkcenterType!.Name  // Use null-forgiving operator since we filtered nulls
                })
                .Select(g => new WorkcenterLoadDto
                {
                    WorkOrderCode = g.Key.Code,
                    WorkOrderPriority = g.Key.Order,
                    WorkOrderPlannedDate = g.Key.PlannedDate,
                    PlannedQuantity = g.Key.PlannedQuantity,
                    PhaseCode = g.Key.PhaseCode,
                    PhaseDescription = g.Key.PhaseDescription,
                    WorkcenterTypeId = g.Key.WorkcenterTypeId,
                    WorkcenterTypeName = g.Key.WorkcenterTypeName,
                    EstimatedTime = g.SelectMany(wp => wp.Details)
                        .Sum(wd => wd.IsCycleTime 
                            ? g.Key.PlannedQuantity * wd.EstimatedTime 
                            : wd.EstimatedTime)
                }))
            .ToListAsync();
    }
}