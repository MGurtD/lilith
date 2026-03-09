namespace Application.Contracts;

public class GenerateProductionPartsRequest
{
    public Guid WorkOrderPhaseId { get; set; }
    public Guid WorkcenterId { get; set; }
    public DateTime ClosedAt { get; set; }
}
