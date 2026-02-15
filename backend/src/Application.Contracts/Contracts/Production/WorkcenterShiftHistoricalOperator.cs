using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Contracts
{
    public class WorkcenterShiftHistoricalOperator : Contract
    {
        public Guid Id { get; set; }
        public string? Key { get; set; } = string.Empty;
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

    }
}
