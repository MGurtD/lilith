namespace Domain.Entities.Production;

public class WorkOrderPhaseRejection : Entity
{
    public Guid WorkOrderPhaseId { get; set; }
    public WorkOrderPhase? WorkOrderPhase { get; set; }
    public Guid RejectionReasonId { get; set; }
    public RejectionReason? RejectionReason { get; set; }
    public Guid? WorkcenterShiftDetailId { get; set; }
    public WorkcenterShiftDetail? WorkcenterShiftDetail { get; set; }
    public decimal Quantity { get; set; }
}
