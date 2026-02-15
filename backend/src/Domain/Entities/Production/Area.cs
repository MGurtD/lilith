namespace Domain.Entities.Production;

public class Area : Entity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Guid SiteId { get; set; }
    public Site? Site { get; set; }
    public bool IsVisibleInPlant { get; set; } = true;

    public ICollection<Workcenter> Workcenters { get; } = [];
}
