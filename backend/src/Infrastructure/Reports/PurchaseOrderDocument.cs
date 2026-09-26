using Application.Contracts;
using Infrastructure.Reports.Common;
using Infrastructure.Reports.Common.Components;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace Infrastructure.Reports;

public sealed class PurchaseOrderDocument(PurchaseOrderReportResponse report) : StandardReportDocument(CreateHeader(report), report.Site.VatNumber)
{
    private static ReportHeaderData CreateHeader(PurchaseOrderReportResponse report) => new("PEDIDO DE COMPRA", report.Order.Number, report.Order.Date, "Número", "Fecha", ReportPartyFactory.Supplier(report.Supplier), ReportPartyFactory.Site(report.Site, report.Enterprise), report.LanguageCode);
    protected override void ComposeContent(ColumnDescriptor column)
    {
        column.Item().Table(table =>
        {
            table.ColumnsDefinition(c => { c.ConstantColumn(80); c.RelativeColumn(); });
            table.Header(h => { h.Cell().Element(c => ReportTable.HeaderCell(c, "Cantidad")); h.Cell().Element(c => ReportTable.HeaderCell(c, "Descripción")); });
            foreach (var detail in report.Details) { table.Cell().Element(c => ReportTable.BodyCell(c, ReportFormatters.Quantity(detail.Quantity, Culture), true)); table.Cell().Element(c => ReportTable.BodyCell(c, detail.Description)); }
        });
    }
}