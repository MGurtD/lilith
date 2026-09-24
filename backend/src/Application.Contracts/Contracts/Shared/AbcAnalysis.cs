namespace Application.Contracts
{
    public class AbcRow : Contract
    {
        public Guid EntityId { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public decimal Value { get; set; }
        public decimal ValuePercent { get; set; }
        public decimal CumulativePercent { get; set; }
        public string Category { get; set; } = string.Empty;
        public int Rank { get; set; }
    }

    public class AbcCategorySummary : Contract
    {
        public string Category { get; set; } = string.Empty;
        public int ItemCount { get; set; }
        public decimal ItemPercent { get; set; }
        public decimal Value { get; set; }
        public decimal ValuePercent { get; set; }
    }

    public class AbcAnalysisResult : Contract
    {
        public decimal TotalValue { get; set; }
        public int TotalItems { get; set; }
        public IEnumerable<AbcCategorySummary> Categories { get; set; } = new List<AbcCategorySummary>();
        public IEnumerable<AbcRow> Rows { get; set; } = new List<AbcRow>();
    }
}
