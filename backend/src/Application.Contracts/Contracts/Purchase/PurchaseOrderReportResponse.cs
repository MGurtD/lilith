using Domain.Entities.Production;
using Domain.Entities.Purchase;

namespace Application.Contracts;

public class PurchaseOrderReportResponse : ReportResponse
{
    public required Supplier Supplier { get; set; }
    public required Site Site { get; set; }
    public required PurchaseOrderReportDto Order { get; set; }
    public required List<PurchaseOrderDetailReportDto> Details { get; set; }

    public PurchaseOrderReportResponse() : base() { }

    public PurchaseOrderReportResponse(string languageCode, ILocalizationService localizationService) : base(languageCode)
    {
    }
}

public class PurchaseOrderReportDto
{
    public required string Number { get; set; }
    public required DateTime Date { get; set; }
    public required decimal Total { get; set; }
}

public class PurchaseOrderDetailReportDto
{
    public required string Description { get; set; }
    public required int Quantity { get; set; }
    public required decimal UnitPrice { get; set; }
    public required decimal Amount { get; set; }
}
