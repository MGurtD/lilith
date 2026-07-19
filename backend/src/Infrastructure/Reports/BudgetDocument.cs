using Application.Contracts;
using Infrastructure.Reports.Common;
using Infrastructure.Reports.Common.Components;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace Infrastructure.Reports;

public sealed class BudgetDocument(BudgetReportResponse report) : StandardReportDocument(CreateHeader(report), report.Site?.VatNumber ?? string.Empty)
{
    private static ReportHeaderData CreateHeader(BudgetReportResponse report)
    {
        var budget = report.Budget ?? throw new ArgumentException("Budget report is incomplete.");
        var customer = report.Customer ?? throw new ArgumentException("Budget customer is missing.");
        var site = report.Site ?? throw new ArgumentException("Budget site is missing.");
        return new(report.Title, budget.Number, budget.Date, report.HeaderNumber, report.HeaderDate, ReportPartyFactory.Site(site, report.Enterprise!), ReportPartyFactory.Customer(customer), report.LanguageCode);
    }

    protected override void ComposeContent(ColumnDescriptor column)
    {
        var budget = report.Budget!;
        column.Item().Text($"{report.HeaderDeliveryIn}: {budget.DeliveryDays} {report.HeaderDeliveryConfirmation}").SemiBold();
        column.Item().PaddingTop(6).Table(table =>
        {
            table.ColumnsDefinition(c => { c.ConstantColumn(60); c.RelativeColumn(); c.ConstantColumn(75); c.ConstantColumn(75); });
            table.Header(h => { h.Cell().Element(c => ReportTable.HeaderCell(c, report.TableQuantity)); h.Cell().Element(c => ReportTable.HeaderCell(c, report.TableConcept)); h.Cell().Element(c => ReportTable.HeaderCell(c, report.TableUnitPrice)); h.Cell().Element(c => ReportTable.HeaderCell(c, report.TableAmount)); });
            foreach (var detail in budget.Details)
            {
                table.Cell().Element(c => ReportTable.BodyCell(c, ReportFormatters.Quantity(detail.Quantity, Culture), true));
                table.Cell().Element(c => ReportTable.BodyCell(c, $"{detail.Reference?.Code} - {detail.Description}"));
                table.Cell().Element(c => ReportTable.BodyCell(c, ReportFormatters.Currency(detail.UnitPrice, Culture), true));
                table.Cell().Element(c => ReportTable.BodyCell(c, ReportFormatters.Currency(detail.Amount, Culture), true));
            }
            table.Cell().ColumnSpan(3).AlignRight().PaddingTop(3).Text(report.TableTotal).Bold();
            table.Cell().AlignRight().PaddingTop(3).Text(ReportFormatters.Currency(report.Total, Culture)).Bold();
        });
        if (!string.IsNullOrWhiteSpace(budget.Notes))
            column.Item().PaddingTop(12).EnsureSpace(20).Text(report.FooterValidation).SemiBold();
            column.Item().Text(budget.Notes);
    }
}