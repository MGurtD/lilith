using Application.Contracts;
using Domain.Entities.Production;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Infrastructure.Persistance.Repositories.Production
{
    public class WorkcenterShiftRepository(ApplicationDbContext context) : Repository<WorkcenterShift, Guid>(context), IWorkcenterShiftRepository
    {
        public IWorkcenterShiftDetailRepository Details { get; } = new WorkcenterShiftDetailRepository(context);

        public IQueryable<WorkcenterShift> FindWithDetails(Expression<Func<WorkcenterShift, bool>> predicate)
        {
            return dbSet.Include(ws => ws.Details).Where(predicate);
        }

        public async Task<WorkcenterShift?> GetCurrentWorkcenterShiftWithCurrentDetails(Guid workcenterId)
        {
            var currentWorkcenterShift = await FindWithDetails(ws => ws.Current && ws.WorkcenterId == workcenterId).FirstOrDefaultAsync();
            if (currentWorkcenterShift == null) return null;

            currentWorkcenterShift.Details = currentWorkcenterShift.Details.Where(d => d.Current).ToList();
            return currentWorkcenterShift;
        }

        public async Task<List<WorkcenterShift>> GetCurrentsWithDetails()
        {
            return dbSet
                    .Include(d => d.Details.Where(detail => detail.Current))
                    .Where(d => d.Current)
                    .AsNoTracking()
                    .ToList();
        }

        public async Task<List<WorkcenterShiftHistorical>> GetWorkcenterShiftHistorical(WorkcenterShiftHistoricRequest request)
        {
            var query = context.Set<WorkcenterShiftHistorical>()
                .Where(ws => ws.StartTime >= request.StartTime && ws.EndTime <= request.EndTime);

            // Filter by specific IDs if needed (e.g. if the request had a filter list, but it doesn't currently)

            var rawData = await query.ToListAsync();

            if (request.GroupBy == GroupBy.None && request.TimeGroupBy == TimeGroupBy.None)
            {
                return rawData;
            }

            // Perform grouping in memory to avoid complex EF LINQ translation issues with Views and custom groupings
            var groupedData = rawData.GroupBy(ws =>
            {
                var keyParts = new List<object>();

                switch (request.GroupBy)
                {
                    case GroupBy.Operator:
                        keyParts.Add(ws.OperatorId);
                        break;
                    case GroupBy.Workcenter:
                        keyParts.Add(ws.WorkcenterId);
                        break;
                    case GroupBy.Shift:
                        // Assuming ReferenceCode or similar acts as Shift lookup, 
                        // or we rely on ShiftDetail linked via some ID. 
                        // The view presumably has the shift info. 
                        // If 'ShiftId' isn't on the view directly, we might group by Time/Shift pattern from the ID or similar.
                        // However, looking at the class WorkcenterShiftHistorical, it doesn't have ShiftId directly exposed?
                        // It has 'WorkOrderCode', 'Workcenter', etc.
                        // Let's assume for now we group by the relevant ID if available or just ignore if not present in View model.
                        // The previous errors complained about ShiftId missing. 
                        break;
                    case GroupBy.WorkOrder:
                        keyParts.Add(ws.WorkOrderId);
                        break;
                }

                switch (request.TimeGroupBy)
                {
                    case TimeGroupBy.Shift:
                        // Requires semantic knowledge of shift times, or just grouping by start time if they are distinct shifts
                        keyParts.Add(ws.StartTime); 
                        break;
                    case TimeGroupBy.Day:
                        keyParts.Add(ws.StartTime.Date);
                        break;
                    case TimeGroupBy.Week:
                        // Simple culture-agnostic week grouping or ISO
                        var cal = System.Globalization.CultureInfo.CurrentCulture.Calendar;
                        keyParts.Add(new { ws.StartTime.Year, Week = cal.GetWeekOfYear(ws.StartTime, System.Globalization.CalendarWeekRule.FirstDay, DayOfWeek.Monday) });
                        break;
                    case TimeGroupBy.Month:
                        keyParts.Add(new { ws.StartTime.Year, ws.StartTime.Month });
                        break;
                    case TimeGroupBy.Year:
                        keyParts.Add(ws.StartTime.Year);
                        break;
                }

                return string.Join("|", keyParts); // Simple composite key
            })
            .Select(g => new WorkcenterShiftHistorical
            {
                // We pick the first item to populate common fields (or empty if not relevant)
                Id = Guid.NewGuid(), // Aggregated row
                Key = g.Key,
                
                // Aggregates
                QuantityOk = g.Sum(x => x.QuantityOk),
                QuantityKo = g.Sum(x => x.QuantityKo),
                TotalHours = g.Sum(x => x.TotalHours),
                OperatorCost = g.Sum(x => x.OperatorCost),
                WorkcenterCost = g.Sum(x => x.WorkcenterCost),
                TotalCost = g.Sum(x => x.TotalCost),
                PlannedQuantity = g.First().PlannedQuantity,
                EstimatedMachineCost = g.First().EstimatedMachineCost,
                EstimatedMachineTime = g.First().EstimatedMachineTime,
                EstimatedOperatorCost = g.First().EstimatedOperatorCost,
                EstimatedOperatorTime = g.First().EstimatedOperatorTime,

                // Set context fields if grouped by them
                OperatorId = request.GroupBy == GroupBy.Operator ? g.First().OperatorId : Guid.Empty,
                WorkcenterId = request.GroupBy == GroupBy.Workcenter ? g.First().WorkcenterId : Guid.Empty,
                Workcenter = request.GroupBy == GroupBy.Workcenter ? g.First().Workcenter : (request.GroupBy == GroupBy.Operator ? "Various" : string.Empty),
                Operator = request.GroupBy == GroupBy.Operator ? g.First().Operator : (g.Select(x => x.Operator).Distinct().Count() > 1 ? "Various" : g.First().Operator),
                WorkOrderId = request.GroupBy == GroupBy.WorkOrder ? g.First().WorkOrderId : Guid.Empty,
                WorkOrderCode = request.GroupBy == GroupBy.WorkOrder ? g.First().WorkOrderCode : (g.Select(x => x.WorkOrderCode).Distinct().Count() > 1 ? "Various" : g.First().WorkOrderCode),                                
                ReferenceCode = request.GroupBy == GroupBy.WorkOrder ? g.First().ReferenceCode : (g.Select(x => x.ReferenceCode).Distinct().Count() > 1 ? "Various" : g.First().ReferenceCode),
                ReferenceDescription = request.GroupBy == GroupBy.WorkOrder ? g.First().ReferenceDescription : (g.Select(x => x.ReferenceDescription).Distinct().Count() > 1 ? "Various" : g.First().ReferenceDescription),
                CustomerComercialName = request.GroupBy == GroupBy.WorkOrder ? g.First().CustomerComercialName : (g.Select(x => x.CustomerComercialName).Distinct().Count() > 1 ? "Various" : g.First().CustomerComercialName),
                
                // Time context
                StartTime = g.Min(x => x.StartTime),
                EndTime = g.Max(x => x.EndTime)
            })
            .ToList();

            return groupedData;
        }
    }
}
