using Domain.Entities.Production;
using Domain.Entities.Sales;

namespace Application.Contracts
{
    public class BudgetReportResponse(string languageCode, ILocalizationService localizationService) : ReportResponse(languageCode)
    {
        public string Title { get; set; } = localizationService.GetLocalizedStringForCulture("Report.Budget.Title", languageCode);
        public string HeaderNumber { get; set; } = localizationService.GetLocalizedStringForCulture("Report.HeaderNumber", languageCode);
        public string HeaderDate { get; set; } = localizationService.GetLocalizedStringForCulture("Report.HeaderDate", languageCode);
        public string HeaderDeliveryIn { get; set; } = localizationService.GetLocalizedStringForCulture("Report.Budget.HeaderDeliveryIn", languageCode);
        public string HeaderDeliveryConfirmation { get; set; } = localizationService.GetLocalizedStringForCulture("Report.Budget.HeaderDeliveryConfirmation", languageCode);
        public string TableQuantity { get; set; } = localizationService.GetLocalizedStringForCulture("Report.TableQuantity", languageCode);
        public string TableConcept { get; set; } = localizationService.GetLocalizedStringForCulture("Report.TableConcept", languageCode);
        public string TableUnitPrice { get; set; } = localizationService.GetLocalizedStringForCulture("Report.TableUnitPrice", languageCode);
        public string TableAmount { get; set; } = localizationService.GetLocalizedStringForCulture("Report.TableAmount", languageCode);
        public string TableTotal { get; set; } = localizationService.GetLocalizedStringForCulture("Report.TableTotal", languageCode);
        public string FooterValidation { get; set; } = localizationService.GetLocalizedStringForCulture("Report.Budget.FooterValidation", languageCode);

        public Customer? Customer { get; set; }
        public Site? Site { get; set; }
        public Budget? Budget { get; set; }
        public decimal Total { get; set; }
    }
}
