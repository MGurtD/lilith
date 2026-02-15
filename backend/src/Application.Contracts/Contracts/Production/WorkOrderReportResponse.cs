
namespace Application.Contracts;

public class WorkOrderReportResponse : ReportResponse
{
    public required WorkOrderReportDto Order { get; set; }
    public required List<WorkOrderPhaseReportDto> Phases { get; set; }
    public required List<WorkOrderPhaseBillOfMaterialsReportDto> BillOfMaterials { get; set; }

    public WorkOrderReportResponse() : base() { }
}

public class WorkOrderReportDto
{
    public required string Code { get; set; }
    public required string ReferenceCode { get; set; }
    public required string ReferenceDescription { get; set; }
    public required DateTime PlannedDate { get; set; }
    public required decimal PlannedQuantity { get; set; }
    public required string StatusName { get; set; }
    public required bool HasExternalWork { get; set; }
    public required string Comment { get; set; }
}

public class WorkOrderPhaseReportDto
{
    public required string Code { get; set; }
    public required string Description { get; set; }
    public required string WorkcenterTypeName { get; set; }
    public required string OperatorTypeName { get; set; }
    public required string WorkcenterName { get; set; }
    public required bool IsExternalWork { get; set; }
    public required List<WorkOrderPhaseDetailReportDto> Details { get; set; }
}

public class WorkOrderPhaseDetailReportDto
{
    public required string Description { get; set; }    
    public required string MachineStatusName { get; set; }
    public required decimal EstimatedTime { get; set; }
    public required decimal EstimatedOperatorTime { get; set; }
}

public class WorkOrderPhaseBillOfMaterialsReportDto
{
    public required string ReferenceCode { get; set; }
    public required string ReferenceDescription { get; set; }
    public required decimal Quantity { get; set; }
    public required decimal Width { get; set; }
    public required decimal Length { get; set; }
    public required decimal Thickness { get; set; }
    public required decimal Diameter { get; set; }
}
