using System.ComponentModel.DataAnnotations;

namespace Application.Contracts
{
    public class ChangeStatusRequest
    {
        [Required]
        public Guid Id { get; set; }
        [Required]
        public Guid StatusToId { get; set; }
    }
}
