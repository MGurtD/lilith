using Application.Contracts;
using Application.Services;
using Application.Services.Sales;
using Domain.Entities.Sales;

namespace Application.Services.Sales
{
    public class DeliveryNoteReportService(IUnitOfWork unitOfWork, ISalesOrderService salesOrderService, ILocalizationService localizationService) : IDeliveryNoteReportService
    {
        public async Task<Contracts.DeliveryNoteReportResponse?> GetReportById(Guid id, bool showPrices = true)
        {
            var deliveryNote = await unitOfWork.DeliveryNotes.Get(id);
            if (deliveryNote is null) return null;

            var orders = salesOrderService.GetByDeliveryNoteId(id);

            var customer = await unitOfWork.Customers.Get(deliveryNote.CustomerId);
            if (customer is null) return null;

            var site = await unitOfWork.Sites.Get(deliveryNote.SiteId);
            if (site is null) return null;

            var deliveryNoteOrders = orders.Select(order => new DeliveryNoteOrderReportDto
            {
                Number = order.Number,
                Date = order.Date,
                CustomerNumber = order.CustomerNumber,
                Details = [.. order.SalesOrderDetails],
                Total = order.SalesOrderDetails.Sum(d => d.Amount)
            }).ToList();

            var report = new DeliveryNoteReportResponse(customer.PreferredLanguage, showPrices, localizationService)
            {
                DeliveryNote = deliveryNote,
                Orders = deliveryNoteOrders,
                Customer = customer,
                Site = site,
                Total = deliveryNote.Details.Sum(d => d.Amount),
            };
            return report;
        }
    }
}








