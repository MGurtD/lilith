using Domain.Entities;
using Domain.Entities.Production;
using Domain.Entities.Sales;

namespace Application.Contracts;

public class InvoiceReportDto(string languageCode, ILocalizationService localizationService) : ReportResponse(languageCode)
{
    public string Title { get; set; } = localizationService.GetLocalizedStringForCulture("Report.SalesInvoice.Title", languageCode);
    public string HeaderNumber { get; set; } = localizationService.GetLocalizedStringForCulture("Report.HeaderNumber", languageCode);
    public string HeaderDate { get; set; } = localizationService.GetLocalizedStringForCulture("Report.HeaderDate", languageCode);
    public string TableQuantity { get; set; } = localizationService.GetLocalizedStringForCulture("Report.TableQuantity", languageCode);
    public string TableConcept { get; set; } = localizationService.GetLocalizedStringForCulture("Report.TableConcept", languageCode);
    public string TableUnitPrice { get; set; } = localizationService.GetLocalizedStringForCulture("Report.TableUnitPrice", languageCode);
    public string TableImport { get; set; } = localizationService.GetLocalizedStringForCulture("Report.TableAmount", languageCode);
    public string TableTotalDeliveryNote { get; set; } = localizationService.GetLocalizedStringForCulture("Report.SalesInvoice.DeliveryNoteTotal", languageCode);
    public string FooterTableTaxBase { get; set; } = localizationService.GetLocalizedStringForCulture("Report.SalesInvoice.TaxBase", languageCode);
    public string FooterTableVat { get; set; } = localizationService.GetLocalizedStringForCulture("Report.SalesInvoice.VAT", languageCode);
    public string FooterTableVatAmount { get; set; } = localizationService.GetLocalizedStringForCulture("Report.SalesInvoice.VATAmount", languageCode);
    public string FooterTableTotal { get; set; } = localizationService.GetLocalizedStringForCulture("Report.SalesInvoice.Total", languageCode);
    public string FooterTableInvoiceTotal { get; set; } = localizationService.GetLocalizedStringForCulture("Report.SalesInvoice.InvoiceTotal", languageCode);
    public string FooterPaymentMethod { get; set; } = localizationService.GetLocalizedStringForCulture("Report.SalesInvoice.PaymentMethod", languageCode);
    public string FooterDueDate { get; set; } = localizationService.GetLocalizedStringForCulture("Report.SalesInvoice.DueDate", languageCode);

    public required string Number { get; set; }
    public required DateTime Date { get; set; }
    public required DateTime DueDate { get; set; }
    public required decimal Total { get; set; }
    public required Customer Customer { get; set; }
    public required Site Site { get; set; }
    public required PaymentMethod PaymentMethod { get; set; }
    public required string QrCodeUrl { get; set; }
    public required string QrCodeReportTag { get; set; }
    public List<InvoiceReportDtoDeliveryNote> DeliveryNotes { get; set; } = [];
    public List<InvoiceReportDtoTaxImport> Imports { get; set; } = [];
}

public class InvoiceReportDtoTaxImport
{
    public required string Name { get; set; }
    public required decimal Percentatge { get; set; }
    public required decimal BaseAmount { get; set; }
    public required decimal NetAmount { get; set; }
    public required decimal TaxAmount { get; set; }
}

public class InvoiceReportDtoDeliveryNote
{
    public required string Number { get; set; }
    public required DateTime Date { get; set; }
    public required string Header { get; set; }
    public required decimal Total { get; set; }
    public required List<SalesInvoiceDetail> Details { get; set; }
}
