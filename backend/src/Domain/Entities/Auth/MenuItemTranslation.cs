namespace Domain.Entities.Auth
{
    public class MenuItemTranslation : Entity
    {
        public Guid MenuItemId { get; set; }
        public MenuItem? MenuItem { get; set; }

        public string LanguageCode { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
    }
}
