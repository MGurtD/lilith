namespace Application.Contracts
{
    public class ConsolidatedExpense : Contract
    {
        public double YearPaymentDate { get; set; }
        public double MonthPaymentDate { get; set; }
        public double WeekPaymentDate { get; set; }
        public DateTime PaymentDate { get; set; }
        public string Type { get; set; } = null!;
        public string TypeDetail { get; set; } = null!;
        public string Description { get; set; } = null!;
        public decimal Amount { get; set; }

        public string MonthPaymentKey
        {
            get
            {
                return $"{YearPaymentDate}-{MonthPaymentDate}";
            }
        }

        public string WeekPaymentKey
        {
            get
            {
                return $"{YearPaymentDate}-{WeekPaymentDate}";
            }
        }
    }
}
