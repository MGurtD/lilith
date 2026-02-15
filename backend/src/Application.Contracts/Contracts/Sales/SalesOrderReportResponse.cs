using Domain.Entities.Production;
using Domain.Entities.Sales;

namespace Application.Contracts;

public class SalesOrderReportResponse(string languageCode, bool showPrices, ILocalizationService localizationService) : ReportResponse(languageCode)
{
    public string Title { get; set; } = localizationService.GetLocalizedStringForCulture("Report.SalesOrder.Title", languageCode);
    public string HeaderNumber { get; set; } = localizationService.GetLocalizedStringForCulture("Report.HeaderNumber", languageCode);
    public string HeaderDate { get; set; } = localizationService.GetLocalizedStringForCulture("Report.HeaderDate", languageCode);
    public string HeaderCustomerOrder { get; set; } = localizationService.GetLocalizedStringForCulture("Report.CustomerOrder", languageCode);
    public string TableQuantity { get; set; } = localizationService.GetLocalizedStringForCulture("Report.TableQuantity", languageCode);
    public string TableConcept { get; set; } = localizationService.GetLocalizedStringForCulture("Report.TableConcept", languageCode);
    public string TableUnitPrice { get; set; } = localizationService.GetLocalizedStringForCulture("Report.TableUnitPrice", languageCode);
    public string TableImport { get; set; } = localizationService.GetLocalizedStringForCulture("Report.TableAmount", languageCode);
    public string TableTotal { get; set; } = localizationService.GetLocalizedStringForCulture("Report.TableTotal", languageCode);

    public Customer? Customer { get; set; }
    public Site? Site { get; set; }
    public SalesOrderHeaderReportDto? Order { get; set; }
    public List<SalesOrderDetailReportDto> OrderDetails { get; set; } = [];
    public decimal Total { get; set; }

    public bool ShowPrices => showPrices;
}
