namespace Domain.Entities.Auth
{
    public class MenuItem : Entity
    {
        public string Key { get; set; } = string.Empty; // unique key for frontend mapping
        public string? Icon { get; set; } // PrimeIcons name or custom
        public string? Route { get; set; } // null for folders
        public int SortOrder { get; set; } = 0;

        public Guid? ParentId { get; set; }
        public MenuItem? Parent { get; set; }
        public ICollection<MenuItem> Children { get; set; } = new List<MenuItem>();

        public ICollection<ProfileMenuItem> ProfileMenuItems { get; set; } = new List<ProfileMenuItem>();
        public ICollection<MenuItemTranslation> Translations { get; set; } = new List<MenuItemTranslation>();
    }
}
