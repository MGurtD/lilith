namespace Domain.Entities.Shared;

public class Lifecycle : Entity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public Guid? InitialStatusId { get; set; }
    public Guid? FinalStatusId { get; set; }

    public ICollection<Status>? Statuses { get; set; }
    public ICollection<LifecycleTag>? Tags { get; set; }
}