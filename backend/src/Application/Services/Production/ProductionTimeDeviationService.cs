using Application.Contracts;

namespace Application.Services.Production
{
    public class ProductionTimeDeviationService(IUnitOfWork unitOfWork) : IProductionTimeDeviationService
    {
        public Task<ProductionTimeDeviationResult> GetDeviation(
            DateTime startDate, DateTime endDate, Guid? workOrderId)
        {
            var parts = unitOfWork.ProductionParts
                .Find(p => p.Date >= startDate && p.Date <= endDate &&
                           (workOrderId == null || p.WorkOrderId == workOrderId))
                .ToList();

            if (parts.Count == 0)
                return Task.FromResult(new ProductionTimeDeviationResult());

            var detailIds = parts.Select(p => p.WorkOrderPhaseDetailId).Distinct().ToList();
            var details = unitOfWork.WorkOrders.Phases.Details
                .Find(d => detailIds.Contains(d.Id))
                .ToDictionary(d => d.Id);

            var phaseIds = details.Values.Select(d => d.WorkOrderPhaseId).Distinct().ToList();
            var phases = unitOfWork.WorkOrders.Phases
                .Find(p => phaseIds.Contains(p.Id))
                .ToDictionary(p => p.Id);

            var workOrderIds = phases.Values.Select(p => p.WorkOrderId).Distinct().ToList();
            var workOrders = unitOfWork.WorkOrders
                .Find(w => workOrderIds.Contains(w.Id))
                .ToDictionary(w => w.Id, w => w.Code);

            var statusIds = details.Values
                .Where(d => d.MachineStatusId.HasValue)
                .Select(d => d.MachineStatusId!.Value)
                .Distinct()
                .ToList();
            var statuses = unitOfWork.MachineStatuses
                .Find(s => statusIds.Contains(s.Id))
                .ToDictionary(s => s.Id, s => s.Name);

            // One row per phase step (WorkOrderPhaseDetail = machine status within a phase).
            var rows = parts
                .GroupBy(p => p.WorkOrderPhaseDetailId)
                .Select(g =>
                {
                    details.TryGetValue(g.Key, out var detail);
                    phases.TryGetValue(detail?.WorkOrderPhaseId ?? Guid.Empty, out var phase);

                    var quantity = g.Sum(p => p.Quantity);
                    var realMachine = g.Sum(p => p.WorkcenterTime);
                    var realOperator = g.Sum(p => p.OperatorTime);

                    decimal theoreticalMachine = 0, theoreticalOperator = 0;
                    if (detail != null)
                    {
                        // IsCycleTime: estimate is per piece; otherwise it is the block time.
                        theoreticalMachine = detail.IsCycleTime ? detail.EstimatedTime * quantity : detail.EstimatedTime;
                        theoreticalOperator = detail.IsCycleTime ? detail.EstimatedOperatorTime * quantity : detail.EstimatedOperatorTime;
                    }

                    var statusName = detail?.MachineStatusId != null
                        ? statuses.GetValueOrDefault(detail.MachineStatusId.Value, string.Empty)
                        : string.Empty;

                    return new ProductionTimeDeviationRow
                    {
                        WorkOrderId = phase?.WorkOrderId ?? Guid.Empty,
                        WorkOrderCode = phase != null ? workOrders.GetValueOrDefault(phase.WorkOrderId, string.Empty) : string.Empty,
                        PhaseId = phase?.Id ?? Guid.Empty,
                        PhaseName = phase != null ? $"{phase.Code} - {phase.Description}" : string.Empty,
                        MachineStatusId = detail?.MachineStatusId,
                        StatusName = statusName,
                        IsCycleTime = detail?.IsCycleTime ?? false,
                        Quantity = quantity,
                        TheoreticalMachineTime = Math.Round(theoreticalMachine, 1),
                        RealMachineTime = Math.Round(realMachine, 1),
                        MachineDeviation = Math.Round(realMachine - theoreticalMachine, 1),
                        TheoreticalOperatorTime = Math.Round(theoreticalOperator, 1),
                        RealOperatorTime = Math.Round(realOperator, 1),
                        OperatorDeviation = Math.Round(realOperator - theoreticalOperator, 1),
                    };
                })
                .OrderBy(r => r.WorkOrderCode)
                .ThenBy(r => r.PhaseName)
                .ToList();

            var theoreticalMachineTotal = rows.Sum(r => r.TheoreticalMachineTime);
            var realMachineTotal = rows.Sum(r => r.RealMachineTime);
            var theoreticalOperatorTotal = rows.Sum(r => r.TheoreticalOperatorTime);
            var realOperatorTotal = rows.Sum(r => r.RealOperatorTime);

            return Task.FromResult(new ProductionTimeDeviationResult
            {
                TheoreticalMachineTime = Math.Round(theoreticalMachineTotal, 1),
                RealMachineTime = Math.Round(realMachineTotal, 1),
                MachineDeviation = Math.Round(realMachineTotal - theoreticalMachineTotal, 1),
                MachineDeviationPercent = theoreticalMachineTotal == 0
                    ? 0
                    : Math.Round((realMachineTotal - theoreticalMachineTotal) / theoreticalMachineTotal * 100, 1),
                TheoreticalOperatorTime = Math.Round(theoreticalOperatorTotal, 1),
                RealOperatorTime = Math.Round(realOperatorTotal, 1),
                OperatorDeviation = Math.Round(realOperatorTotal - theoreticalOperatorTotal, 1),
                OperatorDeviationPercent = theoreticalOperatorTotal == 0
                    ? 0
                    : Math.Round((realOperatorTotal - theoreticalOperatorTotal) / theoreticalOperatorTotal * 100, 1),
                StepCount = rows.Count,
                DeviatedStepCount = rows.Count(r => r.MachineDeviation > 0),
                Rows = rows,
            });
        }
    }
}
