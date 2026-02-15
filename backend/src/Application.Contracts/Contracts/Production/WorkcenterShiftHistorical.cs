using System;
using System.Collections.Generic;
using System.Text;
using Application.Contracts;

namespace Application.Contracts
{
    public class WorkcenterShiftHistorical : Contract
    {
        public Guid Id { get; set; }
        public string? Key { get; set; } = string.Empty;
        public string Operator { get; set; } = string.Empty;
        public string Workcenter { get; set; } = string.Empty;
        public string MachineStatus { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public decimal QuantityOk { get; set; }
        public decimal QuantityKo { get; set; }
        public decimal TotalHours { get; set; }
        public decimal OperatorCost { get; set; }
        public decimal WorkcenterCost { get; set; }
        public decimal TotalCost { get; set; }
        public string WorkOrderCode { get; set; } = string.Empty;
        public int PlannedQuantity { get; set; }
        public decimal EstimatedMachineCost { get; set; }
        public decimal EstimatedMachineTime { get; set; }
        public decimal EstimatedOperatorCost { get; set; }
        public decimal EstimatedOperatorTime { get; set; }
        public string WorkOrderPhaseCode { get; set; } = string.Empty;
        public string WorkOrderPhaseDescription { get; set; } = string.Empty;
        public bool IsPreferredWorkcenter { get; set; }
        public string ReferenceCode { get; set; } = string.Empty;
        public string ReferenceDescription { get; set; } = string.Empty;
        public string CustomerComercialName { get; set; } = string.Empty;
        public Guid OperatorId { get; set; }
        public Guid WorkcenterId { get; set; }
        public Guid ReferenceId { get; set; }
        public Guid CustomerId { get; set; }
        public Guid WorkOrderId { get; set; }
        public Guid WorkOrderPhaseId { get; set; }
    }
}