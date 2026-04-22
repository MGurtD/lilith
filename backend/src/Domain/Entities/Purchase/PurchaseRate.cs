using Domain.Entities.Shared;

namespace Domain.Entities.Purchase
{
    public class PurchaseRate : Entity
    {
        public string Name { get; set; } = string.Empty;
        public Guid SupplierId { get; set; }
        public Supplier? Supplier { get; set; }
        public DateOnly ValidFrom { get; set; }
        public DateOnly ValidTo { get; set; }
        public ICollection<PurchaseRateDetail> Details { get; set; } = [];
    }
}
