namespace Application.Contracts.Migration
{
    /// <summary>Parsed contents of a single worksheet, keyed by header name.</summary>
    public sealed class SheetData
    {
        public string Name { get; set; } = string.Empty;
        public List<string> Headers { get; set; } = [];
        public List<SheetRow> Rows { get; set; } = [];
    }

    public sealed class SheetRow
    {
        /// <summary>1-based worksheet row number, used for error reporting.</summary>
        public int Number { get; set; }
        public Dictionary<string, string?> Values { get; set; } = new(StringComparer.OrdinalIgnoreCase);

        public string? this[string column] => Values.TryGetValue(column, out var value) ? value : null;
    }

    /// <summary>Definition of a worksheet to write: header columns plus optional data rows.</summary>
    public sealed class SheetTemplate
    {
        public string Name { get; set; } = string.Empty;
        public List<SheetColumn> Columns { get; set; } = [];
        public List<IReadOnlyList<string?>> Rows { get; set; } = [];
    }

    public sealed class SheetColumn
    {
        public string Header { get; set; } = string.Empty;
        /// <summary>Header cell note describing data type, requiredness and foreign-key hints.</summary>
        public string? Comment { get; set; }
    }
}
