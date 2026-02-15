using Domain.Entities.Purchase;
using System.ComponentModel.DataAnnotations;

namespace Application.Contracts
{
    public class AddReceptionsRequest
    {
        [Required]
        public Guid ReceiptId { get; set; }
        [Required]
        public List<AddReceptionsDetail> Receptions { get; set; } = [];
    }

    public class AddReceptionsDetail
    {
        [Required]
        public Guid PurchaseOrderDetailId { get; set; }
        [Required]
        public Guid ReceiptDetailId { get; set; }
        [Required]
        public decimal Quantity { get; set; }
        [Required]
        public decimal Price { get; set; }

        public decimal UnitPrice {
            get => Quantity > 0 ? Price / Quantity : 0;
        }
        public string User { get; set; } = string.Empty;
    }
}
