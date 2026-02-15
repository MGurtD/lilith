using Domain.Entities.Shared;

namespace Domain.Entities.Purchase
{
    public class SupplierReference : Entity
    {
        public Reference? Reference { get; set; }
        public Guid ReferenceId { get; set; }

        public Supplier? Supplier { get; set; }
        public Guid SupplierId { get; set; }

        public string SupplierCode { get; set; } = string.Empty;
        public string SupplierDescription { get; set; } = string.Empty;
        public decimal SupplierPrice { get; set; } = decimal.Zero;
        public int SupplyDays { get; set; }

    }
}
