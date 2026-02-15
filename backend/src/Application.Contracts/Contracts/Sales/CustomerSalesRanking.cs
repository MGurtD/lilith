namespace Application.Contracts
{
    public class CustomerSalesRanking : Contract
    {
        public Guid CustomerId { get; set; }
        public string CustomerCode { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public int Year { get; set; }
        public int Month { get; set; }
        public int Quarter { get; set; }
        public decimal TotalSales { get; set; }
        public int InvoiceCount { get; set; }
        
        public string MonthKey => $"{Year}-{Month:D2}";
        public string QuarterKey => $"{Year}-Q{Quarter}";
    }
}
