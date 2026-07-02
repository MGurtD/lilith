namespace Domain.Entities.Auth
{
    public class UserTableView : Entity
    {
        public string Page { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public bool IsDefault { get; set; } = false;
        public string ViewConfig { get; set; } = string.Empty;

        public Guid UserId { get; set; }
        public User? User { get; set; }
    }
}