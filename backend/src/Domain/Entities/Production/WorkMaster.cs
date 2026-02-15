using Domain.Entities;
using Domain.Entities.Shared;

namespace Domain.Entities.Production;
public class WorkMaster : Entity
{
    public Guid ReferenceId { get; set;}
    public Reference? Reference { get; set;}
    public decimal BaseQuantity { get; set;}
    public decimal operatorCost { get; set;} = decimal.Zero;
    public decimal machineCost { get; set; } = decimal.Zero;
    public decimal externalCost { get; set; } = decimal.Zero;
    public decimal materialCost { get; set; } = decimal.Zero;
    public decimal totalWeight { get; set; } = decimal.Zero;
    public int Mode { get; set; } //1 - prototip, 2 - sèrie curta, 3 - sèrie llarga

    public ICollection<WorkMasterPhase> Phases { get; set; } = new List<WorkMasterPhase>();
}
