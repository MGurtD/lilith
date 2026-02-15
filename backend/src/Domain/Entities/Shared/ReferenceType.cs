namespace Domain.Entities.Shared
{
    public class ReferenceType : Entity
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string PrimaryColor {get; set;} = string.Empty;
        public string SecondaryColor {get; set;} = string.Empty;
        public decimal Density { get; set; }

        public ICollection<Reference>? References { get; set; }
    }
}
