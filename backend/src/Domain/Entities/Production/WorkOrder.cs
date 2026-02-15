using Domain.Entities.Shared;

namespace Domain.Entities.Production;
public class WorkOrder : Entity
{
    public string Code { get; set; } = string.Empty;
    public Guid ReferenceId { get; set; }
    public Reference? Reference { get; set;}
    public Guid ExerciseId { get; set; }
    public Exercise? Exercise { get; set; }
    public Guid WorkMasterId { get; set; }
    public WorkMaster? WorkMaster { get; set; }
    public Guid StatusId { get; set; }
    public Status? Status { get; set; }

    public decimal OperatorCost { get; set; }
    public decimal MachineCost { get; set; }
    public decimal OperatorTime { get; set; }
    public decimal MachineTime { get; set; }    
    public decimal MaterialCost { get; set; }

    public DateTime PlannedDate { get; set; }
    public decimal PlannedQuantity { get; set; }

    public decimal TotalQuantity { get; set; }
    public DateTime? StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public int Order { get; set; }
    public string Comment { get; set; } = string.Empty;

    public ICollection<WorkOrderPhase> Phases { get; set; } = new List<WorkOrderPhase>();
}
public class UpdateWorkOrderOrderDTO
{
    public Guid Id { get; set; }
    public int Order { get; set; }
}

public class WorkOrderPhaseEstimationDto
{
    public string Code { get; set; } = string.Empty;
    public decimal PlannedQuantity { get; set; }
    public string PhaseCode { get; set; } = string.Empty;
    public string PhaseDescription { get; set; } = string.Empty;
    public Guid? WorkcenterTypeId { get; set; }
    public decimal EstimatedTime { get; set; }
}