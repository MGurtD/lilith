namespace Domain.Entities.Production
{
    public class Enterprise : Entity
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public Guid? DefaultSiteId { get; set; }
        public Site? DefaultSite { get; set; }
        public ICollection<Site> Sites { get; } = [];

        public string? Theme { get; set; }
        public string? PrimaryColor { get; set; }
        public string? LogoMain { get; set; }
        public string? LogoSidebar { get; set; }
        public string? TitleSidebar { get; set; }

    }
}