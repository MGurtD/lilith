namespace Application.Contracts
{
    public class ProductionTimeDeviationRow : Contract
    {
        public Guid WorkOrderId { get; set; }
        public string WorkOrderCode { get; set; } = string.Empty;
        public Guid PhaseId { get; set; }
        public string PhaseName { get; set; } = string.Empty;
        public Guid? MachineStatusId { get; set; }
        public string StatusName { get; set; } = string.Empty;
        public bool IsCycleTime { get; set; }
        public int Quantity { get; set; }
        public decimal TheoreticalMachineTime { get; set; }
        public decimal RealMachineTime { get; set; }
        public decimal MachineDeviation { get; set; }
        public decimal TheoreticalOperatorTime { get; set; }
        public decimal RealOperatorTime { get; set; }
        public decimal OperatorDeviation { get; set; }
    }

    public class ProductionTimeDeviationResult : Contract
    {
        public decimal TheoreticalMachineTime { get; set; }
        public decimal RealMachineTime { get; set; }
        public decimal MachineDeviation { get; set; }
        public decimal MachineDeviationPercent { get; set; }
        public decimal TheoreticalOperatorTime { get; set; }
        public decimal RealOperatorTime { get; set; }
        public decimal OperatorDeviation { get; set; }
        public decimal OperatorDeviationPercent { get; set; }
        public int StepCount { get; set; }
        public int DeviatedStepCount { get; set; }
        public IEnumerable<ProductionTimeDeviationRow> Rows { get; set; } = new List<ProductionTimeDeviationRow>();
    }
}
