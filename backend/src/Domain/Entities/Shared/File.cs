namespace Domain.Entities
{
    public class File : Entity
    {
        public string Entity { get; set; } = string.Empty;
        public Guid EntityId { get; set; }
        public FileType Type { get; set; }
        public decimal Size { get; set; }
        public string OriginalName { get; set; } = string.Empty;
        public string Path { get; set; } = string.Empty;
    }

    public enum FileType
    {
        Document,
        Image,
        Video,
        Sound,
        Report
    }
}
