namespace Domain.Entities.Production;

public class WorkcenterProfitPercentage : Entity
{
    public Guid WorkcenterId { get; set; }
    public Workcenter? Workcenter { get; set; }
    public decimal ProfitPercentage { get; set; } = decimal.Zero;
}