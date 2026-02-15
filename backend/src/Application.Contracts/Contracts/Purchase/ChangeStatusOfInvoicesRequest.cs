using System.ComponentModel.DataAnnotations;

namespace Application.Contracts
{
    public class ChangeStatusOfInvoicesRequest
    {
        [Required]
        public IEnumerable<Guid> Ids { get; set; } = Enumerable.Empty<Guid>();
        [Required]
        public Guid StatusToId { get; set; }
    }
}
