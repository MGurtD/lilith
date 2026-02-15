namespace Domain.Entities.Auth
{
    // Join entity for many-to-many between Profile and MenuItem
    public class ProfileMenuItem : Entity
    {
        public Guid ProfileId { get; set; }
        public Profile? Profile { get; set; }

        public Guid MenuItemId { get; set; }
        public MenuItem? MenuItem { get; set; }

        public bool IsDefault { get; set; } = false; // indicates default screen for this profile
    }
}
