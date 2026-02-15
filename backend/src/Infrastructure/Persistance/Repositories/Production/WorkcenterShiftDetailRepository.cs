using Application.Contracts;
using Domain.Entities.Production;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistance.Repositories.Production
{
    public class WorkcenterShiftDetailRepository(ApplicationDbContext context) : Repository<WorkcenterShiftDetail, Guid>(context), IWorkcenterShiftDetailRepository
    {
        /// <inheritdoc />
        public async Task<decimal> GetTotalMachineTimeByPhaseAndStatus(Guid phaseId, Guid machineStatusId)
        {
            var now = DateTime.Now;

            var details = await dbSet
                .Where(d => d.WorkOrderPhaseId == phaseId && d.MachineStatusId == machineStatusId && !d.Disabled)
                .ToListAsync();

            decimal totalMinutes = 0;

            foreach (var detail in details)
            {
                var endTime = detail.EndTime ?? now;
                var duration = endTime - detail.StartTime;
                totalMinutes += (decimal)duration.TotalMinutes;
            }

            return totalMinutes;
        }

        /// <inheritdoc />
        public async Task<decimal> GetTotalOperatorTimeByPhaseAndOperator(Guid phaseId, Guid operatorId)
        {
            var now = DateTime.Now;

            var details = await dbSet
                .Where(d => d.WorkOrderPhaseId == phaseId && d.OperatorId == operatorId && !d.Disabled)
                .ToListAsync();

            decimal totalMinutes = 0;

            foreach (var detail in details)
            {
                var endTime = detail.EndTime ?? now;
                var duration = endTime - detail.StartTime;
                var minutes = (decimal)duration.TotalMinutes;

                // Divide by concurrent operators to get fair share of time
                var concurrentOperators = detail.ConcurrentOperatorWorkcenters > 0 
                    ? detail.ConcurrentOperatorWorkcenters 
                    : 1;

                totalMinutes += minutes / concurrentOperators;
            }

            return totalMinutes;
        }
    }
}
