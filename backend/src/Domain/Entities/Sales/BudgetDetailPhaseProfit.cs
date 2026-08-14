namespace Domain.Entities.Sales;

// Persisted per-phase-step profit percentage edited for a budget line, keyed by the
// originating WorkMaster phase detail so values survive across reloads.
public class BudgetDetailPhaseProfit : Entity
{
    public Guid BudgetDetailId { get; set; }
    public BudgetDetail? BudgetDetail { get; set; }
    public Guid WorkMasterPhaseDetailId { get; set; }
    public decimal ProfitPercentage { get; set; } = decimal.Zero;
}
