namespace Domain.Entities.Production;

public class MachineStatus : Entity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Color { get; set;} = string.Empty;
    public bool Stopped { get; set;} = false;
    public bool OperatorsAllowed { get; set; } = true;
    public bool Default { get; set; } = false;
    public bool Closed { get; set;} = false;
    public bool Preferred { get; set; } = false;
    public bool WorkOrderAllowed { get; set; } = true;
    public string Icon { get; set;} = string.Empty;
    public ICollection<MachineStatusReason> Reasons { get; set; } = [];
}