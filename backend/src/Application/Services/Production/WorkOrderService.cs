

using Application.Contracts;
using Domain.Entities.Production;
using Domain.Entities.Sales;

namespace Application.Services.Production
{
    public class WorkOrderService(IUnitOfWork unitOfWork, IExerciseService exerciseService, ISalesOrderService salesOrderService, ILocalizationService localizationService) : IWorkOrderService
    {
        public async Task<WorkOrder?> GetById(Guid id)
        {
            return await unitOfWork.WorkOrders.Get(id);
        }

        public async Task<GenericResponse> Create(WorkOrder workOrder)
        {
            var existsReference = await unitOfWork.References.Exists(workOrder.ReferenceId);
            if (!existsReference)
            {
                return new GenericResponse(false,
                    localizationService.GetLocalizedString("ReferenceNotFound"));
            }

            var exists = unitOfWork.WorkOrders.Find(w => w.Id == workOrder.Id).Any();
            if (exists)
            {
                return new GenericResponse(false,
                    localizationService.GetLocalizedString("WorkOrderAlreadyExists"));
            }

            await unitOfWork.WorkOrders.Add(workOrder);
            return new GenericResponse(true, workOrder);
        }

        public IEnumerable<DetailedWorkOrder> GetWorkOrderDetails(Guid id) {
            var details = unitOfWork.DetailedWorkOrders.Find(d => d.WorkOrderId == id);

            var groupedDetails =
            from d in details
            group d by new
            {
                d.WorkOrderId,
                d.WorkOrderCode,
                d.WorkOrderStartTime,
                d.WorkOrderEndTime,
                d.WorkOrderStatusCode,
                d.WorkOrderStatusDescription,
                d.WorkOrderOrder,
                d.WorkOrderComment,
                d.PlannedDate,
                d.PlannedQuantity,
                d.ReferenceCode,
                d.ReferenceDescription,
                d.ReferenceVersion,
                d.ReferenceCost,
                d.WorkOrderPhaseId,
                d.WorkOrderPhaseCode,
                d.WorkOrderPhaseDescription,
                d.WorkOrderPhaseComment,
                d.WorkOrderPhaseStartTime,
                d.WorkOrderPhaseEndTime,
                d.WorkOrderPhaseStatusCode,
                d.WorkOrderPhaseStatusDescription,
                d.WorkOrderPhaseDetailId,
                d.WorkOrderPhaseDetailOrder,
                d.WorkOrderPhaseDetailEstimatedTime,
                d.WorkOrderPhaseDetailComment,
                d.MachineStatusName,
                d.MachineStatusDescription,
            } into gcs
            select new DetailedWorkOrder()
            {
                WorkOrderId = gcs.Key.WorkOrderId,
                WorkOrderCode = gcs.Key.WorkOrderCode,
                WorkOrderStartTime = gcs.Key.WorkOrderStartTime,
                WorkOrderEndTime = gcs.Key.WorkOrderEndTime,
                WorkOrderStatusCode = gcs.Key.WorkOrderStatusCode,
                WorkOrderStatusDescription = gcs.Key.WorkOrderStatusDescription,
                WorkOrderOrder = gcs.Key.WorkOrderOrder,
                WorkOrderComment = gcs.Key.WorkOrderComment,
                PlannedDate = gcs.Key.PlannedDate,
                PlannedQuantity = gcs.Key.PlannedQuantity,
                ReferenceCode = gcs.Key.ReferenceCode,
                ReferenceDescription = gcs.Key.ReferenceDescription,
                ReferenceVersion = gcs.Key.ReferenceVersion,
                ReferenceCost = gcs.Key.ReferenceCost,
                WorkOrderPhaseId = gcs.Key.WorkOrderPhaseId,
                WorkOrderPhaseCode = gcs.Key.WorkOrderPhaseCode,
                WorkOrderPhaseDescription = gcs.Key.WorkOrderPhaseDescription,
                WorkOrderPhaseComment = gcs.Key.WorkOrderPhaseComment,
                WorkOrderPhaseStartTime = gcs.Key.WorkOrderPhaseStartTime,
                WorkOrderPhaseEndTime = gcs.Key.WorkOrderPhaseEndTime,
                WorkOrderPhaseStatusCode = gcs.Key.WorkOrderPhaseStatusCode,
                WorkOrderPhaseStatusDescription = gcs.Key.WorkOrderPhaseStatusDescription,
                WorkOrderPhaseDetailId = gcs.Key.WorkOrderPhaseDetailId,
                WorkOrderPhaseDetailOrder = gcs.Key.WorkOrderPhaseDetailOrder,
                WorkOrderPhaseDetailEstimatedTime = gcs.Key.WorkOrderPhaseDetailEstimatedTime,
                WorkOrderPhaseDetailComment = gcs.Key.WorkOrderPhaseDetailComment,
                MachineStatusName = gcs.Key.MachineStatusName,
                MachineStatusDescription = gcs.Key.MachineStatusDescription,
                PreferredWorkcenter = false,
                WorkcenterId = Guid.Empty,
                WorkcenterName = "",
                WorkcenterCost = 0,
                WorkcenterDescription = ""
            };

            return groupedDetails;
        }

