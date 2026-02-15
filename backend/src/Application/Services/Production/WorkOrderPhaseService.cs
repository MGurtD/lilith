using Application.Contracts;
using Application.Contracts.Contracts.Production;
using Domain.Entities.Production;
using Microsoft.Extensions.Logging;

namespace Application.Services.Production;

public class WorkOrderPhaseService(
    IUnitOfWork unitOfWork, 
    ILocalizationService localizationService,
    IWorkOrderService workOrderService,
    ILogger<WorkOrderPhaseService> logger) : IWorkOrderPhaseService
{
    #region Phase CRUD
    
    public async Task<WorkOrderPhase?> GetById(Guid id)
    {
        return await unitOfWork.WorkOrders.Phases.Get(id);
    }

    public async Task<GenericResponse> Create(WorkOrderPhase phase)
    {
        // Check if phase already exists
        var exists = unitOfWork.WorkOrders.Phases.Find(p => p.Id == phase.Id).Any();
        if (exists)
            return new GenericResponse(false, 
                localizationService.GetLocalizedString("WorkOrderPhaseAlreadyExists"));

        // Validate parent WorkOrder exists
        var workOrder = await unitOfWork.WorkOrders.Get(phase.WorkOrderId);
        if (workOrder is null)
            return new GenericResponse(false, 
                localizationService.GetLocalizedString("WorkOrderNotFound", phase.WorkOrderId));

        // Create phase
        await unitOfWork.WorkOrders.Phases.Add(phase);
        return new GenericResponse(true, phase);
    }

    public async Task<GenericResponse> Update(WorkOrderPhase phase)
    {
        // Check if phase exists
        var exists = await unitOfWork.WorkOrders.Phases.Exists(phase.Id);
        if (!exists)
            return new GenericResponse(false, 
                localizationService.GetLocalizedString("WorkOrderPhaseNotFound"));

        // Update phase
        await unitOfWork.WorkOrders.Phases.Update(phase);
        return new GenericResponse(true, phase);
    }

    public async Task<GenericResponse> Remove(Guid id)
    {
        var entity = unitOfWork.WorkOrders.Phases.Find(p => p.Id == id).FirstOrDefault();
        if (entity is null)
            return new GenericResponse(false, 
                localizationService.GetLocalizedString("WorkOrderPhaseNotFound"));

        await unitOfWork.WorkOrders.Phases.Remove(entity);
        return new GenericResponse(true, entity);
    }
    
    #endregion

    #region Phase Lifecycle
    
    public async Task<GenericResponse> StartPhase(Guid phaseId, Guid? workOrderStatusId = null)
    {
        // Get phase
        var phase = await unitOfWork.WorkOrders.Phases.Get(phaseId);
        if (phase == null)
            return new GenericResponse(false, 
                localizationService.GetLocalizedString("WorkOrderPhaseNotFound"));
        
        // Get "Producció" status
        var productionStatus = await unitOfWork.Lifecycles.GetStatusByName(
            StatusConstants.Lifecycles.WorkOrder, 
            StatusConstants.Statuses.Production);
        
        if (productionStatus == null)
        {
            logger.LogError("Production status not found in WorkOrder lifecycle");
            return new GenericResponse(false, 
                localizationService.GetLocalizedString("StatusNotFound", StatusConstants.Statuses.Production));
        }
        
        if (!phase.StartTime.HasValue)
        {
            phase.StartTime = DateTime.Now;
        }
        if (phase.EndTime.HasValue)
        {
            phase.EndTime = null;
        }

        phase.StatusId = productionStatus.Id;
        await unitOfWork.WorkOrders.Phases.Update(phase);
        
        // Update parent WorkOrder
        var workOrder = await unitOfWork.WorkOrders.Get(phase.WorkOrderId);
        if (workOrder != null)
        {
            workOrder.Phases = []; // Clear phases to avoid tracking issues
            workOrder.StatusId = productionStatus.Id;

            if (!workOrder.StartTime.HasValue)
            {
                workOrder.StartTime = DateTime.Now;
            }
            if (workOrder.EndTime.HasValue)
            {
                workOrder.EndTime = null;
            }
            
            await unitOfWork.WorkOrders.Update(workOrder);
        }
        
        return new GenericResponse(true, phase);
    }

    public async Task<GenericResponse> EndPhase(Guid phaseId, Guid workOrderStatusId)
    {
        // Get phase
        var phase = await unitOfWork.WorkOrders.Phases.Get(phaseId);
        if (phase == null)
            return new GenericResponse(false, 
                localizationService.GetLocalizedString("WorkOrderPhaseNotFound"));
        
        // Validate phase has been started
        if (!phase.StartTime.HasValue)
            return new GenericResponse(false, 
                localizationService.GetLocalizedString("WorkOrderPhaseNotStarted"));

        var outStatus = await unitOfWork.Lifecycles.StatusRepository.Get(workOrderStatusId);
        if (outStatus == null)
        {
            logger.LogError("Status with id {workOrderStatusId} not found in WorkOrder lifecycle", workOrderStatusId);
            return new GenericResponse(false, 
                localizationService.GetLocalizedString("StatusNotFound"));
        }
        
        // Update phase
        phase.StatusId = outStatus.Id;
        phase.EndTime = DateTime.Now;
        await unitOfWork.WorkOrders.Phases.Update(phase);
        
        // Delegate work order status update to WorkOrderService
        var updateResult = await workOrderService.UpdateStatusAfterPhaseEnd(
            phase.WorkOrderId, 
            phaseId, 
            workOrderStatusId);
        
        if (!updateResult.Result)
            return updateResult;
        
        return new GenericResponse(true);
    }
    
    #endregion

    #region Special Queries
    
    public async Task<IEnumerable<object>> GetExternalPhases(DateTime startTime, DateTime endTime)
    {
        // Get production status
        var status = await unitOfWork.Lifecycles.GetStatusByName(
            StatusConstants.Lifecycles.WorkOrder, 
            StatusConstants.Statuses.Production);
        
        if (status == null)
        {
            return [];
        }

        // Find work orders in production within date range
        var workOrders = await unitOfWork.WorkOrders.FindAsync(w =>
            w.StatusId == status.Id &&
            w.PlannedDate >= startTime &&
            w.PlannedDate < endTime);

        // Find external work phases without purchase orders
        var workOrderPhases = await unitOfWork.WorkOrders.Phases.FindAsync(p =>
            p.IsExternalWork == true &&
            p.ServiceReferenceId != null &&
            p.PurchaseOrderId == null);

        // Join phases with work orders
        var workOrderPhaseJoin = from w in workOrders
                                 join p in workOrderPhases
                                 on w.Id equals p.WorkOrderId
                                 select new
                                 {
                                     WorkOrder = w,
                                     Phase = p
                                 };

        return workOrderPhaseJoin;
    }

    public async Task<IEnumerable<WorkOrderWithPhasesDto>> GetPlannedByWorkcenterType(
        Guid workcenterTypeId)
    {
        // Get work orders from repository (efficient EF query)
        var workOrders = await unitOfWork.WorkOrders
            .GetWorkOrdersWithPlannedPhases(workcenterTypeId);

        // Transform entities to DTOs (without nested phases)
        var results = workOrders.Select(wo => new WorkOrderWithPhasesDto
        {
            WorkOrderId = wo.Id,
            WorkOrderCode = wo.Code,
            CustomerName = wo.Reference?.Customer?.ComercialName ?? string.Empty,
            SalesReferenceId = wo.ReferenceId,
            SalesReferenceDisplay = wo.Reference != null 
                ? $"{wo.Reference.Code} - {wo.Reference.Description}"
                : string.Empty,
            PlannedQuantity = wo.PlannedQuantity,
            PlannedDate = wo.PlannedDate,
            StartTime = wo.StartTime,
            WorkOrderStatus = wo.Status?.Name ?? string.Empty,
            Priority = wo.Order
        }).ToList();

        return results;
    }

    public async Task<IEnumerable<WorkOrderWithPhasesDto>> GetLoadedWorkOrdersByPhaseIds(List<Guid> phaseIds)
    {
        if (phaseIds == null || phaseIds.Count == 0)
            return [];

        // Use optimized repository method with efficient EF Core query
        var workOrders = await unitOfWork.WorkOrders.GetWorkOrdersByLoadedPhaseIds(phaseIds);
        
        if (!workOrders.Any())
            return [];

        // Transform to DTOs
        var results = new List<WorkOrderWithPhasesDto>();

        foreach (var wo in workOrders)
        {
            var plannedPhases = new List<PlannedPhaseDto>();

            foreach (var phase in wo.Phases)
            {
                // Map phase details to PhaseDetailForPlantDto (for activity buttons)
                var phaseDetails = phase.Details
                    .Where(d => d.MachineStatusId != null && d.MachineStatus != null)
                    .OrderBy(d => d.Order)
                    .Select(d => new PhaseDetailForPlantDto
                    {
                        MachineStatusId = d.MachineStatusId,
                        MachineStatusName = d.MachineStatus?.Name ?? string.Empty,
                        MachineStatusColor = d.MachineStatus?.Color ?? string.Empty,
                        MachineStatusIcon = d.MachineStatus?.Icon ?? string.Empty,
                        Order = d.Order,
                        EstimatedTime = d.EstimatedTime,
                        EstimatedOperatorTime = d.EstimatedOperatorTime,
                        Comment = d.Comment ?? string.Empty
                    })
                    .ToList();

                plannedPhases.Add(new PlannedPhaseDto
                {
                    PhaseId = phase.Id,
                    PhaseCode = phase.Code,
                    PhaseDescription = phase.Description ?? string.Empty,
                    PhaseDisplay = $"{phase.Code} - {phase.Description}",
                    PhaseStatusId = phase.StatusId,
                    PhaseStatus = phase.Status?.Name ?? string.Empty,
                    StartTime = phase.StartTime,
                    PreferredWorkcenterName = phase.PreferredWorkcenter?.Name ?? string.Empty,
                    IsExternalWork = phase.IsExternalWork,
                    QuantityOk = phase.QuantityOk,
                    QuantityKo = phase.QuantityKo,
                    Comment = phase.Comment ?? string.Empty,
                    Details = phaseDetails
                });
            }

            results.Add(new WorkOrderWithPhasesDto
            {
                WorkOrderId = wo.Id,
                WorkOrderCode = wo.Code,
                CustomerName = wo.Reference?.Customer?.ComercialName ?? string.Empty,
                SalesReferenceId = wo.ReferenceId,
                SalesReferenceDisplay = wo.Reference != null 
                    ? $"{wo.Reference.Code} - {wo.Reference.Description}"
                    : string.Empty,
                PlannedQuantity = wo.PlannedQuantity,
                PlannedDate = wo.PlannedDate,
                StartTime = wo.StartTime,
                WorkOrderStatus = wo.Status?.Name ?? string.Empty,
                Priority = wo.Order,
                Comment = wo.Comment ?? string.Empty,
                Phases = plannedPhases
            });
        }

        return results;
    }

    public async Task<IEnumerable<WorkOrderPhaseDetailedDto>> GetWorkOrderPhasesDetailed(Guid workOrderId)
    {
        var workOrder = await unitOfWork.WorkOrders.GetWorkOrderWithPhasesDetailed(workOrderId);
        
        if (workOrder == null || workOrder.Phases == null)
            return [];

        var results = new List<WorkOrderPhaseDetailedDto>();

        // Build sales reference display (Customer + Reference + Version)
        var salesReferenceDisplay = string.Empty;
        if (workOrder.Reference != null)
        {
            var customerName = workOrder.Reference.Customer?.ComercialName ?? string.Empty;
            var referenceCode = workOrder.Reference.Code ?? string.Empty;
            var version = !string.IsNullOrEmpty(workOrder.Reference.Version) 
                ? $" v{workOrder.Reference.Version}" 
                : string.Empty;
            
            salesReferenceDisplay = $"{customerName} - {referenceCode}{version}";
        }

        foreach (var phase in workOrder.Phases.Where(p => !p.Disabled))
        {
            var detailedPhase = new WorkOrderPhaseDetailedDto
            {
                WorkOrderId = workOrder.Id,
                WorkOrderCode = workOrder.Code,
                SalesReferenceDisplay = salesReferenceDisplay,
                PlannedQuantity = workOrder.PlannedQuantity,
                PhaseId = phase.Id,
                PhaseCode = phase.Code,
                PhaseDescription = phase.Description,
                PhaseStatusId = phase.StatusId,
                PhaseStatus = phase.Status?.Name ?? string.Empty,
                StartTime = phase.StartTime,
                EndTime = phase.EndTime,
                QuantityOk = phase.QuantityOk,
                QuantityKo = phase.QuantityKo,
                Comment = phase.Comment ?? string.Empty,
                PreferredWorkcenterName = phase.PreferredWorkcenter?.Name ?? string.Empty,
                IsExternalWork = phase.IsExternalWork,
                WorkcenterTypeId = phase.WorkcenterTypeId ?? Guid.Empty
            };

            // Map phase details
            if (phase.Details != null)
            {
                foreach (var detail in phase.Details.Where(d => !d.Disabled))
                {
                    detailedPhase.Details.Add(new PhaseDetailItemDto
                    {
                        MachineStatusId = detail.MachineStatusId,
                        MachineStatusName = detail.MachineStatus?.Name ?? string.Empty,
                        EstimatedTime = detail.EstimatedTime,
                        EstimatedOperatorTime = detail.EstimatedOperatorTime,
                        IsCycleTime = detail.IsCycleTime,
                        Comment = detail.Comment ?? string.Empty
                    });
                }
            }

            // Map bill of materials
            if (phase.BillOfMaterials != null)
            {
                foreach (var bom in phase.BillOfMaterials.Where(b => !b.Disabled))
                {
                    detailedPhase.BillOfMaterials.Add(new BillOfMaterialsItemDto
                    {
                        ReferenceCode = bom.Reference?.Code ?? string.Empty,
                        ReferenceDescription = bom.Reference?.Description ?? string.Empty,
                        Quantity = bom.Quantity
                    });
                }
            }

            results.Add(detailedPhase);
        }

        return results;
    }
    
    public async Task<NextPhaseInfoDto?> GetNextPhaseForWorkcenter(Guid workcenterId, Guid currentPhaseId)
    {
        // Get the workcenter to find its type
        var workcenter = await unitOfWork.Workcenters.Get(workcenterId);
        if (workcenter == null)
            return null;

        var workcenterTypeId = workcenter.WorkcenterTypeId;

        // Get the current phase to find the work order and current phase code
        var currentPhase = await unitOfWork.WorkOrders.Phases.Get(currentPhaseId);
        if (currentPhase == null)
            return null;

        var workOrderId = currentPhase.WorkOrderId;
        var currentPhaseCode = currentPhase.Code;

        // Find next phases that match the criteria:
        // - Same work order
        // - Phase code > current phase code (lexicographically)
        // - Same workcenter type
        // - Not external work
        // - Not completed (EndTime is null)
        var nextPhase = unitOfWork.WorkOrders.Phases
            .Find(p => 
                p.WorkOrderId == workOrderId &&
                p.WorkcenterTypeId == workcenterTypeId &&
                !p.Disabled &&
                !p.IsExternalWork &&
                p.Code.CompareTo(currentPhaseCode) > 0)
            .OrderBy(p => p.Code)
            .FirstOrDefault();

        if (nextPhase == null)
            return null;

        return new NextPhaseInfoDto
        {
            PhaseId = nextPhase.Id,
            PhaseCode = nextPhase.Code,
            PhaseDescription = nextPhase.Description
        };
    }
    
    #endregion

    #region PhaseDetail CRUD
    
    public async Task<WorkOrderPhaseDetail?> GetDetailById(Guid id)
    {
        return await unitOfWork.WorkOrders.Phases.Details.Get(id);
    }

    public async Task<GenericResponse> CreateDetail(WorkOrderPhaseDetail detail)
    {
        // Check if detail already exists
        var exists = unitOfWork.WorkOrders.Phases.Details.Find(d => d.Id == detail.Id).Any();
        if (exists)
            return new GenericResponse(false, 
                localizationService.GetLocalizedString("WorkOrderPhaseDetailAlreadyExists"));

        // Create detail
        await unitOfWork.WorkOrders.Phases.Details.Add(detail);
        return new GenericResponse(true, detail);
    }

    public async Task<GenericResponse> UpdateDetail(WorkOrderPhaseDetail detail)
    {
        // Check if detail exists
        var exists = await unitOfWork.WorkOrders.Phases.Details.Exists(detail.Id);
        if (!exists)
            return new GenericResponse(false, 
                localizationService.GetLocalizedString("WorkOrderPhaseDetailNotFound"));

        // Update detail
        await unitOfWork.WorkOrders.Phases.Details.Update(detail);
        return new GenericResponse(true, detail);
    }

    public async Task<GenericResponse> RemoveDetail(Guid id)
    {
        var entity = unitOfWork.WorkOrders.Phases.Details.Find(d => d.Id == id).FirstOrDefault();
        if (entity is null)
            return new GenericResponse(false, 
                localizationService.GetLocalizedString("WorkOrderPhaseDetailNotFound"));

        await unitOfWork.WorkOrders.Phases.Details.Remove(entity);
        return new GenericResponse(true, entity);
    }
    
    #endregion

    #region BillOfMaterials CRUD
    
    public async Task<WorkOrderPhaseBillOfMaterials?> GetBillOfMaterialsById(Guid id)
    {
        return await unitOfWork.WorkOrders.Phases.BillOfMaterials.Get(id);
    }

    public async Task<GenericResponse> CreateBillOfMaterials(WorkOrderPhaseBillOfMaterials item)
    {
        // Check if item already exists
        var exists = unitOfWork.WorkOrders.Phases.BillOfMaterials.Find(b => b.Id == item.Id).Any();
        if (exists)
            return new GenericResponse(false, 
                localizationService.GetLocalizedString("WorkOrderPhaseDetailAlreadyExists"));

        // Create item
        await unitOfWork.WorkOrders.Phases.BillOfMaterials.Add(item);
        return new GenericResponse(true, item);
    }

    public async Task<GenericResponse> UpdateBillOfMaterials(WorkOrderPhaseBillOfMaterials item)
    {
        // Check if item exists
        var exists = await unitOfWork.WorkOrders.Phases.BillOfMaterials.Exists(item.Id);
        if (!exists)
            return new GenericResponse(false, 
                localizationService.GetLocalizedString("WorkOrderPhaseDetailNotFound"));

        // Update item
        await unitOfWork.WorkOrders.Phases.BillOfMaterials.Update(item);
        return new GenericResponse(true, item);
    }

    public async Task<GenericResponse> RemoveBillOfMaterials(Guid id)
    {
        var entity = unitOfWork.WorkOrders.Phases.BillOfMaterials.Find(b => b.Id == id).FirstOrDefault();
        if (entity is null)
            return new GenericResponse(false, 
                localizationService.GetLocalizedString("WorkOrderPhaseDetailNotFound"));

        await unitOfWork.WorkOrders.Phases.BillOfMaterials.Remove(entity);
        return new GenericResponse(true, entity);
    }
    
    #endregion

    #region Quantity Validation

    public async Task<GenericResponse> ValidatePreviousPhaseQuantity(ValidatePreviousPhaseQuantityRequest request)
    {
        if (request.Quantity == 0)
        {
            return new GenericResponse(true);
        }

        // 1. Obtener la fase actual
        var currentPhase = await unitOfWork.WorkOrders.Phases.Get(request.WorkOrderPhaseId);
        if (currentPhase == null)
        {
            logger.LogWarning("ValidatePreviousPhaseQuantity: Phase not found {PhaseId}", request.WorkOrderPhaseId);
            return new GenericResponse(false, localizationService.GetLocalizedString("WorkOrderPhaseNotFound"));
        }

        // 2. Obtener la orden de fabricación con todas las fases
        var workOrder = await unitOfWork.WorkOrders.Get(currentPhase.WorkOrderId);
        if (workOrder == null)
        {
            logger.LogWarning("ValidatePreviousPhaseQuantity: WorkOrder not found {WorkOrderId}", currentPhase.WorkOrderId);
            return new GenericResponse(false, localizationService.GetLocalizedString("WorkOrderNotFound", currentPhase.WorkOrderId));
        }

        // 3. Buscar la fase anterior por código
        var previousPhase = workOrder.Phases
            .Where(p => !p.Disabled && !p.IsExternalWork && p.CodeAsNumber < currentPhase.CodeAsNumber)
            .OrderByDescending(p => p.CodeAsNumber)
            .FirstOrDefault();

        decimal availableQuantity;

        if (previousPhase == null)
        {
            // 4. Es la primera fase: permitir hasta 200% de la cantidad planificada
            availableQuantity = workOrder.PlannedQuantity * 2;
        }
        else
        {
            // 5. Leer las QuantityOk directamente de la fase anterior
            availableQuantity = previousPhase.QuantityOk;
        }

        // 6. Validar que la cantidad solicitada no supere las unidades disponibles
        // Incluir las unidades ya fabricadas en la fase actual
        var totalRequested = currentPhase.QuantityOk + request.Quantity;
        if (totalRequested > availableQuantity)
        {
            logger.LogWarning("ValidatePreviousPhaseQuantity: Insufficient quantity. Requested {Requested}, Available {Available}", totalRequested, availableQuantity);
            return new GenericResponse(false, localizationService.GetLocalizedString(
                "WorkOrderPhaseInsufficientQuantity",
                totalRequested,
                availableQuantity));
        }

        return new GenericResponse(true, new
        {
            previousPhaseId = previousPhase?.Id,
            previousPhaseCode = previousPhase?.Code,
            availableQuantity,
            currentPhaseQuantityOk = currentPhase.QuantityOk,
            isFirstPhase = previousPhase == null
        });
    }

    #endregion

    #region Time Metrics

    public async Task<GenericResponse> GetPhaseTimeMetrics(Guid phaseId, Guid machineStatusId, Guid? operatorId)
    {
        // 1. Get phase with details
        var phase = await unitOfWork.WorkOrders.Phases.Get(phaseId);
        if (phase == null)
        {
            return new GenericResponse(false, localizationService.GetLocalizedString("WorkOrderPhaseNotFound"));
        }

        // 2. Get parent work order for planned quantity
        var workOrder = await unitOfWork.WorkOrders.Get(phase.WorkOrderId);
        if (workOrder == null)
        {
            return new GenericResponse(false, localizationService.GetLocalizedString("WorkOrderNotFound", phase.WorkOrderId));
        }

        var plannedQuantity = workOrder.PlannedQuantity;

        // 3. Calculate estimated times from phase details matching the machine status
        decimal estimatedMachineTimeMinutes = 0;
        decimal estimatedOperatorTimeMinutes = 0;

        if (phase.Details != null)
        {
            foreach (var detail in phase.Details.Where(d => !d.Disabled && d.MachineStatusId == machineStatusId))
            {
                var machineTime = detail.EstimatedTime;
                var operatorTime = detail.EstimatedOperatorTime;

                // If it's cycle time, multiply by planned quantity
                if (detail.IsCycleTime)
                {
                    machineTime = machineTime * plannedQuantity;
                    operatorTime = operatorTime * plannedQuantity;
                }

                estimatedMachineTimeMinutes += machineTime;
                estimatedOperatorTimeMinutes += operatorTime;
            }
        }

        // 4. Get actual times from WorkcenterShiftDetail
        var actualMachineTimeMinutes = await unitOfWork.WorkcenterShifts.Details
            .GetTotalMachineTimeByPhaseAndStatus(phaseId, machineStatusId);

        decimal actualOperatorTimeMinutes = 0;
        if (operatorId.HasValue)
        {
            actualOperatorTimeMinutes = await unitOfWork.WorkcenterShifts.Details
                .GetTotalOperatorTimeByPhaseAndOperator(phaseId, operatorId.Value);
        }

        // 5. Build and return metrics DTO
        var metrics = new PhaseTimeMetricsDto
        {
            PhaseId = phaseId,
            MachineStatusId = machineStatusId,
            OperatorId = operatorId,
            EstimatedMachineTimeMinutes = estimatedMachineTimeMinutes,
            EstimatedOperatorTimeMinutes = estimatedOperatorTimeMinutes,
            ActualMachineTimeMinutes = actualMachineTimeMinutes,
            ActualOperatorTimeMinutes = actualOperatorTimeMinutes,
            CalculatedAt = DateTime.UtcNow
        };

        return new GenericResponse(true, metrics);
    }

    #endregion
}
