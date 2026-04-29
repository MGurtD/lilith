namespace Domain.Entities.Sales;

public class BudgetExternalServices : Entity
{
    public Guid BudgetId { get; set; }
    public Guid ReferenceId { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Weight { get; set; }
    public decimal Volume { get; set; }
    public decimal Quantity { get; set; }
    public Guid SupplierId { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }

    public ICollection<BudgetExternalServiceDetail> Details { get; set; } = new List<BudgetExternalServiceDetail>();
}