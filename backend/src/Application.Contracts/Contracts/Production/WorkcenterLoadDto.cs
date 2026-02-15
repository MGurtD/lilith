namespace Application.Contracts;

public record WorkcenterLoadDto
{
    public string WorkOrderCode { get; set; } = string.Empty;
    public int WorkOrderPriority { get; set; }
    public DateTime WorkOrderPlannedDate { get; set; }
    public decimal PlannedQuantity { get; set; } = decimal.Zero;
    public string PhaseCode { get; set; } = string.Empty;
    public string PhaseDescription { get; set; } = string.Empty;
    public Guid? WorkcenterTypeId { get; set; }
    public string WorkcenterTypeName { get; set; } = string.Empty;
    public decimal EstimatedTime { get; set; } = decimal.Zero;    
}