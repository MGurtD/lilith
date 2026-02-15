using Domain.Entities.Purchase;

namespace Domain.Entities.Warehouse
{
    public class ReferenceDimensions
    {
        public decimal Density { get; set; }
        public int Quantity { get; set; }
        public decimal Width { get; set; }
        public decimal Length { get; set; }
        public decimal Height { get; set; }
        public decimal Diameter { get; set; }
        public decimal Thickness { get; set; }

        public void SetFromReceiptDetail(ReceiptDetail receiptDetail)
        {            
            Quantity = receiptDetail.Quantity;
            Width = receiptDetail.Width;
            Length = receiptDetail.Lenght;
            Height = receiptDetail.Height;
            Diameter = receiptDetail.Diameter;
            Thickness = receiptDetail.Thickness;
        }
    }
}
