using Application.Contracts;
using Infrastructure.Reports.Common;
using Infrastructure.Reports.Common.Components;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Infrastructure.Reports;

public sealed class DeliveryNoteDocument(DeliveryNoteReportResponse report)
    : StandardReportDocument(CreateHeader(report), report.Site?.VatNumber ?? string.Empty)
{
    private static ReportHeaderData CreateHeader(DeliveryNoteReportResponse report)
    {
        var note = report.DeliveryNote ?? throw new ArgumentException("Delivery note report is incomplete.");
        return new(
            report.Title,
            note.Number,
            note.DeliveryDate ?? DateTime.Today,
            report.HeaderNumber,
            report.HeaderDate,
            ReportPartyFactory.Site(report.Site!, report.Enterprise!),
            ReportPartyFactory.Customer(report.Customer!),
            report.LanguageCode);
    }

    protected override void ComposeContent(ColumnDescriptor column)
    {
        column.Item().PaddingBottom(5).Text(report.ReturnsPolicy).FontSize(9).SemiBold();

        foreach (var order in report.Orders ?? [])
            column.Item().PaddingTop(7).Element(container => ComposeOrderTable(container, order));

        if (report.ShowPrices)
        {
            column.Item().PaddingTop(8).AlignRight()
                .Text($"{report.TableTotal}: {ReportFormatters.Currency(report.Total, Culture)}").Bold();
        }

        column.Item().PaddingTop(18).EnsureSpace(90).ShowEntire().Element(ComposeSignature);
    }

    private void ComposeOrderTable(IContainer container, DeliveryNoteOrderReportDto order)
    {
        container.Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.ConstantColumn(60);
                columns.RelativeColumn();
                if (report.ShowPrices)
                {
                    columns.ConstantColumn(75);
                    columns.ConstantColumn(75);
                }
            });

            table.Header(header =>
            {
                header.Cell().ColumnSpan(report.ShowPrices ? 4u : 2u)
                    .Background(ReportTheme.Accent)
                    .Padding(4)
                    .Text($"{report.TableCustomerOrder}: {order.CustomerNumber} ({order.Number})")
                    .SemiBold();
                header.Cell().Element(cell => ReportTable.HeaderCell(cell, report.TableQuantity));
                header.Cell().Element(cell => ReportTable.HeaderCell(cell, report.TableConcept));
                if (report.ShowPrices)
                {
                    header.Cell().Element(cell => ReportTable.HeaderCell(cell, report.TableUnitPrice));
                    header.Cell().Element(cell => ReportTable.HeaderCell(cell, report.TableImport));
                }
            });

            foreach (var detail in order.Details)
            {
                table.Cell().Element(cell => ReportTable.BodyCell(cell, ReportFormatters.Quantity(detail.Quantity, Culture), true));
                table.Cell().Element(cell => ReportTable.BodyCell(cell, $"{detail.Reference?.Code} - {detail.Description}"));
                if (report.ShowPrices)
                {
                    table.Cell().Element(cell => ReportTable.BodyCell(cell, ReportFormatters.Currency(detail.UnitPrice, Culture), true));
                    table.Cell().Element(cell => ReportTable.BodyCell(cell, ReportFormatters.Currency(detail.Amount, Culture), true));
                }
            }

            if (report.ShowPrices)
            {
                table.Cell().ColumnSpan(3).AlignRight().PaddingTop(3).Text(report.TableOrderTotal).Bold();
                table.Cell().AlignRight().PaddingTop(3).Text(ReportFormatters.Currency(order.Total, Culture)).Bold();
            }
        });
    }

    private void ComposeSignature(IContainer container)
    {
        container.Border(0.75f).BorderColor(Colors.Grey.Lighten1).Padding(9).Column(column =>
        {
            column.Item().Text(report.FooterSignature).FontSize(10).SemiBold();
            column.Item().PaddingTop(3).Text(report.FooterSignatureHint).FontSize(8).FontColor(Colors.Grey.Darken1);
            column.Item().PaddingTop(32).BorderBottom(0.75f).BorderColor(Colors.Grey.Medium);
        });
    }
}