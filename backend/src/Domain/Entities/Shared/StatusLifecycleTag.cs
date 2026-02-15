namespace Domain.Entities.Shared;

public class StatusLifecycleTag : Entity
{
    public Status? Status { get; set; }
    public Guid StatusId { get; set; }

    public LifecycleTag? LifecycleTag { get; set; }
    public Guid LifecycleTagId { get; set; }
}
