using System.Globalization;
using Application.Contracts;
using Infrastructure.Reports.Common.Components;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Infrastructure.Reports;

/// <summary>
/// Native QuestPDF rendering of the richest transactional report. It is isolated
/// alongside the existing JSON/FastReport services.
/// </summary>
public sealed class SalesInvoiceDocument(InvoiceReportDto invoice, string? qrCodePng) : IDocument
{
    private const string DeliveryNoteHeaderColor = "D9E2F3";
    private const string TableHeaderColor = "EBF0F9";
    private const string CompanyLogoResource = "Infrastructure.Reports.Assets.temges-logo.jpg";

    private static readonly Lazy<byte[]> CompanyLogo = new(LoadCompanyLogo);
    private readonly CultureInfo culture = CultureInfo.GetCultureInfo(invoice.LanguageCode);

    public DocumentMetadata GetMetadata() => new()
    {
        Title = $"{invoice.Title} {invoice.Number}",
        Author = "Rawcraft Zenith",
        Subject = "Rawcraft Zenith Sales Invoice Report"
    };

    public void Compose(IDocumentContainer container)
    {
        container.Page(page =>
        {
            page.Size(PageSizes.A4);
            page.MarginHorizontal(28);
            page.MarginVertical(28);
            page.DefaultTextStyle(style => style.FontSize(9));
            page.Foreground().Element(ReportWatermark.Compose);
            page.Header().Element(ComposeHeader);
            page.Content().PaddingTop(4).Column(ComposeContent);
            page.Footer().Element(ComposeFooter);
        });
    }

    private void ComposeHeader(IContainer container)
    {
        container.Column(column =>
        {
            column.Item().ShowOnce().Element(ComposeFirstPageHeader);
            column.Item().SkipOnce().Element(ComposeCompactHeader);
        });
    }