        public async Task<GenericResponse> CreateFromWorkMaster(CreateWorkOrderDto dto)
        {
            // Ruta de fabricació
            var workMaster = await unitOfWork.WorkMasters.GetFullById(dto.WorkMasterId);
            if (workMaster is null) 
                return new GenericResponse(false, localizationService.GetLocalizedString("WorkMasterNotFound"));

            // Exercici
            var exercise = exerciseService.GetExerciceByDate(dto.PlannedDate);
            if (exercise is null) 
                return new GenericResponse(false, localizationService.GetLocalizedString("ExerciseNotFoundForDate"));
            
            var exerciseServiceResponse = await exerciseService.GetNextCounter(exercise.Id, "workorder");
            if (!exerciseServiceResponse.Result || exerciseServiceResponse.Content is null) 
                return new GenericResponse(false, localizationService.GetLocalizedString("ExerciseCounterError"));
            
            var code = Convert.ToString(exerciseServiceResponse.Content);

            // Estat
            var initialStatusId = await GetInitialStatus();
            if (initialStatusId is null) 
                return new GenericResponse(false, localizationService.GetLocalizedString("WorkMasterNoInitialStatus"));

            // Crear ordre de fabricació
            var workOrder = new WorkOrder()
            { 
                WorkMasterId = dto.WorkMasterId,
                Code = code!,
                ExerciseId = exercise.Id,
                ReferenceId = workMaster.ReferenceId,
                PlannedDate = dto.PlannedDate,
                PlannedQuantity = dto.PlannedQuantity,
                StatusId = initialStatusId.Value,
                Comment = dto.Comment
            };

            foreach (var workMasterPhase in workMaster.Phases) 
            {
                // Afegir fases
                var workOrderPhase = new WorkOrderPhase()
                {
                    Code = workMasterPhase.Code,
                    Description = workMasterPhase.Description,
                    Comment = workMasterPhase.Comment,
                    ExternalWorkCost = workMasterPhase.ExternalWorkCost,
                    IsExternalWork = workMasterPhase.IsExternalWork,
                    TransportCost = workMasterPhase.TransportCost,
                    ServiceReferenceId = workMasterPhase.ServiceReferenceId,
                    OperatorTypeId = workMasterPhase.OperatorTypeId,
                    PreferredWorkcenterId = workMasterPhase.PreferredWorkcenterId,
                    WorkcenterTypeId = workMasterPhase.WorkcenterTypeId,
                    ProfitPercentage = workMasterPhase.ProfitPercentage,
                    WorkOrderId = workOrder.Id,
                    StatusId = initialStatusId.Value
                };

                // Afegir detalls de la fase
                foreach (var workMasterPhaseDetail in workMasterPhase.Details)
                {
                    var workOrderPhaseDetail = new WorkOrderPhaseDetail()
                    {
                        WorkOrderPhaseId = workOrderPhase.Id,
                        EstimatedTime = workMasterPhaseDetail.EstimatedTime,
                        EstimatedOperatorTime = workMasterPhaseDetail.EstimatedOperatorTime,
                        Comment = workMasterPhaseDetail.Comment,
                        IsCycleTime = workMasterPhaseDetail.IsCycleTime,
                        MachineStatusId = workMasterPhaseDetail.MachineStatusId,
                        Order = workMasterPhaseDetail.Order                        
                    };
                    workOrderPhase.Details.Add(workOrderPhaseDetail);
                }

                // Afegir llista de materials de la fase
                foreach (var workMasterPhaseBomItem in workMasterPhase.BillOfMaterials)
                {
                    var workOrderPhaseBomItem = new WorkOrderPhaseBillOfMaterials()
                    {
                        WorkOrderPhaseId = workOrderPhase.Id,
                        Quantity = (dto.PlannedQuantity/workMaster.BaseQuantity)* workMasterPhaseBomItem.Quantity,
                        Diameter = workMasterPhaseBomItem.Diameter,
                        Height = workMasterPhaseBomItem.Height,
                        Length = workMasterPhaseBomItem.Length,
                        ReferenceId = workMasterPhaseBomItem.ReferenceId,
                        Thickness = workMasterPhaseBomItem.Thickness,
                        Width = workMasterPhaseBomItem.Width,
                    };
                    workOrderPhase.BillOfMaterials.Add(workOrderPhaseBomItem);
                }

                workOrder.Phases.Add(workOrderPhase);
            }

            // Guardar ordre de fabricació a la BDD
            await unitOfWork.WorkOrders.Add(workOrder);

            return new GenericResponse(true, workOrder);
        }

