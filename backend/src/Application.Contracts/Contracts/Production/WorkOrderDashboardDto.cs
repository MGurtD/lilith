namespace Application.Contracts.Contracts.Production;

public class WorkOrderDashboardDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string ReferenceCode { get; set; } = string.Empty;
    public string ReferenceDescription { get; set; } = string.Empty;
    public decimal PlannedQuantity { get; set; }
    public DateTime PlannedDate { get; set; }
    public DateTime? StartTime { get; set; }

    public decimal PhaseProgressPercentage { get; set; }
    public decimal TimeProgressPercentage { get; set; }
    public decimal TheoreticalTimeMinutes { get; set; }
    public decimal ActualTimeMinutes { get; set; }

    public decimal OrderPrice { get; set; }
    public decimal TheoreticalCost { get; set; }

    public decimal AccumulatedMaterialCost { get; set; }
    public decimal AccumulatedMachineCost { get; set; }
    public decimal AccumulatedOperatorCost { get; set; }
    public decimal AccumulatedExternalCost { get; set; }
    public decimal AccumulatedTotalCost { get; set; }

    public decimal Margin { get; set; }
}
