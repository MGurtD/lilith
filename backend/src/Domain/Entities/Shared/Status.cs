using Domain.Entities.Shared;

namespace Domain.Entities
{
    public class Status : Entity
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        public string? Color { get; set; } = string.Empty;

        public Lifecycle? Lifecycle { get; set; }
        public Guid? LifecycleId { get; set; }

        public ICollection<StatusTransition>? Transitions { get; set; }
        public ICollection<StatusLifecycleTag>? LifecycleTags { get; set; }
    }
}
