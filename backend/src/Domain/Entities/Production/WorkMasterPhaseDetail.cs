namespace Domain.Entities.Production;

public class WorkMasterPhaseDetail : Entity
{
    public Guid WorkMasterPhaseId { get; set;}
    public WorkMasterPhase? WorkMasterPhase { get; set; }    
    public Guid MachineStatusId { get; set;}
    public MachineStatus? MachineStatus { get; set; }

    public int Order { get; set; }
    public bool IsCycleTime { get; set; }
    public decimal EstimatedTime { get; set; }
    public decimal EstimatedOperatorTime { get; set; }
    public string Comment { get; set; } = string.Empty;

}
