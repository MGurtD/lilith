using Application.Contracts;
using Domain.Entities.Production;
using Domain.Entities.Warehouse;
using Domain.Implementations.ReferenceFormat;

namespace Application.Services.Production
{
    public class MetricsService(IUnitOfWork unitOfWork, ILocalizationService localizationService) : IMetricsService
    {
        public async Task<GenericResponse> GetOperatorCost(Guid operatorId)
        {
            var oper = await unitOfWork.Operators.Get(operatorId);
            if (oper == null) {
                return new GenericResponse(false, localizationService.GetLocalizedString("OperatorNotFound")); 
            }

            var operatorType = await unitOfWork.OperatorTypes.Get(oper.OperatorTypeId);
            if (operatorType == null) 
                return new GenericResponse(false, localizationService.GetLocalizedString("OperatorTypeNotFound"));

            return new GenericResponse(true, operatorType.Cost);
        }

        public Task<GenericResponse> GetWorkcenterStatusCost(Guid workcenterId, Guid statusId)
        {
            var workcenterCost = unitOfWork.WorkcenterCosts.Find(wc => wc.WorkcenterId == workcenterId && wc.MachineStatusId == statusId).FirstOrDefault();
            if (workcenterCost == null) 
                return Task.FromResult(new GenericResponse(false, localizationService.GetLocalizedString("WorkcenterCostNotFound")));

            return Task.FromResult(new GenericResponse(true, workcenterCost.Cost));
        }

        public async Task<GenericResponse> GetWorkmasterMetrics(WorkMaster workMaster, decimal? producedQuantity)
        {
            // Recollir la quantitat a calcular, si no es passa es calcula per la quantitat base
            var baseQuantity = producedQuantity ?? workMaster.BaseQuantity;
            var operatorCost = 0.0M;
            var machineCost = 0.0M;
            var materialCost = 0.0M;
            var externalServiceCost = 0.0M;
            var externalServiceTransportCost = 0.0M;
            var totalWeight = 0.0M;
            var Amount = 0.0M;
            var materialFactor = producedQuantity.HasValue ? producedQuantity.Value / workMaster.BaseQuantity : 1;
            
            //Recorrer les fases
            //A cada fase, recollir el operatortypeId, i buscar el seu preu cost/hora
            //Per cada fase recorrer els detalls i obtenim el temps+estat maquina, es busca el cost del binomi
            //Si es temps de cicle es multiplica per la quantitat base, sinó es el temps del bloc. Es multiplica el temps pel cost
            //temps en minuts cost en hores
            foreach (var phase in workMaster.Phases)
            {
                var operatorType = unitOfWork.OperatorTypes.Find(o => o.Id == phase.OperatorTypeId).FirstOrDefault();
                var operatorTypeCost = operatorType != null ? operatorType.Cost : 0;

                var phaseDetails = await unitOfWork.WorkMasters.Phases.Get(phase.Id);
                if (phaseDetails == null) continue;

                // Cálcul de fases externes
                if (phase.IsExternalWork)
                {
                    externalServiceCost += phase.ExternalWorkCost;
                    externalServiceTransportCost += phase.TransportCost;
                    continue;
                }

                // Temps
                foreach (var detail in phaseDetails.Details)
                {
                    var estimatedWorkcenterTime = detail.EstimatedTime;
                    var estimatedOperatorTime = detail.EstimatedOperatorTime;
                    if (detail.IsCycleTime)
                    {
                        estimatedWorkcenterTime = baseQuantity * detail.EstimatedTime;
                        estimatedOperatorTime = baseQuantity * detail.EstimatedOperatorTime;
                    }

                    // Cost Operari
                    operatorCost += estimatedOperatorTime / 60 * operatorTypeCost;

                    // Cost Màquina                    
                    var workCenterCost = unitOfWork.WorkcenterCosts.Find(wc => wc.WorkcenterId == phase.PreferredWorkcenterId && wc.MachineStatusId == detail.MachineStatusId).FirstOrDefault();
                    if (workCenterCost == null) {
                        return new GenericResponse(false, localizationService.GetLocalizedString("WorkcenterCombinationNotFound"));
                    }
                    machineCost += estimatedWorkcenterTime / 60 * workCenterCost.Cost;

                }

                // Material
                foreach (var bom in phaseDetails.BillOfMaterials)
                {

                    var reference = await unitOfWork.References.Get(bom.ReferenceId);
                    if (reference == null) continue;
                    if (!reference.ReferenceTypeId.HasValue)
                    {
                        return new GenericResponse(false, localizationService.GetLocalizedString("ReferenceTypeNotFound"));
                    }

                    if (reference.ReferenceFormatId.HasValue)
                    {
                        var referenceType = await unitOfWork.ReferenceTypes.Get(reference.ReferenceTypeId.Value);

                        // Obtenir calculadora segons el format
                        var format = await unitOfWork.ReferenceFormats.Get(reference.ReferenceFormatId.Value);
                        if (format == null) {
                            return new GenericResponse(false, localizationService.GetLocalizedString("ReferenceFormatNotFound"));
                        }

                        // Assignar dimensions a la calculadora
                        var dimensionsCalculator = ReferenceFormatCalculationFactory.Create(format.Code);
                        var dimensions = new ReferenceDimensions
                        {
                            Quantity = (int)bom.Quantity,
                            Width = bom.Width,
                            Length = bom.Length,
                            Height = bom.Height,
                            Diameter = bom.Diameter,
                            Thickness = bom.Thickness,
                            Density = referenceType!.Density
                        };

                        try
                        {
                            // Calcular el pes
                            if (format.Code == "UNITATS")
                            {
                                Amount = reference.LastCost * bom.Quantity * materialFactor;
                            }
                            else
                            {
                                var UnitWeight = Math.Round(dimensionsCalculator.Calculate(dimensions), 2);
                                totalWeight = totalWeight + (UnitWeight * bom.Quantity * materialFactor);

                                // Calcular el preu

                                Amount = reference.LastCost * UnitWeight * bom.Quantity * materialFactor;

                            }

                            // Acumular el cost del material
                            materialCost += Amount;
                        }
                        catch (Exception e)
                        {
                            return new GenericResponse(false, e.Message);
                        }
                    }

                }
            }

            return new GenericResponse(true, new ProductionMetrics(operatorCost, machineCost, materialCost, externalServiceCost, externalServiceTransportCost, totalWeight));
        }

