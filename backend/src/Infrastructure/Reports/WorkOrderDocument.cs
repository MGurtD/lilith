using System.Globalization;
using Application.Contracts;
using Infrastructure.Reports.Common;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Infrastructure.Reports;

internal sealed class WorkOrderReportLabels
{
    public required string Title { get; init; }
    public required string Year { get; init; }
    public required string Quantity { get; init; }
    public required string WorkOrder { get; init; }
    public required string External { get; init; }
    public required string Reference { get; init; }
    public required string Date { get; init; }
    public required string Operator { get; init; }
    public required string MachinePhase { get; init; }
    public required string MachineHours { get; init; }
    public required string OperatorHours { get; init; }
    public required string MachineTime { get; init; }
    public required string OperatorTime { get; init; }
    public required string GoodQuantity { get; init; }
    public required string DefectiveQuantity { get; init; }
    public required string Observations { get; init; }
    public required string Materials { get; init; }
    public required string Phases { get; init; }
    public required string Phase { get; init; }
    public required string Code { get; init; }
    public required string Description { get; init; }
    public required string WorkcenterType { get; init; }
    public required string Workcenter { get; init; }
    public required string OperatorType { get; init; }
    public required string Width { get; init; }
    public required string Length { get; init; }
    public required string Thickness { get; init; }
    public required string Diameter { get; init; }
    public required string Yes { get; init; }
    public required string No { get; init; }
    public required string NoMaterials { get; init; }
    public required string NoPhases { get; init; }
    public required string Page { get; init; }
}

