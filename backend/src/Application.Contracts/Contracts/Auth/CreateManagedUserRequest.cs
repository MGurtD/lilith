using System.ComponentModel.DataAnnotations;

namespace Application.Contracts
{
    public class CreateManagedUserRequest
    {
        [Required]
        public string Username { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;

        [Required]
        public string RepeatPassword { get; set; } = string.Empty;

        [Required]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        public string LastName { get; set; } = string.Empty;

        [Required]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string PreferredLanguage { get; set; } = string.Empty;

        [Required]
        public Guid RoleId { get; set; }

        public Guid? ProfileId { get; set; }
    }
}
