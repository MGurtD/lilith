namespace Domain.Entities.Purchase
{
    public class ReferenceFormat : Entity
    {
        public string Code { get; set; } = string.Empty;
        public string? Description { get; set; }
    }
}
