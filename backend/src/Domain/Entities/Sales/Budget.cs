namespace Domain.Entities.Sales
{
    public class Budget : Entity
    {
        public string Number { get; set; }
        public DateTime Date { get; set; }
        public decimal Amount { get; set; }
        public decimal TransportCost { get; set; }
        public decimal TotalWeight { get; set; }        
        public int DeliveryDays { get; set; }
        public DateTime? AcceptanceDate { get; set; }
        public Exercise? Exercise { get; set; }
        public Guid ExerciseId { get; set; }
        public Customer? Customer { get; set; }
        public Guid CustomerId { get; set; }
        public Guid? StatusId { get; set; }
        public Status? Status { get; set; }
        public DateTime? AutoRejectedDate { get; set; }
        public string Notes { get; set; } = String.Empty;
        public string UserNotes { get; set; } = String.Empty;

        public ICollection<BudgetDetail> Details { get; set; } = new List<BudgetDetail>();
        public ICollection<BudgetTransport> Transports { get; set; } = new List<BudgetTransport>();

        // Constructors
        public Budget()
        {
            Number = string.Empty;
            Date = DateTime.Now;
        }

        public void Accept() 
        {
            AcceptanceDate = DateTime.Now;
        }
    }
}