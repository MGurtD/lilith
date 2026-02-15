using Application.Contracts;

namespace Application.Services.Production
{
    public class WorkOrderReportService(IUnitOfWork unitOfWork) : IWorkOrderReportService
    {
        public async Task<Application.Contracts.WorkOrderReportResponse?> GetReportById(Guid id)
        {
            var workOrder = await unitOfWork.WorkOrders.GetDetailed(id);
            if (workOrder == null) return null;

            var status = await unitOfWork.Lifecycles.StatusRepository.Get(workOrder.StatusId);
            var machineStatuses = await unitOfWork.MachineStatuses.FindAsync(s => !s.Disabled);
            var operatorTypes = await unitOfWork.OperatorTypes.FindAsync(ot => !ot.Disabled);
            var workcenterTypes = await unitOfWork.WorkcenterTypes.FindAsync(wt => !wt.Disabled);
            var workcenters = await unitOfWork.Workcenters.FindAsync(wc => !wc.Disabled);

            var orderDto = new WorkOrderReportDto()
            {
                Code = workOrder.Code,
                ReferenceCode = workOrder.Reference!.Code,
                ReferenceDescription = workOrder.Reference!.Description,
                PlannedDate = workOrder.PlannedDate,
                PlannedQuantity = workOrder.PlannedQuantity,
                StatusName = status?.Name ?? string.Empty,
                Comment = workOrder.Comment,
                HasExternalWork = workOrder.Phases.Where(p => !p.Disabled).Any(p => p.IsExternalWork && p.ExternalWorkCost > 0)
            };

            var phaseDtos = new List<WorkOrderPhaseReportDto>();
            var bomDtos = new List<WorkOrderPhaseBillOfMaterialsReportDto>();
            foreach (var phase in workOrder.Phases.Where(p => !p.Disabled).OrderBy(p => p.CodeAsNumber))
            {
                var detailDtos = new List<WorkOrderPhaseDetailReportDto>();
                foreach (var detail in phase.Details.Where(d => !d.Disabled).OrderBy(d => d.Order))
                {
                    detailDtos.Add(new WorkOrderPhaseDetailReportDto()
                    {
                        Description = detail.Comment,
                        EstimatedTime = detail.EstimatedTime,
                        EstimatedOperatorTime = detail.EstimatedOperatorTime,
                        MachineStatusName = machineStatuses.FirstOrDefault(s => s.Id == detail.MachineStatusId)?.Name ?? string.Empty
                    });
                }

                foreach (var bom in phase.BillOfMaterials.Where(b => !b.Disabled))
                {
                    var bomReference = await unitOfWork.References.Get(bom.ReferenceId);
                    if (bomReference != null)
                    {
                        bomDtos.Add(new WorkOrderPhaseBillOfMaterialsReportDto()
                        {
                            ReferenceCode = bomReference.Code,
                            ReferenceDescription = bomReference.Description,
                            Quantity = bom.Quantity,
                            Width = bom.Width,
                            Length = bom.Length,
                            Thickness = bom.Thickness,
                            Diameter = bom.Diameter
                        });
                    }
                }

                var workcenterType = workcenterTypes.FirstOrDefault(wt => wt.Id == phase.WorkcenterTypeId);
                var operatorType = operatorTypes.FirstOrDefault(ot => ot.Id == phase.OperatorTypeId);
                var workcenter = workcenters.FirstOrDefault(wc => wc.Id == phase.PreferredWorkcenterId);

                phaseDtos.Add(new WorkOrderPhaseReportDto()
                {
                    Code = phase.Code,
                    Description = phase.Description,
                    WorkcenterTypeName = workcenterType?.Name ?? string.Empty,
                    WorkcenterName = workcenter?.Name ?? string.Empty,
                    OperatorTypeName = operatorType?.Name ?? string.Empty,
                    IsExternalWork = phase.IsExternalWork,
                    Details = detailDtos
                });
            }

            return new WorkOrderReportResponse()
            {
                Order = orderDto,
                Phases = phaseDtos,
                BillOfMaterials = bomDtos
            };
        }
    }
}









