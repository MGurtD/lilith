namespace Domain.Entities.Sales;

public class BudgetExternalServiceDetail : Entity
{
    public Guid BudgetExternalServiceId { get; set; }
    public BudgetExternalServices? BudgetExternalService { get; set; }
    public Guid BudgetDetailId { get; set; }
    public BudgetDetail? BudgetDetail { get; set; }
    public decimal Weight { get; set; }
    public decimal Volume { get; set; }
    public decimal Quantity { get; set; }
}
