namespace Domain.Entities
{
    public class StatusTransition : Entity
    {
        public string Name { get; set; } = string.Empty;

        public Status? Status { get; set; }
        public Guid? StatusId { get; set; }

        public Guid? StatusToId { get; set; }
    }
}
