using Domain.Entities.Production;
using Domain.Entities.Sales;

namespace Application.Contracts
{
    public class DeliveryNoteReportResponse(string languageCode, bool showPrices, ILocalizationService localizationService) : ReportResponse(languageCode)
    {
        public string Title { get; set; } = localizationService.GetLocalizedStringForCulture("Report.DeliveryNote.Title", languageCode);
        public string HeaderNumber { get; set; } = localizationService.GetLocalizedStringForCulture("Report.HeaderNumber", languageCode);
        public string HeaderDate { get; set; } = localizationService.GetLocalizedStringForCulture("Report.HeaderDate", languageCode);
        public string TableCustomerOrder { get; set; } = localizationService.GetLocalizedStringForCulture("Report.CustomerOrder", languageCode);
        public string TableQuantity { get; set; } = localizationService.GetLocalizedStringForCulture("Report.TableQuantity", languageCode);
        public string TableConcept { get; set; } = localizationService.GetLocalizedStringForCulture("Report.TableConcept", languageCode);
        public string TableUnitPrice { get; set; } = localizationService.GetLocalizedStringForCulture("Report.TableUnitPrice", languageCode);
        public string TableImport { get; set; } = localizationService.GetLocalizedStringForCulture("Report.TableAmount", languageCode);
        public string TableTotal { get; set; } = localizationService.GetLocalizedStringForCulture("Report.DeliveryNote.TableTotal", languageCode);
        public string FooterSignature { get; set; } = localizationService.GetLocalizedStringForCulture("Report.DeliveryNote.CustomerSign", languageCode);

        public Customer? Customer { get; set; }
        public Site? Site { get; set; }
        public DeliveryNote? DeliveryNote { get; set; }
        public IList<DeliveryNoteOrderReportDto>? Orders { get; set; }
        public decimal Total { get; set; }

        public bool ShowPrices => showPrices;
    }

    public class DeliveryNoteOrderReportDto
    {
        public required string Number { get; set; }
        public required DateTime Date { get; set; }
        public required string CustomerNumber { get; set; }
        public required decimal Total { get; set; }
        public required List<SalesOrderDetail> Details { get; set; }
    }
}