    private void ComposeFirstPageHeader(IContainer container)
    {
        container.Column(column =>
        {
            column.Item().Height(92).Row(row =>
            {
                row.ConstantItem(145).PaddingTop(16).Height(42).AlignCenter().Image(CompanyLogo.Value).FitArea();
                row.RelativeItem().Row(qrRow =>
                {
                    qrRow.RelativeItem();
                    qrRow.ConstantItem(95).Column(qrColumn =>
                    {
                        if (string.IsNullOrWhiteSpace(qrCodePng))
                            return;

                        qrColumn.Item().Width(30, Unit.Millimetre).Height(30, Unit.Millimetre)
                            .Image(DecodeDataUri(qrCodePng));
                    });

                    qrRow.ConstantItem(70).Column(labelColumn =>
                    {
                        if (!string.IsNullOrWhiteSpace(qrCodePng))
                            labelColumn.Item().PaddingTop(4).Text("VERI*FACTU").FontSize(7).Bold();
                    });
                });
                row.ConstantItem(195).Column(metadata =>
                {
                    metadata.Item().PaddingLeft(50).PaddingTop(4).Text(invoice.Title).FontSize(14).Bold();
                    metadata.Item().PaddingTop(14).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                        });

                        table.Header(header =>
                        {
                            header.Cell().Element(cell => MetadataCell(cell, invoice.HeaderNumber));
                            header.Cell().Element(cell => MetadataCell(cell, invoice.HeaderDate));
                        });

                        table.Cell().Element(cell => MetadataValueCell(cell, invoice.Number));
                        table.Cell().Element(cell => MetadataValueCell(cell, FormatDate(invoice.Date)));
                    });
                });
            });

            column.Item().PaddingTop(2).Row(row =>
            {
                row.RelativeItem().Element(ComposeIssuer);
                row.ConstantItem(230).Element(ComposeCustomer);
            });
        });
    }

    private void ComposeCompactHeader(IContainer container)
    {
        container.BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).PaddingBottom(4).Row(row =>
        {
            row.ConstantItem(85).Height(28).AlignCenter().Image(CompanyLogo.Value).FitArea();
            row.RelativeItem().PaddingLeft(8).Column(details =>
            {
                details.Item().Text($"{invoice.Title}: {invoice.Number} - {invoice.HeaderDate}: {FormatDate(invoice.Date)}").Bold();
                details.Item().Text($"NIF: {invoice.Site.VatNumber} - {invoice.Customer.TaxName}");
            });
            row.ConstantItem(82).AlignRight().Text(text =>
            {
                text.Span("Página ");
                text.CurrentPageNumber();
                text.Span(" de ");
                text.TotalPages();
            });
        });
    }
    private void ComposeContent(ColumnDescriptor column)
    {
        column.Item().ShowOnce().Height(10);

        foreach (var deliveryNote in invoice.DeliveryNotes)
        {
            column.Item().Element(container => ComposeDeliveryNoteTable(container, deliveryNote));
            column.Item().PaddingBottom(11);
        }

        var fiscalClosureHeight = 100 + Math.Max(0, invoice.Imports.Count - 1) * 16;
        column.Item().PaddingTop(12).EnsureSpace(fiscalClosureHeight).ShowEntire().Element(ComposeFiscalClosure);
    }
    private void ComposeIssuer(IContainer container)
    {
        container.Column(column =>
        {
            column.Item().Text(invoice.EnterpriseName).FontSize(11).SemiBold();
            column.Item().Text(invoice.Site.Address).FontSize(11);
            column.Item().Text(FormatLocality(invoice.Site.PostalCode, invoice.Site.City, invoice.Site.Region)).FontSize(11);
            column.Item().Text($"NIF: {invoice.Site.VatNumber}").FontSize(11);
            if (!string.IsNullOrWhiteSpace(invoice.Site.PhoneNumber))
                column.Item().Text($"Tel: {invoice.Site.PhoneNumber}").FontSize(11);
            var email = string.IsNullOrWhiteSpace(invoice.Site.EmailSales) ? invoice.Site.Email : invoice.Site.EmailSales;
            if (!string.IsNullOrWhiteSpace(email))
                column.Item().Text($"Email: {email}").FontSize(11);
        });
    }

    private void ComposeCustomer(IContainer container)
    {
        var address = invoice.Customer.MainAddress();

        container.Column(column =>
        {
            column.Item().Text(invoice.Customer.TaxName).FontSize(11).SemiBold();
            if (!string.IsNullOrWhiteSpace(invoice.Customer.ComercialName))
                column.Item().Text(invoice.Customer.ComercialName).FontSize(11);
            if (address is not null)
            {
                column.Item().Text(address.Address).FontSize(11);
                column.Item().Text(FormatLocality(address.PostalCode, address.City, address.Region)).FontSize(11);
            }
            column.Item().Text($"NIF: {invoice.Customer.VatNumber}").FontSize(11);
        });
    }

    private void ComposeDeliveryNoteTable(IContainer container, InvoiceReportDtoDeliveryNote deliveryNote)
    {
        container.Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.ConstantColumn(64);
                columns.RelativeColumn();
                columns.ConstantColumn(80);
                columns.ConstantColumn(72);
            });

            table.Header(header =>
            {
                header.Cell().ColumnSpan(4).Background(DeliveryNoteHeaderColor).PaddingVertical(4).PaddingHorizontal(6)
                    .Text(deliveryNote.Header).FontSize(11).SemiBold();
                header.Cell().Element(cell => HeaderCell(cell, invoice.TableQuantity));
                header.Cell().Element(cell => HeaderCell(cell, invoice.TableConcept));
                header.Cell().Element(cell => HeaderCell(cell, invoice.TableUnitPrice));
                header.Cell().Element(cell => HeaderCell(cell, invoice.TableImport));
            });

            foreach (var detail in deliveryNote.Details)
            {
                table.Cell().Element(cell => BodyCell(cell, detail.Quantity.ToString("N0", culture), true));
                table.Cell().Element(cell => BodyCell(cell, detail.Description));
                table.Cell().Element(cell => BodyCell(cell, FormatCurrency(detail.UnitPrice), true));
                table.Cell().Element(cell => BodyCell(cell, FormatCurrency(detail.Amount), true));
            }

            table.Cell().ColumnSpan(3).AlignRight().PaddingTop(4).Text(invoice.TableTotalDeliveryNote).SemiBold();
            table.Cell().AlignRight().PaddingTop(4).Text(FormatCurrency(deliveryNote.Total)).SemiBold();
        });
    }
    private void ComposeFiscalClosure(IContainer container)
    {
        container.Column(column =>
        {
            column.Item().Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(1.45f);
                    columns.RelativeColumn();
                    columns.RelativeColumn(1.45f);
                    columns.RelativeColumn(1.45f);
                });

                table.Header(header =>
                {
                    header.Cell().Element(cell => HeaderCell(cell, invoice.FooterTableTaxBase));
                    header.Cell().Element(cell => HeaderCell(cell, invoice.FooterTableVat));
                    header.Cell().Element(cell => HeaderCell(cell, invoice.FooterTableVatAmount));
                    header.Cell().Element(cell => HeaderCell(cell, invoice.FooterTableTotal));
                });

                foreach (var tax in invoice.Imports)
                {
                    table.Cell().Element(cell => BodyCell(cell, FormatCurrency(tax.BaseAmount), true));
                    table.Cell().Element(cell => BodyCell(cell, $"{tax.Percentatge:N2}%", true));
                    table.Cell().Element(cell => BodyCell(cell, FormatCurrency(tax.TaxAmount), true));
                    table.Cell().Element(cell => BodyCell(cell, FormatCurrency(tax.NetAmount), true));
                }

                table.Cell().ColumnSpan(3).AlignRight().PaddingTop(3).Text(invoice.FooterTableInvoiceTotal).Bold();
                table.Cell().AlignRight().PaddingTop(3).Text(FormatCurrency(invoice.Total)).Bold();
            });

            column.Item().PaddingTop(4).Text(text =>
            {
                text.Span($"{invoice.FooterPaymentMethod}: ").SemiBold();
                text.Span(invoice.PaymentMethod.Name);
            });

            column.Item().PaddingTop(1).Text(text =>
            {
                text.Span("IBAN: ").SemiBold();
                text.Span(invoice.Iban);
            });

            column.Item().PaddingTop(1).Text(text =>
            {
                text.Span($"{invoice.FooterDueDate}: ").SemiBold();
                text.Span(FormatDate(invoice.DueDate));
            });
        });
    }

    private void ComposeFooter(IContainer container)
    {
        container.BorderTop(0.5f).BorderColor(Colors.Grey.Lighten2).PaddingTop(3).Row(row =>
        {
            row.RelativeItem().Text($"{invoice.HeaderNumber}: {invoice.Number} - NIF: {invoice.Site.VatNumber}");
            row.ConstantItem(82).AlignRight().Text(text =>
            {
                text.Span("Página ");
                text.CurrentPageNumber();
                text.Span(" de ");
                text.TotalPages();
            });
        });
    }
    private static void HeaderCell(IContainer container, string value) =>
        container.Background(TableHeaderColor).BorderBottom(0.5f).BorderColor(Colors.Grey.Medium).PaddingVertical(3).PaddingHorizontal(4).Text(value).SemiBold();

    private static void MetadataCell(IContainer container, string value) =>
        container.Background(DeliveryNoteHeaderColor).Border(0.5f).BorderColor(Colors.Grey.Medium).Padding(4).Text(value).SemiBold();

    private static void MetadataValueCell(IContainer container, string value) =>
        container.Border(0.5f).BorderColor(Colors.Grey.Medium).Padding(4).AlignCenter().Text(value);

    private static void BodyCell(IContainer container, string value, bool rightAligned = false)
    {
        container = container.BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).PaddingVertical(3).PaddingHorizontal(4);
        if (rightAligned) container = container.AlignRight();
        container.Text(value);
    }

    private string FormatDate(DateTime date) => date.ToString("d", culture);

    private string FormatAmount(decimal amount) => amount.ToString("N2", culture);

    private string FormatCurrency(decimal amount) => $"{FormatAmount(amount)} €";

    private static string FormatLocality(string postalCode, string city, string region)
    {
        var locality = string.Join(" – ", (new[] { postalCode, city }).Where(value => !string.IsNullOrWhiteSpace(value)));
        return string.IsNullOrWhiteSpace(region) ? locality : $"{locality} ({region})";
    }

    private static byte[] LoadCompanyLogo()
    {
        var assembly = typeof(SalesInvoiceDocument).Assembly;
        var resourceName = assembly.GetManifestResourceNames()
            .SingleOrDefault(name => name.EndsWith(".temges-logo.jpg", StringComparison.OrdinalIgnoreCase))
            ?? throw new InvalidOperationException($"Embedded report resource not found: {CompanyLogoResource}");
        using var stream = assembly.GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException($"Embedded report resource cannot be read: {resourceName}");
        using var buffer = new MemoryStream();
        stream.CopyTo(buffer);
        return buffer.ToArray();
    }

    private static byte[] DecodeDataUri(string dataUri)
    {
        var commaIndex = dataUri.IndexOf(',');
        var base64 = commaIndex >= 0 ? dataUri[(commaIndex + 1)..] : dataUri;
        return Convert.FromBase64String(base64);
    }
}