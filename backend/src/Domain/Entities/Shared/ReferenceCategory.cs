namespace Domain.Entities.Shared
{

    public struct ReferenceCategories
    {
        public const string Product = "Product";
        public const string Service = "Service";
        public const string Material = "Material";
        public const string Tool = "Tool";
    }

    public class ReferenceCategory
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool Disabled { get; set; }

        public ICollection<Reference>? References { get; set; }
    }
}
