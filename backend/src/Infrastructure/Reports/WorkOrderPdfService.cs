using Application.Contracts;
using QuestPDF.Fluent;

namespace Infrastructure.Reports;

public sealed class WorkOrderPdfService(ILocalizationService localizationService) : IWorkOrderPdfService
{
    public byte[] Generate(WorkOrderReportResponse report)
    {
        string Localize(string key) => localizationService.GetLocalizedStringForCulture(key, report.LanguageCode);

        var labels = new WorkOrderReportLabels
        {
            Title = Localize("Report.WorkOrder.Title"),
            Year = Localize("Report.WorkOrder.Year"),
            Quantity = Localize("Report.TableQuantity"),
            WorkOrder = Localize("Report.WorkOrder.Number"),
            External = Localize("Report.WorkOrder.External"),
            Reference = Localize("Report.TableReference"),
            Date = Localize("Report.HeaderDate"),
            Operator = Localize("Report.WorkOrder.Operator"),
            MachinePhase = Localize("Report.WorkOrder.MachinePhase"),
            MachineHours = Localize("Report.WorkOrder.MachineHours"),
            OperatorHours = Localize("Report.WorkOrder.OperatorHours"),
            MachineTime = Localize("Report.WorkOrder.MachineTime"),
            OperatorTime = Localize("Report.WorkOrder.OperatorTime"),
            GoodQuantity = Localize("Report.WorkOrder.GoodQuantity"),
            DefectiveQuantity = Localize("Report.WorkOrder.DefectiveQuantity"),
            Observations = Localize("Report.WorkOrder.Observations"),
            Materials = Localize("Report.WorkOrder.Materials"),
            Phases = Localize("Report.WorkOrder.Phases"),
            Phase = Localize("Report.WorkOrder.Phase"),
            Code = Localize("Report.WorkOrder.Code"),
            Description = Localize("Report.TableDescription"),
            WorkcenterType = Localize("Report.WorkOrder.WorkcenterType"),
            Workcenter = Localize("Report.WorkOrder.Workcenter"),
            OperatorType = Localize("Report.WorkOrder.OperatorType"),
            Width = Localize("Report.WorkOrder.Width"),
            Length = Localize("Report.WorkOrder.Length"),
            Thickness = Localize("Report.WorkOrder.Thickness"),
            Diameter = Localize("Report.WorkOrder.Diameter"),
            Yes = Localize("Report.WorkOrder.Yes"),
            No = Localize("Report.WorkOrder.No"),
            NoMaterials = Localize("Report.WorkOrder.NoMaterials"),
            NoPhases = Localize("Report.WorkOrder.NoPhases"),
            Page = Localize("Report.WorkOrder.Page")
        };

        return new WorkOrderDocument(report, labels).GeneratePdf();
    }
}
