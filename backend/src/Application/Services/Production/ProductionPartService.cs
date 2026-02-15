using Application.Contracts;
using Domain.Entities.Production;

namespace Application.Services.Production;

public class ProductionPartService(
    IUnitOfWork unitOfWork,
    IMetricsService metricsService,
    IWorkOrderService workOrderService,
    ILocalizationService localizationService) : IProductionPartService
{
    public async Task<ProductionPart?> GetById(Guid id)
    {
        return await unitOfWork.ProductionParts.Get(id);
    }

    public IEnumerable<ProductionPart> GetByWorkOrderId(Guid workOrderId)
    {
        return unitOfWork.ProductionParts.Find(pp => pp.WorkOrderId == workOrderId);
    }

    public IEnumerable<ProductionPart> GetBetweenDates(DateTime startTime, DateTime endTime, Guid? workcenterId = null, Guid? operatorId = null, Guid? workorderId = null)
    {
        var productionParts = unitOfWork.ProductionParts.Find(r => r.Date >= startTime && r.Date <= endTime);

        if (workcenterId.HasValue)
            productionParts = productionParts.Where(r => r.WorkcenterId == workcenterId);
        if (operatorId.HasValue)
            productionParts = productionParts.Where(r => r.OperatorId == operatorId);
        if (workorderId.HasValue)
            productionParts = productionParts.Where(r => r.WorkOrderId == workorderId);

        return productionParts;
    }

    public async Task<GenericResponse> Create(ProductionPart productionPart)
    {
        // Check if already exists
        var exists = unitOfWork.ProductionParts.Find(pp => pp.Id == productionPart.Id).Any();
        if (exists)
            return new GenericResponse(false, localizationService.GetLocalizedString("EntityAlreadyExists"));

        // Get current costs from metrics service
        var costsRequest = await metricsService.GetProductionPartCosts(productionPart);
        if (costsRequest.Result && costsRequest.Content is ProductionMetrics costs)
        {
            productionPart.MachineHourCost = costs.MachineCost;
            productionPart.OperatorHourCost = costs.OperatorCost;
        }

        // Add production part
        await unitOfWork.ProductionParts.Add(productionPart);

        // Add to work order
        await workOrderService.AddProductionPart(productionPart.WorkOrderId, productionPart);

        return new GenericResponse(true, productionPart);
    }

    public async Task<GenericResponse> Update(ProductionPart productionPart)
    {
        var exists = await unitOfWork.ProductionParts.Exists(productionPart.Id);
        if (!exists)
            return new GenericResponse(false, localizationService.GetLocalizedString("EntityNotFound", productionPart.Id));

        await unitOfWork.ProductionParts.Update(productionPart);
        return new GenericResponse(true, productionPart);
    }

    public async Task<GenericResponse> Remove(Guid id)
    {
        var entity = unitOfWork.ProductionParts.Find(e => e.Id == id).FirstOrDefault();
        if (entity is null)
            return new GenericResponse(false, localizationService.GetLocalizedString("EntityNotFound", id));

        // Remove from work order first
        await workOrderService.RemoveProductionPart(entity.WorkOrderId, entity);

        // Then remove from repository
        await unitOfWork.ProductionParts.Remove(entity);

        return new GenericResponse(true, entity);
    }
}
