using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Domain.Entities.Production;

public class WorkcenterShiftDetail : Entity
{
    public Guid WorkcenterShiftId { get; set; }
    public WorkcenterShift? WorkcenterShift { get; set; }
    public Guid MachineStatusId { get; set; }
    public MachineStatus? MachineStatus { get; set; }
    public Guid? MachineStatusReasonId { get; set; }
    public MachineStatusReason? MachineStatusReason { get; set; }
    public Guid? OperatorId { get; set; }
    public Operator? Operator { get; set; }
    public Guid? WorkOrderPhaseId { get; set; }
    public WorkOrderPhase? WorkOrderPhase { get; set; }
    public bool Current { get; set; } = true;
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public decimal OperatorCost { get; set; }
    public decimal WorkcenterCost { get; set; }
    public int ConcurrentOperatorWorkcenters { get; set; }
    public int ConcurrentWorkorderPhases { get; set; }
    public decimal QuantityOk { get; set; }
    public decimal QuantityKo { get; set; }

    public void Open(DateTime StartTime)
    {
        Current = true;
        this.StartTime = StartTime;
    }
    public void Close(DateTime EndTime)
    {
        Current = false;
        this.EndTime = EndTime;
    }

    [NotMapped]
    [JsonIgnore]
    public override DateTime CreatedOn { get => base.CreatedOn; set => base.CreatedOn = value; }
    [NotMapped]
    [JsonIgnore]
    public override DateTime UpdatedOn { get => base.UpdatedOn; set => base.UpdatedOn = value; }
}
