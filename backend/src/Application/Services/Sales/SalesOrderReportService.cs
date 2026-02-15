using Application.Contracts;
using Domain.Entities.Sales;

namespace Application.Services.Sales
{
    public class SalesOrderReportService(
        IUnitOfWork unitOfWork,
        ISalesOrderService salesOrderService,
        ILocalizationService localizationService) : ISalesOrderReportService
    {
        public async Task<SalesOrderReportResponse?> GetReportById(Guid id, bool showPrices = true)
        {
            // Get order header and validate
            var order = await salesOrderService.GetById(id);
            if (order is null) return null;

            if (!order.CustomerId.HasValue) return null;
            var customer = await unitOfWork.Customers.Get(order.CustomerId.Value);
            if (customer is null) return null;

            if (!order.SiteId.HasValue) return null;
            var site = await unitOfWork.Sites.Get(order.SiteId.Value);
            if (site is null) return null;

            // Build report response using customer's preferred language
            var report = new SalesOrderReportResponse(customer.PreferredLanguage, showPrices, localizationService)
            {
                Order = new SalesOrderHeaderReportDto
                {
                    Number = order.Number,
                    Date = order.Date,
                    CustomerNumber = order.CustomerNumber
                },
                OrderDetails = [.. order.SalesOrderDetails
                    .OrderBy(d => d.Reference!.Code)
                    .Select(d => new SalesOrderDetailReportDto
                    {
                        Quantity = d.Quantity,
                        Description = $"{d.Reference!.Code} - {d.Description}",
                        UnitPrice = d.UnitPrice,
                        Amount = d.Amount
                    })],
                Customer = customer,
                Site = site,
                Total = order.SalesOrderDetails.Sum(d => d.Amount)
            };

            return report;
        }
    }
}