        private async Task<Guid?> GetInitialStatus()
        {
            var status = await unitOfWork.Lifecycles.GetStatusByName(StatusConstants.Lifecycles.WorkOrder, StatusConstants.Statuses.Creada);
            if (status != null) return status.Id;
            return null;
        }

        public async Task<GenericResponse> Start(Guid id)
        {
            var workOrder = await unitOfWork.WorkOrders.Get(id);
            if (workOrder is null) 
                return new GenericResponse(false, localizationService.GetLocalizedString("WorkOrderNotFound", id));

            if (!workOrder.StartTime.HasValue) workOrder.StartTime = DateTime.Now;
            return new GenericResponse(true);
        }

        public async Task<GenericResponse> End(Guid id)
        {
            var workOrder = await unitOfWork.WorkOrders.Get(id);
            if (workOrder is null) 
                return new GenericResponse(false, localizationService.GetLocalizedString("WorkOrderNotFound", id));

            if (!workOrder.EndTime.HasValue) workOrder.EndTime = DateTime.Now;
            return new GenericResponse(true);
        }

        public IEnumerable<WorkOrder> GetBetweenDatesAndStatus(DateTime startDate, DateTime endDate, Guid? statusId)
        {
            var workOrders = unitOfWork.WorkOrders.Find(w => w.PlannedDate >= startDate && w.PlannedDate <= endDate);
            if (statusId.HasValue) workOrders = workOrders.Where(w => w.StatusId == statusId);

            return workOrders;
        }

        public async Task<IEnumerable<WorkOrder>> GetBySalesOrderId(Guid salesOrderId)
        {
            var workOrders = new List<WorkOrder>();

            var salesOrder = await salesOrderService.GetById(salesOrderId);
            if (salesOrder != null)
            {
                var workOrderIds = salesOrder.SalesOrderDetails.Where(d => d.WorkOrderId != null).Select(d => d.WorkOrderId);
                workOrders = unitOfWork.WorkOrders.Find(w => workOrderIds.Contains(w.Id)).ToList();
            }

            return workOrders;
        }

        public async Task<GenericResponse> Delete(Guid id)
        {
            var woOrderDetails = unitOfWork.SalesOrderDetails.Find(d => d.WorkOrderId == id);
                    if (woOrderDetails.Any()) {
                var orderDetail = woOrderDetails.FirstOrDefault();

                if (orderDetail is not null) {
                    orderDetail.WorkOrderId = null;
                    await unitOfWork.SalesOrderDetails.Update(orderDetail);
                }
            }

            var entity = unitOfWork.WorkOrders.Find(e => e.Id == id).FirstOrDefault();
            if (entity is null)
                return new GenericResponse(false, localizationService.GetLocalizedString("WorkOrderNotFound", id));

            await unitOfWork.WorkOrders.Remove(entity);
            return new GenericResponse(true, entity);
        }
        
        private enum OperationType
        {
            Add,
            Remove
        }

