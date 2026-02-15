using Domain.Entities.Purchase;
using Domain.Entities.Shared;

namespace Domain.Entities.Production;

public class WorkOrderPhase : Entity
{
    public string Code { get; set; } = string.Empty;
    
    public int CodeAsNumber
    {
        get
        {
            return int.TryParse(Code, out var number) ? number : 0;
        }
    }
    public string Description { get; set; } = string.Empty;

    public Guid WorkOrderId { get; set; }
    public WorkOrder? WorkOrder { get; set; }
    public Guid StatusId { get; set; }
    public Status? Status { get; set; }
    public Guid? OperatorTypeId { get; set; }
    public OperatorType? OperatorType { get; set; }
    public Guid? WorkcenterTypeId { get; set; }
    public WorkcenterType? WorkcenterType { get; set; }
    public decimal ProfitPercentage { get; set; } = decimal.Zero;
    public Guid? PreferredWorkcenterId { get; set; }
    public Workcenter? PreferredWorkcenter { get; set; }

    public bool IsExternalWork { get; set; }
    public Guid? ServiceReferenceId { get; set; }
    public Reference? ServiceReference { get; set; }
    public decimal ExternalWorkCost { get; set; }
    public decimal TransportCost { get; set; }

    public string Comment { get; set; } = string.Empty;
    public DateTime? StartTime { get; set; }
    public DateTime? EndTime { get; set; }

    public decimal QuantityOk { get; set; } = decimal.Zero;
    public decimal QuantityKo { get; set; } = decimal.Zero;

    public Guid? PurchaseOrderId { get; set; }
    public PurchaseOrder? PurchaseOrder { get; set; }
    public ICollection<WorkOrderPhaseDetail> Details { get; set; } = [];
    public ICollection<WorkOrderPhaseBillOfMaterials> BillOfMaterials { get; set; } = [];
}
