using Application.Contracts;
using Infrastructure.Reports.Common;
using Infrastructure.Reports.Common.Components;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace Infrastructure.Reports;

public sealed class SalesOrderDocument(SalesOrderReportResponse report) : StandardReportDocument(CreateHeader(report), report.Site?.VatNumber ?? string.Empty)
{
    private static ReportHeaderData CreateHeader(SalesOrderReportResponse report)
    {
        var order = report.Order ?? throw new ArgumentException("Sales order report is incomplete.");
        return new(report.Title, order.Number, order.Date, report.HeaderNumber, report.HeaderDate, ReportPartyFactory.Site(report.Site!, report.Enterprise!), ReportPartyFactory.Customer(report.Customer!), report.LanguageCode);
    }

    protected override void ComposeContent(ColumnDescriptor column)
    {
        var order = report.Order!;
        if (!string.IsNullOrWhiteSpace(order.CustomerNumber)) column.Item().Text($"{report.HeaderCustomerOrder}: {order.CustomerNumber}").SemiBold();
        column.Item().PaddingTop(6).Table(table =>
        {
            table.ColumnsDefinition(c => { c.ConstantColumn(60); c.RelativeColumn(); if (report.ShowPrices) { c.ConstantColumn(75); c.ConstantColumn(75); } });
            table.Header(h => { h.Cell().Element(c => ReportTable.HeaderCell(c, report.TableQuantity)); h.Cell().Element(c => ReportTable.HeaderCell(c, report.TableConcept)); if (report.ShowPrices) { h.Cell().Element(c => ReportTable.HeaderCell(c, report.TableUnitPrice)); h.Cell().Element(c => ReportTable.HeaderCell(c, report.TableImport)); } });
            foreach (var detail in report.OrderDetails) { table.Cell().Element(c => ReportTable.BodyCell(c, ReportFormatters.Quantity(detail.Quantity, Culture), true)); table.Cell().Element(c => ReportTable.BodyCell(c, detail.Description)); if (report.ShowPrices) { table.Cell().Element(c => ReportTable.BodyCell(c, ReportFormatters.Currency(detail.UnitPrice, Culture), true)); table.Cell().Element(c => ReportTable.BodyCell(c, ReportFormatters.Currency(detail.Amount, Culture), true)); } }
            if (report.ShowPrices) { table.Cell().ColumnSpan(3).AlignRight().PaddingTop(3).Text(report.TableTotal).Bold(); table.Cell().AlignRight().PaddingTop(3).Text(ReportFormatters.Currency(report.Total, Culture)).Bold(); }
        });
    }
}