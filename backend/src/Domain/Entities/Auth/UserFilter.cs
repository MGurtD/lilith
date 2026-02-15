namespace Domain.Entities.Auth
{
    public class UserFilter : Entity
    {
        public string Page { get; set; } = string.Empty;
        public string Key { get; set; } = string.Empty;
        public string Filter { get; set; } = string.Empty;

        public Guid UserId { get; set; }
        public User? User { get; set; }

    }
}