        private async Task<GenericResponse> UpdateWorkOrderTotalsFromProductionPart(Guid id, ProductionPart productionPart, OperationType operationType)
        {
            var workOrder = await unitOfWork.WorkOrders.Get(id);            
            if (workOrder is null) 
                return new GenericResponse(false, localizationService.GetLocalizedString("WorkOrderNotFound", id));

            if (operationType == OperationType.Add) {
                workOrder.OperatorTime += productionPart.OperatorTime;
                workOrder.MachineTime += productionPart.WorkcenterTime;
                workOrder.TotalQuantity += productionPart.Quantity;
                workOrder.MachineCost += (productionPart.WorkcenterTime / 60) * productionPart.MachineHourCost;
                workOrder.OperatorCost += (productionPart.OperatorTime/60) * productionPart.OperatorHourCost;
            } else {
                workOrder.OperatorTime -= productionPart.OperatorTime;
                workOrder.MachineTime -= productionPart.WorkcenterTime;
                workOrder.TotalQuantity -= productionPart.Quantity;
                workOrder.MachineCost -= (productionPart.WorkcenterTime / 60) * productionPart.MachineHourCost;
                workOrder.OperatorCost -= (productionPart.OperatorTime/60) * productionPart.OperatorHourCost;
            }

            await unitOfWork.WorkOrders.Update(workOrder);
            return new GenericResponse(true , workOrder);
        }

        public async Task<GenericResponse> AddProductionPart(Guid id, ProductionPart productionPart)
        {
            return await UpdateWorkOrderTotalsFromProductionPart(id, productionPart, OperationType.Add);
        }

        public async Task<GenericResponse> RemoveProductionPart(Guid id, ProductionPart productionPart)
        {
            return await UpdateWorkOrderTotalsFromProductionPart(id, productionPart, OperationType.Remove);
        }

        public async Task Update(WorkOrder workOrder)
        {
            var lastCost = workOrder.MachineCost + workOrder.OperatorCost + workOrder.MaterialCost;
            var reference = await unitOfWork.References.Get(workOrder.ReferenceId);
            if (reference != null)
            {
                reference.LastCost = lastCost;
                await unitOfWork.References.Update(reference);
            }

            var details = unitOfWork.SalesOrderDetails.Find(e => e.WorkOrderId == workOrder.Id).ToList();
            foreach (SalesOrderDetail detail in details)
            {
                detail.LastCost = lastCost;
                await unitOfWork.SalesOrderDetails.Update(detail);
            }
            
            await unitOfWork.WorkOrders.Update(workOrder);
        }

        public async Task<IEnumerable<WorkOrder>> GetPlannableWorkOrders()
        {
            var workOrders = Enumerable.Empty<WorkOrder>();
            
            // Get WorkOrder lifecycle
            var lifecycle = await unitOfWork.Lifecycles.GetByName(StatusConstants.Lifecycles.WorkOrder);
            if (lifecycle == null)
            {
                return workOrders;
            }

            // Get statuses with 'Plannable' tag - these are the ones to exclude
            var plannableStatuses = await unitOfWork.LifecycleTags.GetStatusesByTagName(
                StatusConstants.LifecycleTags.Available, 
                lifecycle.Id);
            
            if (plannableStatuses.Count == 0)
            {
                return await unitOfWork.WorkOrders.GetPlannableWorkOrders(lifecycle.InitialStatusId.HasValue ? [lifecycle.InitialStatusId.Value] : []);
            }

            var includedStatusId = plannableStatuses.Select(s => s.Id).ToArray();
            return await unitOfWork.WorkOrders.GetPlannableWorkOrders(includedStatusId);
        }

