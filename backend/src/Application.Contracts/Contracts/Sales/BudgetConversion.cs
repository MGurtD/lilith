namespace Application.Contracts
{
    public class BudgetConversionRow : Contract
    {
        public Guid CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public Guid BudgetId { get; set; }
        public string BudgetNumber { get; set; } = string.Empty;
        public DateTime BudgetDate { get; set; }
        public Guid? StatusId { get; set; }
        public decimal Amount { get; set; }
        public Guid? OrderId { get; set; }
        public string? OrderNumber { get; set; }
        public DateTime? OrderDate { get; set; }
        public decimal? OrderAmount { get; set; }
        public int? DaysToConversion { get; set; }
    }

    public class BudgetConversionResult : Contract
    {
        public int TotalBudgets { get; set; }
        public int TotalOrders { get; set; }
        public decimal ConversionRate { get; set; }
        public double AverageAcceptanceDays { get; set; }
        public decimal TotalBudgetAmount { get; set; }
        public decimal TotalConvertedAmount { get; set; }
        public IEnumerable<BudgetConversionRow> Rows { get; set; } = new List<BudgetConversionRow>();
    }
}
