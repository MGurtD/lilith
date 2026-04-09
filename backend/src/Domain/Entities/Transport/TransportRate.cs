namespace Domain.Entities.Transport
{
    public class TransportRate : Entity
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public Guid SupplierId { get; set; }        
        public DateOnly ValidFrom { get; set; }
        public DateOnly ValidTo { get; set; }
        public ICollection<TransportRateDetail> Details { get; set; } = [];
    }

    public class TransportRateDetail : Entity
    {
        public Guid TransportRateId { get; set; }
        public TransportRate? TransportRate { get; set; }
        public decimal MinWeight { get; set; } = 0;
        public decimal MaxWeight { get; set; } = 0;
        public decimal MinVolume { get; set; } = 0;
        public decimal MaxVolume { get; set; } = 0;
        public decimal MinDistance { get; set; } = 0;
        public decimal MaxDistance { get; set; } = 0;
        public decimal Price { get; set; } = 0;

    }
}