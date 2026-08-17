namespace Application.Contracts.Migration
{
    /// <summary>Entity group exposed to the client as a selectable migration target.</summary>
    public sealed class MigrationEntityInfo
    {
        public string Key { get; set; } = string.Empty;
        public string DisplayNameKey { get; set; } = string.Empty;
    }

    public sealed class ImportReport
    {
        public int Total { get; set; }
        public int Inserted { get; set; }
        public int Skipped { get; set; }
        public List<ImportRowError> Errors { get; set; } = [];
    }

    public sealed class ImportRowError
    {
        public string Sheet { get; set; } = string.Empty;
        public int Row { get; set; }
        public string? Code { get; set; }
        public string Reason { get; set; } = string.Empty;
    }
}
