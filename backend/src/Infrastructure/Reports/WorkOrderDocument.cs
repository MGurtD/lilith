using Application.Contracts;
using Infrastructure.Reports.Common;
using Infrastructure.Reports.Common.Components;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace Infrastructure.Reports;

public sealed class WorkOrderDocument(WorkOrderReportResponse report)
    : StandardReportDocument(CreateHeader(report), report.Site.VatNumber)
{
    private static ReportHeaderData CreateHeader(WorkOrderReportResponse report) => new(
        "ORDRE DE TREBALL",
        report.Order.Code,
        report.Order.PlannedDate,
        "Codi",
        "Data",
        ReportPartyFactory.Site(report.Site, report.Enterprise),
        new ReportParty(
            report.Order.ReferenceCode,
            [report.Order.ReferenceDescription, report.Order.StatusName, $"Quantitat: {report.Order.PlannedQuantity:N0}", report.Order.Comment],
            string.Empty),
        report.LanguageCode);

    protected override void ComposeContent(ColumnDescriptor column)
    {
        column.Item().Text("Fases").FontSize(11).SemiBold();
        column.Item().PaddingTop(4).Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.ConstantColumn(45);
                columns.RelativeColumn();
                columns.RelativeColumn();
                columns.RelativeColumn();
            });
            table.Header(header =>
            {
                header.Cell().Element(cell => ReportTable.HeaderCell(cell, "Codi"));
                header.Cell().Element(cell => ReportTable.HeaderCell(cell, "Descripci\u00f3"));
                header.Cell().Element(cell => ReportTable.HeaderCell(cell, "Centre de treball"));
                header.Cell().Element(cell => ReportTable.HeaderCell(cell, "Operari"));
            });
            foreach (var phase in report.Phases)
            {
                table.Cell().Element(cell => ReportTable.BodyCell(cell, phase.Code));
                table.Cell().Element(cell => ReportTable.BodyCell(cell, phase.Description));
                table.Cell().Element(cell => ReportTable.BodyCell(cell, phase.WorkcenterName));
                table.Cell().Element(cell => ReportTable.BodyCell(cell, phase.OperatorTypeName));
                foreach (var detail in phase.Details)
                    table.Cell().ColumnSpan(4).PaddingLeft(12).Element(cell => ReportTable.BodyCell(cell, detail.Description));
            }
        });

        if (report.BillOfMaterials.Count == 0)
            return;

        column.Item().PaddingTop(12).Text("Materials").FontSize(11).SemiBold();
        column.Item().PaddingTop(4).Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.ConstantColumn(85);
                columns.RelativeColumn();
                columns.ConstantColumn(60);
            });
            table.Header(header =>
            {
                header.Cell().Element(cell => ReportTable.HeaderCell(cell, "Refer\u00e8ncia"));
                header.Cell().Element(cell => ReportTable.HeaderCell(cell, "Descripci\u00f3"));
                header.Cell().Element(cell => ReportTable.HeaderCell(cell, "Quantitat"));
            });
            foreach (var bom in report.BillOfMaterials)
            {
                table.Cell().Element(cell => ReportTable.BodyCell(cell, bom.ReferenceCode));
                table.Cell().Element(cell => ReportTable.BodyCell(cell, bom.ReferenceDescription));
                table.Cell().Element(cell => ReportTable.BodyCell(cell, ReportFormatters.Quantity(bom.Quantity, Culture), true));
            }
        });
    }
}