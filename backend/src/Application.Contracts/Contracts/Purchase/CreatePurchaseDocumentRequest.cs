using System.ComponentModel.DataAnnotations;

namespace Application.Contracts
{
    public class CreatePurchaseDocumentRequest
    {
        [Required]
        public Guid Id { get; set; }
        [Required]
        public DateTime Date { get; set; }
        [Required]
        public Guid ExerciseId { get; set; }
        [Required]
        public Guid SupplierId { get; set; }
    }
}
