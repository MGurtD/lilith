using Application.Contracts;
using System.Globalization;

namespace Application.Services.Production
{
    public class WorkOrderReportService(IUnitOfWork unitOfWork) : IWorkOrderReportService
    {
        public async Task<Application.Contracts.WorkOrderReportResponse?> GetReportById(Guid id)
        {
            var workOrder = await unitOfWork.WorkOrders.GetDetailed(id);
            if (workOrder == null) return null;

            var site = (await unitOfWork.Sites.FindAsync(s => !s.Disabled)).FirstOrDefault();
            if (site == null) return null;

            var enterprise = await unitOfWork.Enterprises.Get(site.EnterpriseId);
            if (enterprise == null) return null;

            var status = await unitOfWork.Lifecycles.StatusRepository.Get(workOrder.StatusId);
            var machineStatuses = await unitOfWork.MachineStatuses.FindAsync(s => !s.Disabled);
            var operatorTypes = await unitOfWork.OperatorTypes.FindAsync(ot => !ot.Disabled);
            var workcenterTypes = await unitOfWork.WorkcenterTypes.FindAsync(wt => !wt.Disabled);
            var workcenters = await unitOfWork.Workcenters.FindAsync(wc => !wc.Disabled);
            var activePhases = workOrder.Phases
                .Where(p => !p.Disabled)
                .OrderBy(p => p.CodeAsNumber)
                .ToList();
            var materialReferenceIds = activePhases
                .SelectMany(p => p.BillOfMaterials.Where(b => !b.Disabled))
                .Select(b => b.ReferenceId)
                .Distinct()
                .ToList();
            var materialReferences = await unitOfWork.References.FindAsync(r => materialReferenceIds.Contains(r.Id));
            var materialReferencesById = materialReferences.ToDictionary(r => r.Id);

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
            foreach (var phase in activePhases)
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
                    if (materialReferencesById.TryGetValue(bom.ReferenceId, out var bomReference))
                    {
                        bomDtos.Add(new WorkOrderPhaseBillOfMaterialsReportDto()
                        {
                            PhaseCode = phase.Code,
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

            return new WorkOrderReportResponse(CultureInfo.CurrentUICulture.TwoLetterISOLanguageName)
            {
                Site = site,
                Enterprise = enterprise,
                Order = orderDto,
                Phases = phaseDtos,
                BillOfMaterials = bomDtos
            };
        }
    }
}









