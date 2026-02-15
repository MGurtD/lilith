namespace Domain.Entities.Shared;

public class LifecycleTag : Entity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? Color { get; set; } = string.Empty;
    public string? Icon { get; set; } = string.Empty;

    public Lifecycle? Lifecycle { get; set; }
    public Guid LifecycleId { get; set; }

    public ICollection<StatusLifecycleTag>? StatusTags { get; set; }
}
