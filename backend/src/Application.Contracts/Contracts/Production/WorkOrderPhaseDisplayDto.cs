namespace Application.Contracts.Contracts.Production;

/// <summary>
/// DTO for displaying work order phases with priority sorting.
/// Used for manufacturing order visualization and planning.
/// </summary>
public class WorkOrderPhaseDisplayDto
{
    /// <summary>Work Order ID</summary>
    public Guid WorkOrderId { get; set; }
    
    /// <summary>Work Order Code (OF Code)</summary>
    public string WorkOrderCode { get; set; } = string.Empty;
    
    /// <summary>Customer commercial name</summary>
    public string CustomerName { get; set; } = string.Empty;
    
    /// <summary>Work Order Phase ID</summary>
    public Guid PhaseId { get; set; }
    
    /// <summary>Phase code</summary>
    public string PhaseCode { get; set; } = string.Empty;
    
    /// <summary>Phase description</summary>
    public string PhaseDescription { get; set; } = string.Empty;
    
    /// <summary>Phase code + description (formatted for display)</summary>
    public string PhaseDisplay { get; set; } = string.Empty;
    
    /// <summary>Sales reference code</summary>
    public string SalesReferenceCode { get; set; } = string.Empty;
    
    /// <summary>Sales reference description</summary>
    public string SalesReferenceDescription { get; set; } = string.Empty;
    
    /// <summary>Sales reference code + description (formatted for display)</summary>
    public string SalesReferenceDisplay { get; set; } = string.Empty;
    
    /// <summary>Planned production quantity</summary>
    public decimal PlannedQuantity { get; set; }
    
    /// <summary>Planned production date</summary>
    public DateTime PlannedDate { get; set; }
    
    /// <summary>Current phase status name</summary>
    public string PhaseStatus { get; set; } = string.Empty;
    
    /// <summary>Machine status ID from primary phase detail</summary>
    public Guid? MachineStatusId { get; set; }
    
    /// <summary>Work order priority (Order field - lower value = higher priority)</summary>
    public int Priority { get; set; }
}

/// <summary>
/// Hierarchical DTO for displaying work orders with their planned phases.
/// Used for TreeTable visualization in plant module.
/// </summary>
public class WorkOrderWithPhasesDto
{
    /// <summary>Work Order ID</summary>
    public Guid WorkOrderId { get; set; }
    
    /// <summary>Work Order Code (OF Code)</summary>
    public string WorkOrderCode { get; set; } = string.Empty;
    
    /// <summary>Customer commercial name</summary>
    public string CustomerName { get; set; } = string.Empty;
    
    /// <summary>Sales reference ID</summary>
    public Guid SalesReferenceId { get; set; }
    
    /// <summary>Sales reference code + description (formatted for display)</summary>
    public string SalesReferenceDisplay { get; set; } = string.Empty;
    
    /// <summary>Planned production quantity</summary>
    public decimal PlannedQuantity { get; set; }
    
    /// <summary>Planned production date</summary>
    public DateTime PlannedDate { get; set; }
    
    /// <summary>Work order start time (when production actually started)</summary>
    public DateTime? StartTime { get; set; }
    
    /// <summary>Work order status name</summary>
    public string WorkOrderStatus { get; set; } = string.Empty;
    
    /// <summary>Work order priority (Order field - lower value = higher priority)</summary>
    public int Priority { get; set; }
    
    /// <summary>Work order comment</summary>
    public string Comment { get; set; } = string.Empty;
    
    /// <summary>List of planned phases for this work order</summary>
    public List<PlannedPhaseDto> Phases { get; set; } = new();
}

/// <summary>
/// DTO for individual phase within a work order.
/// Child node in TreeTable structure.
/// </summary>
public class PlannedPhaseDto
{
    /// <summary>Work Order Phase ID</summary>
    public Guid PhaseId { get; set; }
    
    /// <summary>Phase code</summary>
    public string PhaseCode { get; set; } = string.Empty;
    
    /// <summary>Phase description</summary>
    public string PhaseDescription { get; set; } = string.Empty;
    
    /// <summary>Phase code + description (formatted for display)</summary>
    public string PhaseDisplay { get; set; } = string.Empty;

    /// <summary>
    /// Phase status ID</summary>
    public Guid PhaseStatusId { get; set; }
    
    /// <summary>Current phase status name</summary>
    public string PhaseStatus { get; set; } = string.Empty;
    
    /// <summary>Phase start time</summary>
    public DateTime? StartTime { get; set; }
    
    /// <summary>Preferred workcenter name for this phase</summary>
    public string PreferredWorkcenterName { get; set; } = string.Empty;

    /// <summary>Indicates if this phase involves external work</summary>
     public bool IsExternalWork { get; set; }

     public decimal QuantityOk { get; set; }
     public decimal QuantityKo { get; set; }

    /// <summary>Phase comment</summary>
    public string Comment { get; set; } = string.Empty;

    /// <summary>Phase operation details for activity buttons in plant module</summary>
    public List<PhaseDetailForPlantDto> Details { get; set; } = [];
}

