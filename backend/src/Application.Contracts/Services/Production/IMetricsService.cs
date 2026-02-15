using Application.Contracts;
using Domain.Entities.Production;

namespace Application.Contracts
{
    public interface IMetricsService
    {
        Task<GenericResponse> GetOperatorCost(Guid operatorId);
        Task<GenericResponse> GetWorkcenterStatusCost(Guid workcenterId, Guid statusId);

        Task<GenericResponse> GetWorkmasterMetrics(WorkMaster workMaster, decimal? productedQuantity);
        Task<GenericResponse> GetProductionPartCosts(ProductionPart productionPart);
    }
}