        public async Task<GenericResponse> Priorize(List<UpdateWorkOrderOrderDTO> orders)
        {
            // Validate input
            if (orders == null || orders.Count == 0)
                return new GenericResponse(false, 
                    localizationService.GetLocalizedString("NoOrdersToUpdate"));

            var ids = orders.Select(o => o.Id).ToArray();
            
            // Get all work orders by IDs
            var workOrders = unitOfWork.WorkOrders
                .Find(w => ids.Contains(w.Id))
                .ToList();
            
            // Business validation: verify all work orders exist
            if (workOrders.Count != ids.Length)
            {
                var missingIds = ids.Except(workOrders.Select(w => w.Id));
                return new GenericResponse(false,
                    localizationService.GetLocalizedString("WorkOrdersNotFound"));
            }
            
            var lifecycle = await unitOfWork.Lifecycles.GetByName(StatusConstants.Lifecycles.WorkOrder);
            if (lifecycle == null)
            {
                return new GenericResponse(false,
                    localizationService.GetLocalizedString("LifecycleNotFound", StatusConstants.Lifecycles.WorkOrder));
            }

            // Update the order field for each work order
            foreach (var workOrder in workOrders)
            {
                var orderDto = orders.First(o => o.Id == workOrder.Id);
                workOrder.Order = orderDto.Order;

                if (workOrder.StatusId == lifecycle.InitialStatusId)
                {
                    workOrder.StatusId = lifecycle.Statuses!.FirstOrDefault(s => s.Name == StatusConstants.Statuses.Llancada)?.Id ?? workOrder.StatusId;
                }

                await unitOfWork.WorkOrders.Update(workOrder);
            }

            return new GenericResponse(true);
        }
        
	    public async Task<IEnumerable<WorkOrderPhaseEstimationDto>>GetWorkcenterLoadBetweenDatesByWorkcenterType(DateTime startDate, DateTime endDate)
	    {
	        return await unitOfWork.WorkOrders.GetWorkcenterLoadBetweenDatesByWorkcenterType(startDate, endDate);
	    }

        public async Task<GenericResponse> UpdateStatusAfterPhaseEnd(Guid workOrderId, Guid completedPhaseId, Guid phaseOutStatusId)
        {
            // Get WorkOrder with all phases
            var workOrder = await unitOfWork.WorkOrders.Get(workOrderId);
            if (workOrder == null)
                return new GenericResponse(false, 
                    localizationService.GetLocalizedString("WorkOrderNotFound", workOrderId));

            // Get the completed phase to determine its position
            var completedPhase = workOrder.Phases.FirstOrDefault(p => p.Id == completedPhaseId);
            if (completedPhase == null)
                return new GenericResponse(false, 
                    localizationService.GetLocalizedString("WorkOrderPhaseNotFound"));

            // Check if this is the last active non-external phase
            var activePhasesOrdered = workOrder.Phases
                .Where(p => p.Disabled == false && p.IsExternalWork == false)
                .OrderByDescending(p => p.CodeAsNumber)
                .ToList();
            
            bool isLastPhase = activePhasesOrdered.FirstOrDefault()?.Id == completedPhaseId;
            
            // Check if there's a subsequent external work phase
            var currentPhaseCodeAsNumber = completedPhase.CodeAsNumber;
            var hasSubsequentExternalPhase = workOrder.Phases
                .Any(p => !p.Disabled && p.IsExternalWork && p.CodeAsNumber > currentPhaseCodeAsNumber);
            
            // Determine the appropriate status for the work order
            if (hasSubsequentExternalPhase)
            {
                // Set work order status to "Servei Extern"
                var externalServiceStatus = await unitOfWork.Lifecycles.GetStatusByName(
                    StatusConstants.Lifecycles.WorkOrder,
                    StatusConstants.Statuses.ServeiExtern);
                
                if (externalServiceStatus == null)
                {
                    return new GenericResponse(false, 
                        localizationService.GetLocalizedString("StatusNotFound", StatusConstants.Statuses.ServeiExtern));
                }
                
                workOrder.StatusId = externalServiceStatus.Id;
                workOrder.Phases = []; // Clear phases to avoid tracking issues
                await unitOfWork.WorkOrders.Update(workOrder);
            }
            else if (isLastPhase)
            {
                // If last phase and no subsequent external phases, close the work order
                var outStatus = await unitOfWork.Lifecycles.StatusRepository.Get(phaseOutStatusId);
                if (outStatus == null)
                    return new GenericResponse(false, 
                        localizationService.GetLocalizedString("StatusNotFound"));

                workOrder.StatusId = outStatus.Id;
                workOrder.EndTime = DateTime.Now;
                
                workOrder.Phases = []; // Clear phases to avoid tracking issues
                await unitOfWork.WorkOrders.Update(workOrder);
            }
            
            return new GenericResponse(true);
        }

    }
}