internal sealed class WorkOrderDocument(
    WorkOrderReportResponse report,
    WorkOrderReportLabels labels) : IDocument
{
    private readonly CultureInfo culture = ReportFormatters.Culture(report.LanguageCode);

    public DocumentMetadata GetMetadata() => new()
    {
        Title = $"{labels.Title} {report.Order.Code}",
        Author = report.Enterprise.Name
    };

    public void Compose(IDocumentContainer container)
    {
        container.Page(page =>
        {
            ConfigurePage(page);
            page.Header().Element(ComposeHeader);
            page.Content().PaddingTop(6).Column(column =>
            {
                column.Item().Element(ComposeSummary);
                column.Item().PaddingTop(6).Element(ComposeProductionControlTable);
                column.Item().PaddingTop(6).Element(ComposeObservations);
            });
            page.Footer().Element(ComposeFooter);
        });

        container.Page(page =>
        {
            ConfigurePage(page);
            page.Header().Element(ComposeCompactHeader);
            page.Content().PaddingTop(8).Column(column =>
            {
                column.Item().Text(labels.Materials).FontSize(11).Bold();
                column.Item().PaddingTop(4).Element(ComposeMaterials);
                column.Item().PaddingTop(12).Text(labels.Phases).FontSize(11).Bold();
                column.Item().PaddingTop(4).Element(ComposePhases);
            });
            page.Footer().Element(ComposeFooter);
        });
    }

    private static void ConfigurePage(PageDescriptor page)
    {
        page.Size(PageSizes.A4);
        page.Margin(20);
        page.DefaultTextStyle(style => style.FontSize(8));
    }

    private void ComposeHeader(IContainer container)
    {
        container.Height(48).Row(row =>
        {
            row.ConstantItem(145).AlignMiddle().Image(ReportAssets.Logo).FitArea();
            row.RelativeItem().AlignMiddle().AlignCenter().Text(labels.Title).FontSize(16).Bold();
        });
    }

    private void ComposeCompactHeader(IContainer container)
    {
        container.BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).PaddingBottom(4).Row(row =>
        {
            row.ConstantItem(92).Height(28).AlignMiddle().Image(ReportAssets.Logo).FitArea();
            row.RelativeItem().PaddingLeft(8).AlignMiddle().Text($"{labels.Title} · {report.Order.Code}").FontSize(10).Bold();
        });
    }

    private void ComposeSummary(IContainer container)
    {
        container.Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.RelativeColumn();
                columns.RelativeColumn();
                columns.RelativeColumn();
                columns.RelativeColumn();
            });

            table.Cell().Element(cell => SummaryCell(cell, labels.Year, report.Order.PlannedDate.Year.ToString(culture)));
            table.Cell().Element(cell => SummaryCell(cell, labels.Quantity, FormatDecimal(report.Order.PlannedQuantity)));
            table.Cell().Element(cell => SummaryCell(cell, labels.WorkOrder, report.Order.Code));
            table.Cell().Element(cell => SummaryCell(cell, labels.External, report.Order.HasExternalWork ? labels.Yes : labels.No));
            table.Cell().ColumnSpan(3).Element(cell => SummaryCell(cell, labels.Reference, $"{report.Order.ReferenceCode} - {report.Order.ReferenceDescription}"));
            table.Cell().Element(cell => SummaryCell(cell, labels.Date, ReportFormatters.Date(report.Order.PlannedDate, culture)));
        });
    }

    private void ComposeProductionControlTable(IContainer container)
    {
        container.Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.RelativeColumn(1.4f);
                columns.RelativeColumn();
                columns.RelativeColumn(1.4f);
                columns.RelativeColumn(0.85f);
                columns.RelativeColumn(0.85f);
                columns.RelativeColumn(0.85f);
                columns.RelativeColumn(0.85f);
                columns.RelativeColumn(0.45f);
                columns.RelativeColumn(0.45f);
            });

            table.Header(header =>
            {
                header.Cell().Element(cell => HeaderCell(cell, labels.Operator));
                header.Cell().Element(cell => HeaderCell(cell, labels.Date));
                header.Cell().Element(cell => HeaderCell(cell, labels.MachinePhase));
                header.Cell().Element(cell => HeaderCell(cell, labels.MachineHours));
                header.Cell().Element(cell => HeaderCell(cell, labels.OperatorHours));
                header.Cell().Element(cell => HeaderCell(cell, labels.MachineTime));
                header.Cell().Element(cell => HeaderCell(cell, labels.OperatorTime));
                header.Cell().Element(cell => HeaderCell(cell, labels.GoodQuantity));
                header.Cell().Element(cell => HeaderCell(cell, labels.DefectiveQuantity));
            });

            for (var row = 0; row < 33; row++)
            {
                for (var column = 0; column < 9; column++)
                    table.Cell().Height(17.5f).Border(0.5f).BorderColor(Colors.Grey.Medium);
            }
        });
    }

    private void ComposeObservations(IContainer container)
    {
        container.Border(0.5f).BorderColor(Colors.Grey.Medium).MinHeight(62).Padding(5).Column(column =>
        {
            column.Item().Text(labels.Observations).SemiBold();
            if (!string.IsNullOrWhiteSpace(report.Order.Comment))
                column.Item().PaddingTop(3).Text(report.Order.Comment);
        });
    }

    private void ComposeMaterials(IContainer container)
    {
        if (report.BillOfMaterials.Count == 0)
        {
            container.Text(labels.NoMaterials).Italic().FontColor(Colors.Grey.Darken1);
            return;
        }

        container.Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.ConstantColumn(36);
                columns.ConstantColumn(72);
                columns.RelativeColumn();
                columns.ConstantColumn(46);
                columns.ConstantColumn(42);
                columns.ConstantColumn(42);
                columns.ConstantColumn(42);
                columns.ConstantColumn(42);
            });

            table.Header(header =>
            {
                header.Cell().Element(cell => HeaderCell(cell, labels.Phase));
                header.Cell().Element(cell => HeaderCell(cell, labels.Code));
                header.Cell().Element(cell => HeaderCell(cell, labels.Description));
                header.Cell().Element(cell => HeaderCell(cell, labels.Quantity));
                header.Cell().Element(cell => HeaderCell(cell, labels.Width));
                header.Cell().Element(cell => HeaderCell(cell, labels.Length));
                header.Cell().Element(cell => HeaderCell(cell, labels.Thickness));
                header.Cell().Element(cell => HeaderCell(cell, labels.Diameter));
            });

            foreach (var material in report.BillOfMaterials)
            {
                table.Cell().Element(cell => BodyCell(cell, material.PhaseCode));
                table.Cell().Element(cell => BodyCell(cell, material.ReferenceCode));
                table.Cell().Element(cell => BodyCell(cell, material.ReferenceDescription));
                table.Cell().Element(cell => BodyCell(cell, FormatDecimal(material.Quantity), true));
                table.Cell().Element(cell => BodyCell(cell, FormatDimension(material.Width), true));
                table.Cell().Element(cell => BodyCell(cell, FormatDimension(material.Length), true));
                table.Cell().Element(cell => BodyCell(cell, FormatDimension(material.Thickness), true));
                table.Cell().Element(cell => BodyCell(cell, FormatDimension(material.Diameter), true));
            }
        });
    }

    private void ComposePhases(IContainer container)
    {
        if (report.Phases.Count == 0)
        {
            container.Text(labels.NoPhases).Italic().FontColor(Colors.Grey.Darken1);
            return;
        }

        container.Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.ConstantColumn(38);
                columns.RelativeColumn();
                columns.ConstantColumn(92);
                columns.ConstantColumn(92);
                columns.ConstantColumn(78);
                columns.ConstantColumn(44);
            });

            table.Header(header =>
            {
                header.Cell().Element(cell => HeaderCell(cell, labels.Code));
                header.Cell().Element(cell => HeaderCell(cell, labels.Description));
                header.Cell().Element(cell => HeaderCell(cell, labels.WorkcenterType));
                header.Cell().Element(cell => HeaderCell(cell, labels.Workcenter));
                header.Cell().Element(cell => HeaderCell(cell, labels.OperatorType));
                header.Cell().Element(cell => HeaderCell(cell, labels.External));
            });

            foreach (var phase in report.Phases)
            {
                table.Cell().Element(cell => BodyCell(cell, phase.Code, bold: true));
                table.Cell().Element(cell => BodyCell(cell, phase.Description, bold: true));
                table.Cell().Element(cell => BodyCell(cell, phase.WorkcenterTypeName, bold: true));
                table.Cell().Element(cell => BodyCell(cell, phase.WorkcenterName, bold: true));
                table.Cell().Element(cell => BodyCell(cell, phase.OperatorTypeName, bold: true));
                table.Cell().Element(cell => BodyCell(cell, phase.IsExternalWork ? labels.Yes : labels.No, bold: true));

                foreach (var detail in phase.Details)
                {
                    table.Cell().Element(cell => BodyCell(cell, string.Empty));
                    var parts = new[]
                    {
                        detail.Description,
                        detail.MachineStatusName,
                        $"{labels.MachineTime}: {FormatDecimal(detail.EstimatedTime)} min",
                        $"{labels.OperatorTime}: {FormatDecimal(detail.EstimatedOperatorTime)} min"
                    }.Where(value => !string.IsNullOrWhiteSpace(value));

                    table.Cell().ColumnSpan(5).Element(cell =>
                        cell.Background(Colors.Grey.Lighten4)
                            .BorderBottom(0.5f)
                            .BorderColor(Colors.Grey.Lighten2)
                            .PaddingVertical(3)
                            .PaddingHorizontal(5)
                            .Text($"- {string.Join(" · ", parts)}")
                            .Italic());
                }
            }
        });
    }

    private void ComposeFooter(IContainer container)
    {
        container.BorderTop(0.5f).BorderColor(Colors.Grey.Lighten2).PaddingTop(3).Row(row =>
        {
            row.RelativeItem().Text($"{labels.WorkOrder}: {report.Order.Code} · NIF: {report.Site.VatNumber}");
            row.ConstantItem(75).AlignRight().Text(text =>
            {
                text.Span($"{labels.Page} ");
                text.CurrentPageNumber();
                text.Span(" / ");
                text.TotalPages();
            });
        });
    }

    private static void SummaryCell(IContainer container, string label, string value)
    {
        container.Border(0.5f).BorderColor(Colors.Grey.Medium).Padding(4).Column(column =>
        {
            column.Item().Text(label).FontSize(7).SemiBold().FontColor(Colors.Grey.Darken1);
            column.Item().Text(value);
        });
    }

    private static void HeaderCell(IContainer container, string value) =>
        container.Background(ReportTheme.TableHeader)
            .Border(0.5f)
            .BorderColor(Colors.Grey.Medium)
            .PaddingVertical(3)
            .PaddingHorizontal(3)
            .AlignCenter()
            .Text(value)
            .FontSize(6.5f)
            .SemiBold();

    private static void BodyCell(IContainer container, string value, bool rightAligned = false, bool bold = false)
    {
        var styled = container.BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).PaddingVertical(3).PaddingHorizontal(4);
        if (rightAligned)
            styled = styled.AlignRight();

        var text = styled.Text(value);
        if (bold)
            text.Bold();
    }

    private string FormatDecimal(decimal value) => value.ToString("0.##", culture);
    private string FormatDimension(decimal value) => value == 0 ? string.Empty : FormatDecimal(value);
}
