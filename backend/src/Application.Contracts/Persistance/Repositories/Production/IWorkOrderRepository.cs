using Application.Contracts.Contracts.Production;
using Domain.Entities.Production;

namespace Application.Contracts
{
    public interface IWorkOrderRepository : IRepository<WorkOrder, Guid>
    {
        
        IWorkOrderPhaseRepository Phases { get; }

        Task<WorkOrder?> GetDetailed(Guid id);

        Task<IEnumerable<WorkOrder>> GetPlannableWorkOrders(Guid[] includedStatusIds);

        /// <summary>
        /// Gets work orders with Planned lifecycle tag for a specific workcenter type.
        /// Returns only WorkOrder entities without phase details.
        /// </summary>
        Task<IEnumerable<WorkOrder>> GetWorkOrdersWithPlannedPhases(Guid workcenterTypeId);

        /// <summary>
        /// Gets work orders by loaded phase IDs with efficient EF Core query.
        /// Loads all related data (phases, reference, customer, status) in a single query.
        /// Used for WebSocket workcenter updates to display loaded work order information.
        /// </summary>
        /// <param name="phaseIds">List of work order phase IDs currently loaded on workcenter</param>
        /// <returns>Work orders with their loaded phases and all navigation properties</returns>
        Task<IEnumerable<WorkOrder>> GetWorkOrdersByLoadedPhaseIds(List<Guid> phaseIds);

        /// <summary>
        /// Gets a work order with all phases including detailed information (phase details, BOM).
        /// Used for Step 2 of work order selection process.
        /// </summary>
        Task<WorkOrder?> GetWorkOrderWithPhasesDetailed(Guid workOrderId);

        Task<IEnumerable<WorkOrderPhaseEstimationDto>>GetWorkcenterLoadBetweenDatesByWorkcenterType(DateTime startDate, DateTime endDate);

    }
}
