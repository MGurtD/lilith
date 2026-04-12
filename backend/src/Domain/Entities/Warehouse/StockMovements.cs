using Domain.Entities.Purchase;
using Domain.Entities.Shared;

namespace Domain.Entities.Warehouse
{
    public struct StockMovementType
    {
        public const string INPUT = "INPUT";
        public const string OUTPUT = "OUTPUT";
        public const string SUPPLY = "SUPPLY";
        public const string CONSUMPTION = "CONSUMPTION";
        public const string PRODUCTION = "PRODUCTION";
    }

    public class StockMovement : Entity
    {
        public Guid StockId { get; set; }
        public Stock? Stock { get; set; }
        public Guid? LocationId { get; set; }
        public Location? Location { get; set; }
        public Guid ReferenceId { get; set; }
        public Reference? Reference { get; set; }
        public string Description { get; set; } = string.Empty;
        public string MovementType { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal Width { get; set; }
        public decimal Length { get; set; }
        public decimal Height { get; set; }
        public DateTime MovementDate { get; set; }
        public decimal Diameter { get; set; }
        public decimal Thickness { get; set; }
        public string? Entity { get; set; }
        public Guid? EntityId { get; set; }

        public void SetFromReceiptDetail(ReceiptDetail receiptDetail)
        {
            ReferenceId = receiptDetail.ReferenceId;

            Quantity = receiptDetail.Quantity;
            Width = receiptDetail.Width;
            Diameter = receiptDetail.Diameter;
            Length = receiptDetail.Lenght;
            Height = receiptDetail.Height;
            Thickness = receiptDetail.Thickness;
        }
    }
}
