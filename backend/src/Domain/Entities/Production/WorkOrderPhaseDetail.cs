namespace Domain.Entities.Production;

public class WorkOrderPhaseDetail : Entity
{
    public Guid WorkOrderPhaseId { get; set;}
    public WorkOrderPhase? WorkOrderPhase { get; set; }
    public Guid? MachineStatusId { get; set;}
    public MachineStatus? MachineStatus { get; set; }

    public int Order { get; set; }
    public bool IsCycleTime { get;set; }
    public decimal EstimatedTime { get; set; }
    public decimal EstimatedOperatorTime { get; set; }
    public string Comment { get; set; } = string.Empty;

}
