namespace Domain.Entities
{
    public class Exercise : Entity
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public string PurchaseOrderCounter { get; set; } = string.Empty;
        public string ReceiptCounter { get; set; } = string.Empty;
        public string PurchaseInvoiceCounter { get; set; } = string.Empty;
        public string SalesOrderCounter { get; set; } = string.Empty;
        public string SalesInvoiceCounter { get; set; } = string.Empty;
        public string DeliveryNoteCounter { get; set; } = string.Empty;
        public string BudgetCounter { get; set; } = string.Empty;
        public string WorkOrderCounter { get; set; } = string.Empty;
        
        public decimal MaterialProfit { get; set; } = 30m;
        public decimal ExternalProfit { get; set; } = 30m;
    }
}
