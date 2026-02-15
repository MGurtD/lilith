using Domain.Entities.Shared;

namespace Domain.Entities.Production;
public class WorkMasterPhaseBillOfMaterials: Entity
{
    public Guid WorkMasterPhaseId {get; set;}
    public WorkMasterPhase? WorkMasterPhase {get; set;}
    public Guid ReferenceId { get; set; }
    public Reference? Reference { get; set; }
    public decimal Quantity { get; set; }
    public decimal Width { get; set; }
    public decimal Length { get; set; }
    public decimal Height { get; set; }
    public decimal Diameter { get; set; }
    public decimal Thickness { get; set; }
}
