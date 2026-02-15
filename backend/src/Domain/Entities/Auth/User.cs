namespace Domain.Entities.Auth
{
    public class User : Entity
    {
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string PreferredLanguage { get; set; } = "ca";

        public Guid RoleId { get; set; }
        public Role? Role { get; set; }

        // Profile association (optional)
        public Guid? ProfileId { get; set; }
        public Profile? Profile { get; set; }

    }
}
