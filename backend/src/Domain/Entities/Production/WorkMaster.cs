using Domain.Entities;
using Domain.Entities.Shared;

namespace Domain.Entities.Production;
public class WorkMaster : Entity
{
    public Guid ReferenceId { get; set;}
    public Reference? Reference { get; set;}
    public decimal BaseQuantity { get; set;}
    public decimal OperatorCost { get; set;} = decimal.Zero;
    public decimal MachineCost { get; set; } = decimal.Zero;
    public decimal ExternalCost { get; set; } = decimal.Zero;
    public decimal MaterialCost { get; set; } = decimal.Zero;
    public decimal TotalWeight { get; set; } = decimal.Zero;
    public decimal Volume { get; set; } = decimal.Zero;
    public int Mode { get; set; } //1 - prototip, 2 - srie curta, 3 - srie llarga

    public ICollection<WorkMasterPhase> Phases { get; set; } = new List<WorkMasterPhase>();
}
