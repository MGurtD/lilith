using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Domain.Entities.Production;

public class WorkcenterShift : Entity
{
    public Guid WorkcenterId { get; set; }
    public Workcenter? Workcenter { get; set; }
    public Guid ShiftDetailId { get; set; }
    public ShiftDetail? ShiftDetail { get; set; }
    public bool Current { get; set; } = true;
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public ICollection<WorkcenterShiftDetail> Details { get; set; } = [];

    [NotMapped]
    [JsonIgnore]
    public override DateTime CreatedOn { get => base.CreatedOn; set => base.CreatedOn = value; }
    [NotMapped]
    [JsonIgnore]
    public override DateTime UpdatedOn { get => base.UpdatedOn; set => base.UpdatedOn = value; }
}
