using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Contracts
{
    public class DetailedWorkOrder : Contract
    {
        public Guid WorkOrderId { get; set; }
        public string WorkOrderCode { get; set; } = String.Empty;
        public string WorkOrderStatusCode { get; set; } = String.Empty;
        public string WorkOrderStatusDescription { get; set; } = String.Empty;
        public int PlannedQuantity {  get; set; }
        public DateTime? WorkOrderStartTime { get; set; }
        public DateTime? WorkOrderEndTime { get; set;}
        public int WorkOrderOrder { get; set; }
        public string WorkOrderComment { get; set; } = String.Empty;
        public DateTime? PlannedDate { get; set; }
        public string ReferenceCode { get; set; } = String.Empty;
        public string ReferenceDescription { get; set; } = String.Empty;
        public string ReferenceVersion { get; set; } = String.Empty;
        public decimal ReferenceCost { get; set; }
        public Guid WorkOrderPhaseId { get; set; }
        public string WorkOrderPhaseCode { get; set; } = String.Empty;
        public string WorkOrderPhaseDescription { get;set; } = String.Empty;
        public string WorkOrderPhaseComment { get; set; } = String.Empty;
        public string WorkOrderPhaseStatusCode { get; set; } = String.Empty;
        public string WorkOrderPhaseStatusDescription { get; set; } = String.Empty;
        public DateTime? WorkOrderPhaseStartTime { get; set; }
        public DateTime? WorkOrderPhaseEndTime { get; set; }
        public Guid WorkOrderPhaseDetailId { get; set; }
        public int WorkOrderPhaseDetailOrder { get; set; }
        public decimal WorkOrderPhaseDetailEstimatedTime { get; set; }
        public string WorkOrderPhaseDetailComment { get; set; } = String.Empty;
        public string MachineStatusName { get; set; } = String.Empty;
        public string MachineStatusDescription { get; set; } = String.Empty;
        public Guid WorkcenterId { get; set; }
        public string WorkcenterName { get; set; } = String.Empty;
        public string WorkcenterDescription { get; set; } = String.Empty;
        public decimal WorkcenterCost { get; set; }
        public bool PreferredWorkcenter { get; set; }
    }
}
