namespace Domain.Entities.Production
{
    public class Enterprise : Entity, IMasterData
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public Guid? DefaultSiteId { get; set; }
        public Site? DefaultSite { get; set; }
        public string? BrandName { get; set; }
        public string? PrimaryColor { get; set; }
        public Guid? LogoMainFileId { get; set; }
        public Guid? LogoSidebarFileId { get; set; }
        public Guid? LogoWatermarkFileId { get; set; }
        public bool ReportWatermarkEnabled { get; set; } = true;
        public ICollection<Site> Sites { get; } = [];

    }
}
