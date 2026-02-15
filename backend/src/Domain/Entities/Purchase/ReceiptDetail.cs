using Domain.Entities.Shared;
using Domain.Entities.Warehouse;

namespace Domain.Entities.Purchase
{
    public class ReceiptDetail : Entity
    {
        public Guid ReceiptId { get; set; }
        public Receipt? Receipt { get; set; }

        public Guid ReferenceId { get; set; }
        public Reference? Reference { get; set; }
        public string Description { get; set; } = string.Empty;

        public Guid? StockMovementId { get; set; }
        public StockMovement? StockMovement { get; set; }

        public int Quantity { get; set; }
        public decimal Width { get; set; } = decimal.Zero;
        public decimal Lenght { get; set; } = decimal.Zero;
        public decimal Height { get; set; } = decimal.Zero;
        public decimal Diameter { get; set; } = decimal.Zero;
        public decimal Thickness { get; set; } = decimal.Zero;
        public decimal TotalWeight { get; set; } = decimal.Zero;
        public decimal UnitWeight { get; set; } = decimal.Zero;
        public decimal KilogramPrice { get; set; } = decimal.Zero;
        public decimal UnitPrice { get; set; } = decimal.Zero;
        public decimal Amount { get; set; } = decimal.Zero;
    }
}
