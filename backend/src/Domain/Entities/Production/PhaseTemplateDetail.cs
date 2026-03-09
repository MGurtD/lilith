namespace Domain.Entities.Production;

public class PhaseTemplateDetail : Entity
{
    public Guid PhaseTemplateId { get; set; }
    public PhaseTemplate? PhaseTemplate { get; set; }
    public Guid MachineStatusId { get; set; }
    public MachineStatus? MachineStatus { get; set; }
    public int Order { get; set; }
    public string Comment { get; set; } = string.Empty;
}
