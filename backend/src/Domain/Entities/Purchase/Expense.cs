namespace Domain.Entities.Purchase
{
    public class Expenses : Entity
    {
        public string Description { get; set; } = string.Empty;
        public DateTime CreationDate { get; set; }
        public DateTime PaymentDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal Amount { get; set; } = decimal.Zero;
        public bool Recurring { get; set; } = false;
        public int Frecuency { get; set; } = 0;
        public int PaymentDay { get; set; } = 0;
        public string RelatedExpenseId { get; set; } = string.Empty;
        public Guid ExpenseTypeId { get; set; }

        public ExpenseType? ExpenseType { get; set; }


    }
}
