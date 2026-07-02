using System.ComponentModel.DataAnnotations;

namespace Application.Contracts
{
    public class CreateRectificativeInvoiceRequest
    {
        [Required]
        public Guid Id { get; set; }
        public bool CreateCorrectionInvoice { get; set; } = false;
        public decimal Quantity { get; set; }
    }
}
