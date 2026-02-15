namespace Domain.Entities.Auth
{
    public class Profile : Entity
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsSystem { get; set; } = false; // protects from deletion

        // Navigation collections
        public ICollection<ProfileMenuItem> ProfileMenuItems { get; set; } = new List<ProfileMenuItem>();
        public ICollection<User> Users { get; set; } = new List<User>();
    }
}
