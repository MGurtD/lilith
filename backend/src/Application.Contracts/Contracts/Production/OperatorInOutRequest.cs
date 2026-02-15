using System.ComponentModel.DataAnnotations;

namespace Application.Contracts;

public class OperatorInOutRequest
{
    [Required]
    public Guid WorkcenterId { get; set; }
    [Required]
    public Guid OperatorId { get; set; }
    [Required]
    public DateTime Timestamp { get; set; }
}

public enum OperatorDirection
{
    In,
    Out
}