/// <summary>
/// Simplified phase detail DTO for plant module activity buttons.
/// Contains only the information needed for dynamic status buttons.
/// </summary>
public class PhaseDetailForPlantDto
{
    /// <summary>Machine status ID</summary>
    public Guid? MachineStatusId { get; set; }
    
    /// <summary>Machine status name (display text for button)</summary>
    public string MachineStatusName { get; set; } = string.Empty;
    
    /// <summary>Machine status color (hex code for button background)</summary>
    public string MachineStatusColor { get; set; } = string.Empty;
    
    /// <summary>Machine status icon (PrimeIcons class for button icon)</summary>
    public string MachineStatusIcon { get; set; } = string.Empty;
    
    /// <summary>Display order for buttons</summary>
    public int Order { get; set; }
    
    /// <summary>Estimated machine time in minutes</summary>
    public decimal EstimatedTime { get; set; }
    
    /// <summary>Estimated operator time in minutes</summary>
    public decimal EstimatedOperatorTime { get; set; }
    
    /// <summary>Detail comment</summary>
    public string Comment { get; set; } = string.Empty;
}

/// <summary>
/// Detailed phase information including work order context, phase details, and bill of materials.
/// Used for Step 2 of work order selection process in plant module.
/// </summary>
public class WorkOrderPhaseDetailedDto
{
    /// <summary>Work order ID</summary>
    public Guid WorkOrderId { get; set; }
    
    /// <summary>Work order code</summary>
    public string WorkOrderCode { get; set; } = string.Empty;
    
    /// <summary>Sales reference display (customer + reference + version)</summary>
    public string SalesReferenceDisplay { get; set; } = string.Empty;
    
    /// <summary>Planned quantity for work order</summary>
    public decimal PlannedQuantity { get; set; }
    
    /// <summary>Phase ID</summary>
    public Guid PhaseId { get; set; }
    
    /// <summary>Phase code</summary>
    public string PhaseCode { get; set; } = string.Empty;
    
    /// <summary>Phase description</summary>
    public string PhaseDescription { get; set; } = string.Empty;

    /// <summary>Phase status ID</summary>
    public Guid PhaseStatusId { get; set; }
    
    /// <summary>Current phase status name</summary>
    public string PhaseStatus { get; set; } = string.Empty;
    
    /// <summary>Phase start time</summary>
    public DateTime? StartTime { get; set; }
    
    /// <summary>Phase end time</summary>
    public DateTime? EndTime { get; set; }
    
    /// <summary>Preferred workcenter name</summary>
    public string PreferredWorkcenterName { get; set; } = string.Empty;
    
    /// <summary>Workcenter type ID (for filtering selectable phases)</summary>
    public Guid WorkcenterTypeId { get; set; }

    /// <summary>Indicates if this phase involves external work</summary>
    public bool IsExternalWork { get; set; }

    public decimal QuantityOk { get; set; }
    public decimal QuantityKo { get; set; }
    
    /// <summary>Phase comment</summary>
    public string Comment { get; set; } = string.Empty;
    
    /// <summary>Phase operation details</summary>
    public List<PhaseDetailItemDto> Details { get; set; } = [];
    
    /// <summary>Bill of materials for this phase</summary>
    public List<BillOfMaterialsItemDto> BillOfMaterials { get; set; } = [];
}

/// <summary>
/// Individual operation detail within a phase.
/// Represents a specific machine status and time estimate.
/// </summary>
public class PhaseDetailItemDto
{    
    /// <summary>Detail comment</summary>
    public string Comment { get; set; } = string.Empty;
    
    /// <summary>Machine status ID</summary>
    public Guid? MachineStatusId { get; set; }
    
    /// <summary>Machine status name</summary>
    public string MachineStatusName { get; set; } = string.Empty;
    
    /// <summary>Estimated machine time</summary>
    public decimal EstimatedTime { get; set; }
    
    /// <summary>Estimated operator time</summary>
    public decimal EstimatedOperatorTime { get; set; }

    /// <summary>Indicates if this detail represents cycle time</summary>
    public bool IsCycleTime { get; set; }
}

/// <summary>
/// Material component required for a phase.
/// References product/material catalog.
/// </summary>
public class BillOfMaterialsItemDto
{
    /// <summary>Reference code (product/material code)</summary>
    public string ReferenceCode { get; set; } = string.Empty;
    
    /// <summary>Reference description (product/material name)</summary>
    public string ReferenceDescription { get; set; } = string.Empty;
    
    /// <summary>Required quantity</summary>
    public decimal Quantity { get; set; }
}

/// <summary>
/// Simple DTO for next phase information when unloading a phase from a workcenter.
/// Used to suggest the next phase that can be loaded on the same workcenter type.
/// </summary>
public class NextPhaseInfoDto
{
    /// <summary>Phase ID</summary>
    public Guid PhaseId { get; set; }
    
    /// <summary>Phase code</summary>
    public string PhaseCode { get; set; } = string.Empty;
    
    /// <summary>Phase description</summary>
    public string PhaseDescription { get; set; } = string.Empty;
}
