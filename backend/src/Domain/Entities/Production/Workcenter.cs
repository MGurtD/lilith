namespace Domain.Entities.Production;

public class Workcenter : Entity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public double costHour { get; set; } = 0.0;
    public Guid AreaId { get; set; }
    public Area? Area { get; set; }
    public Guid WorkcenterTypeId { get; set; }
    public WorkcenterType? WorkcenterType { get; set; }
    public Guid ShiftId { get; set; }
    public Shift? Shift { get; set; }
    public decimal ProfitPercentage { get; set; } = decimal.Zero;
    public bool MultiWoAvailable { get; set; } = false;
    public ICollection<WorkcenterLocation> WorkcenterLocations { get; set; } = [];
}