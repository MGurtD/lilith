using System.ComponentModel.DataAnnotations;

namespace Application.Contracts
{
    public class CreateHeaderRequest
    {
        [Required]
        public Guid Id { get; set; }
        [Required]
        public DateTime Date { get; set; }
        [Required]
        public Guid ExerciseId { get; set;}
        [Required]
        public Guid CustomerId { get; set; }
        public Guid? InitialStatusId { get; set; }
    }
}
