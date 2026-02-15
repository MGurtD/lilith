namespace Domain.Entities.Production;

public class MachineStatusReason : Entity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public Guid MachineStatusId { get; set; }
    public MachineStatus? MachineStatus { get; set; }
}
