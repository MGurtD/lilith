using Domain.Entities.Warehouse;

namespace Domain.Entities.Production;

public class WorkcenterLocation : Entity
{
    public Guid WorkcenterId { get; set; }
    public Workcenter? Workcenter { get; set; }

    public Guid LocationId { get; set; }
    public Location? Location { get; set; }
}
