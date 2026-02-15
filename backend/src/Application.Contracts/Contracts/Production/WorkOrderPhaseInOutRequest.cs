using System.ComponentModel.DataAnnotations;

namespace Application.Contracts;

public class WorkOrderPhaseInRequest
{
    [Required]
    public Guid WorkcenterId { get; set; }
    [Required]
    public Guid WorkOrderPhaseId { get; set; }
    [Required]
    public DateTime Timestamp { get; set; }
}

public class WorkOrderPhaseOutRequest : WorkOrderPhaseInRequest
{
    public Guid WorkOrderStatusId { get; set; }
    public Guid? NextWorkOrderPhaseId { get; set; }
    public Guid? NextMachineStatusId { get; set; }
}
