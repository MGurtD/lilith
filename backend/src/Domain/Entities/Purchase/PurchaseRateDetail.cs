using Domain.Entities.Shared;

namespace Domain.Entities.Purchase
{
    public class PurchaseRateDetail : Entity
    {
        public Guid PurchaseRateId { get; set; }
        public PurchaseRate? PurchaseRate { get; set; }
        public Guid ReferenceId { get; set; }
        public Reference? Reference { get; set; }
        public decimal From { get; set; } = 0;
        public decimal To { get; set; } = 0;
        public int CalculationType { get; set; } = 2; // Default to Units
        public decimal Price { get; set; } = 0;
    }
}
