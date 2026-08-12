namespace Domain.Entities.Sales;

// Persisted per-phase-step profit percentage edited for a sales order line, keyed by the
// originating WorkMaster phase detail so values survive across reloads.
public class SalesOrderDetailPhaseProfit : Entity
{
    public Guid SalesOrderDetailId { get; set; }
    public SalesOrderDetail? SalesOrderDetail { get; set; }
    public Guid WorkMasterPhaseDetailId { get; set; }
    public decimal ProfitPercentage { get; set; } = decimal.Zero;
}
