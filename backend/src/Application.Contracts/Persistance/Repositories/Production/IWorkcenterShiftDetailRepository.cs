using Domain.Entities.Production;

namespace Application.Contracts
{
    public interface IWorkcenterShiftDetailRepository : IRepository<WorkcenterShiftDetail, Guid>
    {
        /// <summary>
        /// Calculates total machine time in minutes for a specific phase and machine status.
        /// Sums time from StartTime to EndTime (or current time if EndTime is null).
        /// </summary>
        /// <param name="phaseId">Work order phase ID</param>
        /// <param name="machineStatusId">Machine status ID to filter by</param>
        /// <returns>Total time in minutes</returns>
        Task<decimal> GetTotalMachineTimeByPhaseAndStatus(Guid phaseId, Guid machineStatusId);

        /// <summary>
        /// Calculates total operator time in minutes for a specific phase and operator.
        /// Time is divided by ConcurrentOperatorWorkcenters for each record to account for shared time.
        /// Sums time from StartTime to EndTime (or current time if EndTime is null).
        /// </summary>
        /// <param name="phaseId">Work order phase ID</param>
        /// <param name="operatorId">Operator ID to filter by</param>
        /// <returns>Total time in minutes (adjusted for concurrent operators)</returns>
        Task<decimal> GetTotalOperatorTimeByPhaseAndOperator(Guid phaseId, Guid operatorId);
    }
}
