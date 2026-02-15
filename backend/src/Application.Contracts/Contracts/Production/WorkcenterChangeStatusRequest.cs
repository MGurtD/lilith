using System.ComponentModel.DataAnnotations;

namespace Application.Contracts;

public class WorkcenterChangeStatusRequest
{
    [Required]
    public Guid WorkcenterId { get; set; }
    [Required]
    public Guid MachineStatusId { get; set; }
    public Guid? MachineStatusReasonId { get; set; }
    [Required]
    public DateTime Timestamp { get; set; }
}
