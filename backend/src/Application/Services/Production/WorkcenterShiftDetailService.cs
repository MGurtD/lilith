using Application.Contracts;
using Domain.Entities.Production;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Services.Production
{
    public class WorkcenterShiftDetailService(IUnitOfWork unitOfWork, IMetricsService metricsService, IWorkOrderPhaseService workOrderPhaseService, IProductionPartChannel productionPartChannel, ILocalizationService localizationService, ILogger<WorkcenterShiftDetailService> logger) : IWorkcenterShiftDetailService
    {
        private GenericResponse LogAndReturnError(string message)
        {
            logger.LogWarning("Validation failed: {Message}", message);
            return new GenericResponse(false, message);
        }

        public async Task<WorkcenterShiftDetail?> GetWorkcenterShiftDetailById(Guid id)
        {
            return await unitOfWork.WorkcenterShifts.Details.Get(id);
        }

        public async Task<List<WorkcenterShiftDetail>> GetCurrentWorkcenterShiftDetailsByWorkcenterShiftId(Guid workcenterShiftDetailId)
        {
            var currentWorkcenterShift = await unitOfWork.WorkcenterShifts.Get(workcenterShiftDetailId);
            if (currentWorkcenterShift == null)
            {
                return [];
            }

            return [.. currentWorkcenterShift.Details.Where(wsd => wsd.Current)];
        }

        #region ClockInOutOperator
        private GenericResponse ValidateOperatorClockInOut(List<WorkcenterShiftDetail> currentWorkcenterDetails, Guid operatorId, OperatorDirection direction)
        {
            if (direction == OperatorDirection.In && currentWorkcenterDetails.Any(wsd => wsd.OperatorId == operatorId))
            {
                return LogAndReturnError("L'operari indicat es troba actualment al centre de treball");

            }
            else if (direction == OperatorDirection.Out && !currentWorkcenterDetails.Any(wsd => wsd.OperatorId == operatorId))
            {
                return LogAndReturnError("L'operari indicat no es troba actualment al centre de treball");
            }
            return new GenericResponse(true);
        }

        public async Task<GenericResponse> OperatorIn(OperatorInOutRequest request)
        {
            var currentWorkcenterShifts = await unitOfWork.WorkcenterShifts
                                                    .FindWithDetails(ws => ws.Current)
                                                    .ToListAsync();

            var currentDetailsWithOperator = currentWorkcenterShifts
                                                    .SelectMany(ws => ws.Details.Where(wsd => wsd.Current && wsd.OperatorId == request.OperatorId))
                                                    .ToList();

            var currentWorkcenterShift = currentWorkcenterShifts.First(ws => ws.WorkcenterId == request.WorkcenterId);
            var currentWorkcenterDetails = currentWorkcenterShift.Details
                                                    .Where(wsd => wsd.Current)
                                                    .ToList();

            var validationResponse = ValidateOperatorClockInOut(currentWorkcenterDetails, request.OperatorId, OperatorDirection.In);
            if (!validationResponse.Result)
            {
                return validationResponse;
            }

            decimal operatorCost = 0;
            var getOperatorCostResponse = await metricsService.GetOperatorCost(request.OperatorId);
            if (getOperatorCostResponse.Result)
            {
                operatorCost = (decimal)getOperatorCostResponse.Content!;
            }

            if (currentWorkcenterDetails.Count == 0)
            {
                // Cas 1 : No hi ha detalls actius al centre de treball
                var defaultMachineStatus = (await unitOfWork.MachineStatuses.FindAsync(ms => ms.Default)).FirstOrDefault();
                if (defaultMachineStatus == null) return LogAndReturnError("L'estat per defecte no existeix");

                var newDetail = new WorkcenterShiftDetail()
                {
                    WorkcenterShiftId = currentWorkcenterShift.Id,
                    MachineStatusId = defaultMachineStatus.Id,
                    OperatorId = request.OperatorId,
                    OperatorCost = operatorCost,
                    ConcurrentOperatorWorkcenters = currentDetailsWithOperator.Count + 1
                };
                newDetail.Open(request.Timestamp);

                await unitOfWork.WorkcenterShifts.Details.AddWithoutSave(newDetail);
            }
            else
            {
                // Cas 2 : Hi ha detalls actius al centre de treball
                foreach (var currentWorkcenterDetail in currentWorkcenterDetails.Where(wsd => wsd.OperatorId == null))
                {
                    currentWorkcenterDetail.Close(request.Timestamp);
                    unitOfWork.WorkcenterShifts.Details.UpdateWithoutSave(currentWorkcenterDetail);

                    var newDetail = new WorkcenterShiftDetail()
                    {
                        WorkcenterShiftId = currentWorkcenterDetail.WorkcenterShiftId,
                        MachineStatusId = currentWorkcenterDetail.MachineStatusId,
                        WorkcenterCost = currentWorkcenterDetail.WorkcenterCost,
                        WorkOrderPhaseId = currentWorkcenterDetail.WorkOrderPhaseId,
                        ConcurrentWorkorderPhases = currentWorkcenterDetail.ConcurrentWorkorderPhases,
                        OperatorId = request.OperatorId,
                        OperatorCost = operatorCost,
                        ConcurrentOperatorWorkcenters = currentDetailsWithOperator.Count + 1
                    };
                    newDetail.Open(request.Timestamp);

                    await unitOfWork.WorkcenterShifts.Details.AddWithoutSave(newDetail);
                }
            }

            // Cas 3 : Hi ha detalls actius a altres centres de treball. Cal tancar i obrir nous detalls amb un operari concurrent més
            foreach (var otherWorkcenterDetail in currentDetailsWithOperator.Where(cdo => cdo.WorkcenterShiftId != currentWorkcenterShift.Id))
            {
                otherWorkcenterDetail.Close(request.Timestamp);
                unitOfWork.WorkcenterShifts.Details.UpdateWithoutSave(otherWorkcenterDetail);

                var newDetail = new WorkcenterShiftDetail()
                {
                    WorkcenterShiftId = otherWorkcenterDetail.WorkcenterShiftId,
                    MachineStatusId = otherWorkcenterDetail.MachineStatusId,
                    MachineStatusReasonId = otherWorkcenterDetail.MachineStatusReasonId,
                    WorkcenterCost = otherWorkcenterDetail.WorkcenterCost,
                    WorkOrderPhaseId = otherWorkcenterDetail.WorkOrderPhaseId,
                    ConcurrentWorkorderPhases = otherWorkcenterDetail.ConcurrentWorkorderPhases,
                    OperatorId = request.OperatorId,
                    OperatorCost = operatorCost,
                    ConcurrentOperatorWorkcenters = currentDetailsWithOperator.Count + 1
                };
                newDetail.Open(request.Timestamp);

                await unitOfWork.WorkcenterShifts.Details.AddWithoutSave(newDetail);
            }

            await unitOfWork.CompleteAsync();
            return new GenericResponse(true);
        }

        public async Task<GenericResponse> OperatorOut(OperatorInOutRequest request)
        {
            var currentWorkcenterShifts = await unitOfWork.WorkcenterShifts
                                                    .FindWithDetails(ws => ws.Current)
                                                    .ToListAsync();

            var currentDetailsWithOperator = currentWorkcenterShifts
                                                    .SelectMany(ws => ws.Details.Where(wsd => wsd.Current && wsd.OperatorId == request.OperatorId))
                                                    .ToList();

            var currentWorkcenterShift = currentWorkcenterShifts.First(ws => ws.WorkcenterId == request.WorkcenterId);
            var currentWorkcenterDetails = currentWorkcenterShift.Details
                                                    .Where(wsd => wsd.Current && wsd.OperatorId == request.OperatorId)
                                                    .ToList();

            var validationResponse = ValidateOperatorClockInOut(currentWorkcenterDetails, request.OperatorId, OperatorDirection.Out);
            if (!validationResponse.Result)
            {
                return validationResponse;
            }

            foreach (var currentWorkcenterDetail in currentDetailsWithOperator)
            {
                currentWorkcenterDetail.Close(request.Timestamp);
                unitOfWork.WorkcenterShifts.Details.UpdateWithoutSave(currentWorkcenterDetail);

                var newDetail = new WorkcenterShiftDetail()
                {
                    WorkcenterShiftId = currentWorkcenterDetail.WorkcenterShiftId,
                    MachineStatusId = currentWorkcenterDetail.MachineStatusId,
                    MachineStatusReasonId = currentWorkcenterDetail.MachineStatusReasonId,
                    WorkOrderPhaseId = currentWorkcenterDetail.WorkOrderPhaseId,
                    WorkcenterCost = currentWorkcenterDetail.WorkcenterCost,
                    ConcurrentWorkorderPhases = currentWorkcenterDetail.ConcurrentWorkorderPhases
                };
                if (currentWorkcenterShift.Id == currentWorkcenterDetail.WorkcenterShiftId)
                {
                    newDetail.OperatorId = null;
                    newDetail.ConcurrentOperatorWorkcenters = 0;
                }
                else
                {
                    newDetail.OperatorId = currentWorkcenterDetail.OperatorId;
                    newDetail.ConcurrentOperatorWorkcenters = currentDetailsWithOperator.Count - 1;
                }
                newDetail.Open(request.Timestamp);

                await unitOfWork.WorkcenterShifts.Details.AddWithoutSave(newDetail);
            }

            await unitOfWork.CompleteAsync();
            return new GenericResponse(true);
        }
        #endregion

        #region WorkOrderPhaseInOut
        private GenericResponse ValidateWorkOrderPhase(List<WorkcenterShiftDetail> currentWorkcenterDetails, WorkOrderPhaseOutRequest request, OperatorDirection direction)
        {
            if (direction == OperatorDirection.In)
            {
                if (currentWorkcenterDetails.Any(wsd => wsd.WorkOrderPhaseId == request.WorkOrderPhaseId))
                {
                    return LogAndReturnError("La fase de fabricació indicada es troba actualment al centre de treball");
                }
            }
            else if (direction == OperatorDirection.Out && !currentWorkcenterDetails.Any(wsd => wsd.WorkOrderPhaseId == request.WorkOrderPhaseId))
            {
                return LogAndReturnError("La fase de fabricació indicada no es troba actualment al centre de treball");
            }
            return new GenericResponse(true);
        }

        private GenericResponse ValidateWorkOrderPhaseAndStatus(List<WorkcenterShiftDetail> currentWorkcenterDetails, WorkOrderPhaseAndStatusInRequest request)
        {
            // Validar que la fase de fabricació + estat de màquina no estigui ja carregada
            if (currentWorkcenterDetails.Any(wsd => wsd.WorkOrderPhaseId == request.WorkOrderPhaseId && wsd.MachineStatusId == request.MachineStatusId))
            {
                return LogAndReturnError("La fase de fabricació indicada es troba actualment al centre de treball amb l'estat de màquina indicat");
            }

            return new GenericResponse(true);
        }       
        

        public async Task<GenericResponse> WorkOrderPhaseIn(WorkOrderPhaseInRequest request)
        {
            var currentWorkcenterShift = await unitOfWork.WorkcenterShifts.GetCurrentWorkcenterShiftWithCurrentDetails(request.WorkcenterId);
            if (currentWorkcenterShift == null)
            {
                return LogAndReturnError("El centre de treball no té cap torn actiu");
            }
            var currentWorkcenterShiftDetails = currentWorkcenterShift.Details.ToList();

            // Validar la petició contra l'estat del sistema
            var validationResponse = ValidateWorkOrderPhase(currentWorkcenterShiftDetails, (WorkOrderPhaseOutRequest) request, OperatorDirection.In);
            if (!validationResponse.Result)
            {
                return validationResponse;
            }

            if (currentWorkcenterShiftDetails.Count == 0)
            {
                // No hi ha detalls actius al centre de treball, crear un nou detall amb l'estat per defecte
                var defaultMachineStatus = (await unitOfWork.MachineStatuses.FindAsync(ms => ms.Default)).FirstOrDefault();
                if (defaultMachineStatus == null) return LogAndReturnError("L'estat per defecte no existeix");
                
                var newDetail = new WorkcenterShiftDetail()
                {
                    WorkcenterShiftId = currentWorkcenterShift.Id,
                    MachineStatusId = defaultMachineStatus.Id,
                    WorkOrderPhaseId = request.WorkOrderPhaseId,
                    ConcurrentWorkorderPhases = 1
                };
                newDetail.Open(request.Timestamp);

                await unitOfWork.WorkcenterShifts.Details.AddWithoutSave(newDetail);
            }
            else
            {
                foreach (var currentDetail in currentWorkcenterShiftDetails.Where(wsd => wsd.WorkOrderPhaseId == null))
                {
                    currentDetail.Close(request.Timestamp);
                    unitOfWork.WorkcenterShifts.Details.UpdateWithoutSave(currentDetail);

                    var newDetail = new WorkcenterShiftDetail()
                    {
                        WorkcenterShiftId = currentDetail.WorkcenterShiftId,
                        MachineStatusId = currentDetail.MachineStatusId,
                        MachineStatusReasonId = currentDetail.MachineStatusReasonId,
                        WorkcenterCost = currentDetail.WorkcenterCost,
                        WorkOrderPhaseId = request.WorkOrderPhaseId,
                        ConcurrentWorkorderPhases = currentDetail.ConcurrentWorkorderPhases + 1,
                        OperatorId = currentDetail.OperatorId,
                        OperatorCost = currentDetail.OperatorCost,
                        ConcurrentOperatorWorkcenters = currentDetail.ConcurrentOperatorWorkcenters
                    };
                    newDetail.Open(request.Timestamp);
                    await unitOfWork.WorkcenterShifts.Details.AddWithoutSave(newDetail);
                }
            }

            await unitOfWork.CompleteAsync();
            return await workOrderPhaseService.StartPhase(request.WorkOrderPhaseId);
        }

        public async Task<GenericResponse> WorkOrderPhaseAndStatusIn(WorkOrderPhaseAndStatusInRequest request)
        {
            var currentWorkcenterShift = await unitOfWork.WorkcenterShifts.GetCurrentWorkcenterShiftWithCurrentDetails(request.WorkcenterId);
            if (currentWorkcenterShift == null)
            {
                return LogAndReturnError("El centre de treball no té cap torn actiu");
            }
            var currentWorkcenterShiftDetails = currentWorkcenterShift.Details.ToList();

            // Validar la petició de la fase i l'estat
            var validationResponse = ValidateWorkOrderPhaseAndStatus(currentWorkcenterShiftDetails, request);
            if (!validationResponse.Result)
            {
                return validationResponse;
            }

            // Obtenir el cost del workcenter per l'estat indicat
            decimal workcenterCost = 0;
            var getWorkcenterStatusCostResponse = await metricsService.GetWorkcenterStatusCost(request.WorkcenterId, request.MachineStatusId);
            if (getWorkcenterStatusCostResponse.Result)
            {
                workcenterCost = (decimal)getWorkcenterStatusCostResponse.Content!;
            }

            if (currentWorkcenterShiftDetails.Count == 0)
            {
                // No hi ha detalls actius al centre de treball
                var newDetail = new WorkcenterShiftDetail()
                {
                    WorkcenterShiftId = currentWorkcenterShift.Id,
                    MachineStatusId = request.MachineStatusId,
                    WorkcenterCost = workcenterCost,
                    WorkOrderPhaseId = request.WorkOrderPhaseId,
                    ConcurrentWorkorderPhases = 1
                };
                newDetail.Open(request.Timestamp);

                await unitOfWork.WorkcenterShifts.Details.AddWithoutSave(newDetail);
            }
            else
            {
                // Hi ha detalls actius al centre de treball
                foreach (var currentDetail in currentWorkcenterShiftDetails)
                {
                    currentDetail.Close(request.Timestamp);
                    unitOfWork.WorkcenterShifts.Details.UpdateWithoutSave(currentDetail);

                    var newDetail = new WorkcenterShiftDetail()
                    {
                        WorkcenterShiftId = currentDetail.WorkcenterShiftId,
                        MachineStatusId = request.MachineStatusId,
                        WorkcenterCost = workcenterCost,
                        WorkOrderPhaseId = request.WorkOrderPhaseId,
                        ConcurrentWorkorderPhases = currentDetail.ConcurrentWorkorderPhases + 1,
                        OperatorId = currentDetail.OperatorId,
                        OperatorCost = currentDetail.OperatorCost,
                        ConcurrentOperatorWorkcenters = currentDetail.ConcurrentOperatorWorkcenters
                    };
                    newDetail.Open(request.Timestamp);
                    await unitOfWork.WorkcenterShifts.Details.AddWithoutSave(newDetail);
                }
            }

            await unitOfWork.CompleteAsync();
            return await workOrderPhaseService.StartPhase(request.WorkOrderPhaseId);
        }

        public async Task<GenericResponse> WorkOrderPhaseOut(WorkOrderPhaseOutRequest request)
        {
            var currentWorkcenterShift = await unitOfWork.WorkcenterShifts.GetCurrentWorkcenterShiftWithCurrentDetails(request.WorkcenterId);
            if (currentWorkcenterShift == null)
            {
                return LogAndReturnError("El centre de treball no té cap torn actiu");
            }
            var currentWorkcenterShiftDetails = currentWorkcenterShift.Details.ToList();

            // Validar petició
            var validationResponse = ValidateWorkOrderPhase(currentWorkcenterShiftDetails, request, OperatorDirection.Out);
            if (!validationResponse.Result)
            {
                return validationResponse;
            }

            // Tancar la fase de fabricació al centre de treball
            foreach (var currentDetail in currentWorkcenterShiftDetails.Where(wsd => wsd.WorkOrderPhaseId == request.WorkOrderPhaseId))
            {
                currentDetail.Close(request.Timestamp);
                unitOfWork.WorkcenterShifts.Details.UpdateWithoutSave(currentDetail);
                
                // Determinar el nou estat de màquina
                var machineStatus = currentDetail.MachineStatusId;
                var machineStatusReasonId = currentDetail.MachineStatusReasonId;
                var workcenterCost = currentDetail.WorkcenterCost;
                if (request.NextMachineStatusId.HasValue && machineStatus != request.NextMachineStatusId)
                {
                    machineStatus = request.NextMachineStatusId.Value;
                    machineStatusReasonId = null;

                    // Obtenir el cost del workcenter per l'estat indicat
                    var workcenterCostResponse = await metricsService.GetWorkcenterStatusCost(request.WorkcenterId, machineStatus);
                    if (workcenterCostResponse.Result)
                    {
                        workcenterCost = (decimal)workcenterCostResponse.Content!;
                    }
                }

                var newDetail = new WorkcenterShiftDetail()
                {
                    WorkcenterShiftId = currentDetail.WorkcenterShiftId,
                    MachineStatusId = machineStatus,
                    MachineStatusReasonId = machineStatusReasonId,
                    WorkcenterCost = workcenterCost,
                    WorkOrderPhaseId = request.NextWorkOrderPhaseId,
                    ConcurrentWorkorderPhases = currentDetail.ConcurrentWorkorderPhases - 1,
                    OperatorId = currentDetail.OperatorId,
                    OperatorCost = currentDetail.OperatorCost,
                    ConcurrentOperatorWorkcenters = currentDetail.ConcurrentOperatorWorkcenters
                };
                newDetail.Open(request.Timestamp);
                await unitOfWork.WorkcenterShifts.Details.AddWithoutSave(newDetail);
            }

            await unitOfWork.CompleteAsync();

            // Encuar generacio de tiquets de produccio en segon pla
            await productionPartChannel.EnqueueAsync(new GenerateProductionPartsRequest
            {
                WorkOrderPhaseId = request.WorkOrderPhaseId,
                WorkcenterId = request.WorkcenterId,
                ClosedAt = request.Timestamp
            });

            // Si hi ha una següent fase, obrir-la
            if (request.NextWorkOrderPhaseId.HasValue)
            {
                var startPhaseResponse = await workOrderPhaseService.StartPhase(request.NextWorkOrderPhaseId.Value);
                if (!startPhaseResponse.Result)
                {
                    return startPhaseResponse;
                }
            }

            return await workOrderPhaseService.EndPhase(request.WorkOrderPhaseId, request.WorkOrderStatusId);
        }
        #endregion

        #region ChangeWorkcenterStatus

        public async Task<GenericResponse> ChangeWorkcenterStatus(WorkcenterChangeStatusRequest request)
        {
            // Obtenir el torn actual del centre de treball
            var currentWorkcenterShift = await unitOfWork.WorkcenterShifts
                                                .FindWithDetails(ws => ws.Current && ws.WorkcenterId == request.WorkcenterId)
                                                .FirstOrDefaultAsync();
            if (currentWorkcenterShift == null)
            {
                return LogAndReturnError("El centre de treball no té cap torn actiu");
            }

            // Obtenir els detalls actius del centre de treball
            var currentDetails = currentWorkcenterShift.Details
                                                .Where(wsd => wsd.Current)
                                                .ToList();

            // Validar la petició
            var validationResponse = await ValidateWorkcenterStatus(currentDetails, request.MachineStatusId);
            if (!validationResponse.Result)
            {
                return validationResponse;
            }

            decimal workcenterCost = 0;
            var getWorkcenterStatusCostResponse = await metricsService.GetWorkcenterStatusCost(request.WorkcenterId, request.MachineStatusId);
            if (getWorkcenterStatusCostResponse.Result)
            {
                workcenterCost = (decimal)getWorkcenterStatusCostResponse.Content!;
            }

            if (currentDetails.Count == 0)
            {
                var newDetail = new WorkcenterShiftDetail()
                {
                    WorkcenterShiftId = currentWorkcenterShift.Id,
                    MachineStatusId = request.MachineStatusId,
                    MachineStatusReasonId = request.MachineStatusReasonId,
                    WorkcenterCost = workcenterCost
                };
                newDetail.Open(request.Timestamp);

                await unitOfWork.WorkcenterShifts.Details.AddWithoutSave(newDetail);
            }
            else
            {
                foreach (var detail in currentDetails)
                {
                    detail.Close(request.Timestamp);
                    unitOfWork.WorkcenterShifts.Details.UpdateWithoutSave(detail);

                    var newDetail = new WorkcenterShiftDetail()
                    {
                        WorkcenterShiftId = detail.WorkcenterShiftId,
                        MachineStatusId = request.MachineStatusId,
                        MachineStatusReasonId = request.MachineStatusReasonId,
                        WorkcenterCost = workcenterCost,
                        WorkOrderPhaseId = detail.WorkOrderPhaseId,
                        ConcurrentWorkorderPhases = detail.ConcurrentWorkorderPhases,
                        OperatorId = detail.OperatorId,
                        OperatorCost = detail.OperatorCost,
                        ConcurrentOperatorWorkcenters = detail.ConcurrentOperatorWorkcenters
                    };
                    newDetail.Open(request.Timestamp);

                    await unitOfWork.WorkcenterShifts.Details.AddWithoutSave(newDetail);
                }
            }

            await unitOfWork.CompleteAsync();
            return new GenericResponse(true);
        }

        private async Task<GenericResponse> ValidateWorkcenterStatus(List<WorkcenterShiftDetail> currentDetails, Guid machineStatusId)
        {
            var machineStatus = await unitOfWork.MachineStatuses.Get(machineStatusId);
            if (machineStatus == null)
            {
                return LogAndReturnError("L'estat indicat no existeix");
            }
            if (currentDetails.Any(wsd => wsd.MachineStatusId == machineStatusId))
            {
                return LogAndReturnError("El centre de treball ja té l'estat indicat");
            }
            return new GenericResponse(true);
        }
        #endregion

        public async Task<GenericResponse> UpdateWorkcenterShiftDetailQuantities(UpdateWorkcenterShiftDetailQuantitiesDto dto)
        {
            // Obtener el último detalle de la fase de la orden de trabajo
            var recentDetail = await unitOfWork.WorkcenterShifts
                                                .FindWithDetails(ws => ws.Current && ws.WorkcenterId == dto.WorkcenterId)
                                                .SelectMany(ws => ws.Details
                                                    .Where(wsd => wsd.Current && wsd.WorkOrderPhaseId == dto.WorkOrderPhaseId))
                                                .OrderByDescending(wsd => wsd.StartTime)
                                                .FirstOrDefaultAsync();
            if (recentDetail != null)
            {
                // Actualizar WorkcenterShiftDetail
                recentDetail.QuantityOk += dto.QuantityOk;
                recentDetail.QuantityKo += dto.QuantityKo;
                await unitOfWork.WorkcenterShifts.Details.Update(recentDetail);

                // Actualizar WorkOrderPhase con las unidades fabricadas
                var workOrderPhase = await unitOfWork.WorkOrders.Phases.Get(dto.WorkOrderPhaseId);
                if (workOrderPhase != null)
                {
                    workOrderPhase.QuantityOk += dto.QuantityOk;
                    workOrderPhase.QuantityKo += dto.QuantityKo;
                    await unitOfWork.WorkOrders.Phases.Update(workOrderPhase);
                }

                return new GenericResponse(true);
            }
            else
            {
                return LogAndReturnError(localizationService.GetLocalizedString("WorkOrderPhaseShiftDetailNotLoaded"));
            }
        }

        public async Task<GenericResponse> DisableWorkcenterShiftDetail(Guid workcenterShiftDetailId)
        {
            var workcenterShiftDetail = await unitOfWork.WorkcenterShifts.Details.Get(workcenterShiftDetailId);
            if (workcenterShiftDetail == null)
            {
                return LogAndReturnError("Workcenter shift detail not found");
            }

            workcenterShiftDetail.Disabled = true;
            await unitOfWork.WorkcenterShifts.Details.Update(workcenterShiftDetail);
            return new GenericResponse(true);
        }
        
    }
}