        public async Task<GenericResponse> GetProductionPartCosts(ProductionPart productionPart)
        {
            var productionMetrics = new ProductionMetrics(0, 0, 0, 0, 0, 0);

            // Get operator cost
            if (productionPart.OperatorId.HasValue)
            {
                var operatorCostRequest = await GetOperatorCost(productionPart.OperatorId.Value);
                if (operatorCostRequest.Result)
                {
                    productionMetrics.OperatorCost = (decimal)operatorCostRequest.Content!;
                }
            }

            // Get workcenter cost
            var workOrderPhaseDetail = await unitOfWork.WorkOrders.Phases.Details.Get(productionPart.WorkOrderPhaseDetailId);
            if (workOrderPhaseDetail != null && workOrderPhaseDetail.MachineStatusId.HasValue)
            {
                var workcenterCostRequest = await GetWorkcenterStatusCost(productionPart.WorkcenterId, workOrderPhaseDetail.MachineStatusId.Value);
                if (!workcenterCostRequest.Result) return workcenterCostRequest;
                productionMetrics.MachineCost = (decimal)workcenterCostRequest.Content!;
            }

            return new GenericResponse(true, productionMetrics);
        }

        /// <summary>
        /// Calcula el cost real de material consumit per una OF a partir dels moviments
        /// de magatzem de tipus CONSUMPTION vinculats a les seves fases. Utilitza el mateix
        /// càlcul de cost que GetWorkmasterMetrics (pes per format de referència × cost).
        /// Quantitat negativa = consum (suma cost), positiva = retorn (resta cost).
        /// </summary>
        public async Task<decimal> GetWorkOrderConsumedMaterialCost(Guid workOrderId)
        {
            var phaseIds = unitOfWork.WorkOrders.Phases
                .Find(p => p.WorkOrderId == workOrderId)
                .Select(p => p.Id)
                .ToList();

            if (phaseIds.Count == 0) return 0;

            var consumptionMovements = unitOfWork.StockMovements
                .Find(m => m.Entity == StockMovementEntities.WorkOrderPhase
                         && m.EntityId.HasValue
                         && phaseIds.Contains(m.EntityId.Value)
                         && m.MovementType == StockMovementType.CONSUMPTION)
                .ToList();

            if (consumptionMovements.Count == 0) return 0;

            var referenceCache = new Dictionary<Guid, (decimal LastCost, string FormatCode, decimal Density)>();
            decimal totalMaterialCost = 0;

            foreach (var movement in consumptionMovements)
            {
                if (!referenceCache.TryGetValue(movement.ReferenceId, out var refData))
                {
                    var reference = await unitOfWork.References.Get(movement.ReferenceId);
                    if (reference == null) continue;

                    var formatCode = ReferenceFormatCodes.UNITATS;
                    decimal density = 0;

                    if (reference.ReferenceFormatId.HasValue)
                    {
                        var format = await unitOfWork.ReferenceFormats.Get(reference.ReferenceFormatId.Value);
                        if (format != null) formatCode = format.Code;
                    }

                    if (reference.ReferenceTypeId.HasValue)
                    {
                        var referenceType = await unitOfWork.ReferenceTypes.Get(reference.ReferenceTypeId.Value);
                        if (referenceType != null) density = referenceType.Density;
                    }

                    refData = (reference.LastCost, formatCode, density);
                    referenceCache[movement.ReferenceId] = refData;
                }

                decimal movementCost;
                if (refData.FormatCode == ReferenceFormatCodes.UNITATS)
                {
                    movementCost = Math.Abs(movement.Quantity) * refData.LastCost;
                }
                else
                {
                    var calculator = ReferenceFormatCalculationFactory.Create(refData.FormatCode);
                    var dimensions = new ReferenceDimensions
                    {
                        Quantity = Math.Abs(movement.Quantity),
                        Width = movement.Width,
                        Length = movement.Length,
                        Height = movement.Height,
                        Diameter = movement.Diameter,
                        Thickness = movement.Thickness,
                        Density = refData.Density
                    };

                    try
                    {
                        var unitWeight = Math.Round(calculator.Calculate(dimensions), 2);
                        movementCost = Math.Abs(movement.Quantity) * unitWeight * refData.LastCost;
                    }
                    catch
                    {
                        continue;
                    }
                }

                if (movement.Quantity < 0)
                    totalMaterialCost += movementCost;
                else
                    totalMaterialCost -= movementCost;
            }

            return Math.Max(0, Math.Round(totalMaterialCost, 2));
        }
    }
}
