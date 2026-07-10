namespace Application.Contracts.Dtos
{
    /// <summary>
    /// Branding payload returned by <c>GET /api/Branding/{id}</c>.
    /// Contains only the branding subset of <see cref="Domain.Entities.Production.Enterprise"/>
    /// so the boot-time fetch stays lightweight and does not leak internal fields.
    /// </summary>
    public class BrandingDto
    {
        public string? Theme { get; set; }
        public string? PrimaryColor { get; set; }
        public string? LogoMain { get; set; }
        public string? LogoSidebar { get; set; }
        public string? TitleSidebar { get; set; }
    }
}