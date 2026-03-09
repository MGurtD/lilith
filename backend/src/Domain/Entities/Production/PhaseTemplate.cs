namespace Domain.Entities.Production;

public class PhaseTemplate : Entity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public ICollection<PhaseTemplateDetail> Details { get; set; } = [];
}
