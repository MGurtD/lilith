using System.ComponentModel.DataAnnotations;

namespace Application.Contracts;

public class WorkOrderPhaseAndStatusInRequest
{
    [Required]
    public Guid WorkcenterId { get; set; }
    [Required]
    public Guid WorkOrderPhaseId { get; set; }
    [Required]
    public Guid MachineStatusId { get; set; }
    [Required]
    public DateTime Timestamp { get; set; }
    public Guid? WorkOrderStatusId { get; set; }
}
