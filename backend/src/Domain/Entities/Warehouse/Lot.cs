using Domain.Entities.Shared;

namespace Domain.Entities.Warehouse
{
    public class Lot : Entity
    {
        public Guid ReferenceId { get; set; }
        public Reference? Reference { get; set; }
        public string Code { get; set; } = string.Empty;
        public string? SupplierLotCode { get; set; }
        public DateTime? ExpirationDate { get; set; }
        public DateTime? ClosedDate { get; set; }
        public decimal RemainingQuantity { get; set; }
        public string? Comment { get; set; }
    }
}
