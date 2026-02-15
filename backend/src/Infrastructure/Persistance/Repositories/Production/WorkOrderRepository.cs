using System.Linq.Expressions;
using Application.Contracts;
using Domain.Entities;
using Domain.Entities.Production;
using Domain.Entities.Shared;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistance.Repositories.Production
{
    public class WorkOrderRepository(ApplicationDbContext context) : Repository<WorkOrder, Guid>(context), IWorkOrderRepository
    {
        public IWorkOrderPhaseRepository Phases { get; } = new WorkOrderPhaseRepository(context);

        public override async Task<IEnumerable<WorkOrder>> GetAll()
        {
            return await dbSet
                        .Include(d => d.Reference)
                        .AsNoTracking()
                        .ToListAsync();
        }

        public override IEnumerable<WorkOrder> Find(Expression<Func<WorkOrder, bool>> predicate) {
            return dbSet
                    .Include(d => d.Reference)
                    .Where(predicate)
                    .AsNoTracking()
                    .OrderBy(w => w.Code);
        }

        public override async Task<WorkOrder?> Get(Guid id)
        {
            return await dbSet
                        .Include(d => d.Reference)
                        .Include(d => d.Phases)
                        .AsNoTracking()
                        .FirstOrDefaultAsync(e => e.Id == id);
        }

        public async Task<WorkOrder?> GetDetailed(Guid id)
        {
            return await dbSet
                        .Include(d => d.Reference)
                        .Include(d => d.Phases)
                            .ThenInclude(p => p.Details)
                        .Include(d => d.Phases)
                            .ThenInclude(d => d.BillOfMaterials)
                        .AsNoTracking()
                        .FirstOrDefaultAsync(e => e.Id == id);
        }

        public async Task<IEnumerable<WorkOrder>> GetPlannableWorkOrders(Guid[] includedStatusIds)
        {
            return await dbSet
                .Where(w => includedStatusIds.Contains(w.StatusId))
                .Include(w => w.Reference)
                    .ThenInclude(r => r!.Customer)
                .Include(w => w.Status)            
                .AsNoTracking()
                .OrderBy(w => w.Order)
                    .ThenBy(w => w.PlannedDate)
                .ToListAsync();
        }

        /// <summary>
        /// Gets work orders with Planned lifecycle tag for a specific workcenter type.
        /// Filters by WorkOrder status with Planned lifecycle tag at WorkOrder level.
        /// Returns only WorkOrder entities without phase details.
        /// </summary>
        public async Task<IEnumerable<WorkOrder>> GetWorkOrdersWithPlannedPhases(Guid workcenterTypeId)
        {
            // Get WorkOrder IDs with Planned lifecycle tag at WorkOrder level
            var workOrderIdsQuery = from wo in context.Set<WorkOrder>()
                                    join status in context.Set<Status>() 
                                        on wo.StatusId equals status.Id
                                    join statusTag in context.Set<StatusLifecycleTag>() 
                                        on status.Id equals statusTag.StatusId
                                    join tag in context.Set<LifecycleTag>() 
                                        on statusTag.LifecycleTagId equals tag.Id
                                    where tag.Name == StatusConstants.LifecycleTags.Plant
                                        && !wo.Disabled
                                        && wo.Phases.Any(p => p.WorkcenterTypeId == workcenterTypeId && !p.Disabled)
                                    select wo.Id;

            var workOrderIds = await workOrderIdsQuery.ToListAsync();
            
            if (workOrderIds.Count == 0)
                return [];

            // Load WorkOrders with navigation properties
            var workOrders = await dbSet
                .Where(wo => workOrderIds.Contains(wo.Id))
                .Include(wo => wo.Status)
                .Include(wo => wo.Reference)
                    .ThenInclude(r => r!.Customer)
                .AsNoTracking()
                .OrderBy(w => w.Order)
                    .ThenBy(w => w.PlannedDate)
                .ToListAsync();

            return workOrders;
        }

        public async Task<WorkOrder?> GetWorkOrderWithPhasesDetailed(Guid workOrderId)
        {
            return await dbSet
                .Include(wo => wo.Reference)
                    .ThenInclude(r => r!.Customer)
                .Include(wo => wo.Phases)
                    .ThenInclude(p => p.Status)
                .Include(wo => wo.Phases)
                    .ThenInclude(p => p.PreferredWorkcenter)
                .Include(wo => wo.Phases)
                    .ThenInclude(p => p.Details)
                        .ThenInclude(d => d.MachineStatus)
                .Include(wo => wo.Phases)
                    .ThenInclude(p => p.BillOfMaterials)
                        .ThenInclude(bom => bom.Reference)
                .AsNoTracking()
                .FirstOrDefaultAsync(wo => wo.Id == workOrderId);
        }

        /// <summary>
        /// Gets work orders by loaded phase IDs with efficient single EF Core query.
        /// Includes all navigation properties needed for DTO transformation.
        /// </summary>
        public async Task<IEnumerable<WorkOrder>> GetWorkOrdersByLoadedPhaseIds(List<Guid> phaseIds)
        {
            if (phaseIds == null || phaseIds.Count == 0)
                return [];

            // Single efficient query with all necessary includes
            var workOrders = await dbSet
                .Where(wo => wo.Phases.Any(p => phaseIds.Contains(p.Id)) && !wo.Disabled)
                .Include(wo => wo.Status)
                .Include(wo => wo.Reference)
                    .ThenInclude(r => r!.Customer)
                .Include(wo => wo.Phases.Where(p => phaseIds.Contains(p.Id)))
                    .ThenInclude(p => p.Status)
                .Include(wo => wo.Phases.Where(p => phaseIds.Contains(p.Id)))
                    .ThenInclude(p => p.PreferredWorkcenter)
                .Include(wo => wo.Phases.Where(p => phaseIds.Contains(p.Id)))
                    .ThenInclude(p => p.Details.OrderBy(d => d.Order))
                        .ThenInclude(d => d.MachineStatus)
                .AsNoTracking()
                .OrderBy(w => w.Order)
                    .ThenBy(w => w.PlannedDate)
                .ToListAsync();

            return workOrders;
        }

        public async Task<IEnumerable<WorkOrderPhaseEstimationDto>> GetWorkcenterLoadBetweenDatesByWorkcenterType(DateTime startDate, DateTime endDate)
        {
            return await dbSet
                .Where(wo => wo.PlannedDate >= startDate && wo.PlannedDate <= endDate)
                .Include(wo => wo.Phases)
                    .ThenInclude(p => p.Details)
                .AsNoTracking()
                .SelectMany(wo => wo.Phases
                            .GroupBy(wp => new 
                            {
                                wo.Code,
                                wo.PlannedQuantity,
                                PhaseCode = wp.Code,
                                PhaseDescription = wp.Description,
                                wp.WorkcenterTypeId                                
                            })
                .Select(g => new WorkOrderPhaseEstimationDto
                {
                    Code = g.Key.Code,
                    PlannedQuantity = g.Key.PlannedQuantity,
                    PhaseCode = g.Key.PhaseCode,
                    PhaseDescription = g.Key.PhaseDescription,
                    WorkcenterTypeId = g.Key.WorkcenterTypeId,
                    EstimatedTime = g.SelectMany(wp => wp.Details)
                        .Sum(wd => wd.IsCycleTime
                            ? g.Key.PlannedQuantity * wd.EstimatedTime
                            : wd.EstimatedTime
                        )
                }))
                .ToListAsync();
        }

    }
}
