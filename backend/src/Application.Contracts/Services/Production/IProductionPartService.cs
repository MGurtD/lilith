using Application.Contracts;
using Domain.Entities.Production;

namespace Application.Contracts;

public interface IProductionPartService
{
    Task<ProductionPart?> GetById(Guid id);
    IEnumerable<ProductionPart> GetByWorkOrderId(Guid workOrderId);
    IEnumerable<ProductionPart> GetBetweenDates(DateTime startTime, DateTime endTime, Guid? workcenterId = null, Guid? operatorId = null, Guid? workorderId = null);
    Task<GenericResponse> Create(ProductionPart productionPart);
    Task<GenericResponse> CreateWithoutCostLookup(ProductionPart productionPart);
    Task<GenericResponse> Update(ProductionPart productionPart);
    Task<GenericResponse> Remove(Guid id);
}
