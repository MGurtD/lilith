using Application.Contracts;

namespace Application.Services.Purchase
{
    public class PurchaseOrderReportService(IUnitOfWork unitOfWork) : IPurchaseOrderReportService
    {
        public async Task<Application.Contracts.PurchaseOrderReportResponse?> GetReportById(Guid id)
        {
            var order = await unitOfWork.PurchaseOrders.Get(id);
            if (order == null) return null;

            var supplier = await unitOfWork.Suppliers.Get(order.SupplierId);
            if (supplier == null) return null;

            var site = (await unitOfWork.Sites.FindAsync(s => !s.Disabled)).FirstOrDefault();
            if (site == null) return null;

            var referenceIds = order.Details.Select(detail => detail.ReferenceId).ToList();
            var references = await unitOfWork.References.FindAsync(r => referenceIds.Contains(r.Id));
            var supplierReferences = unitOfWork.Suppliers.GetSupplierReferences(supplier.Id);

            var orderDto = new PurchaseOrderReportDto()
            {
                Number = order.Number,
                Date = order.Date,
                Total = order.Details.Sum(d => d.Amount)
            };

            var orderDetails = new List<PurchaseOrderDetailReportDto>();
            foreach (var detail in order.Details)
            {
                var reference = references.FirstOrDefault(r => r.Id == detail.ReferenceId);
                var supplierReference = supplierReferences.FirstOrDefault(sr => sr.ReferenceId == detail.ReferenceId);
                var description = supplierReference != null ? $"{supplierReference.SupplierCode} - {supplierReference.SupplierDescription}" : reference!.GetFullName();

                if (!string.IsNullOrEmpty(detail.Description)) description = $"{description} - {detail.Description}";

                orderDetails.Add(new PurchaseOrderDetailReportDto()
                {
                    Description = description,
                    Quantity = detail.Quantity,
                    UnitPrice = detail.UnitPrice,
                    Amount = detail.Amount
                });
            }

            return new PurchaseOrderReportResponse()
            {
                Supplier = supplier,
                Site = site,
                Order = orderDto,
                Details = orderDetails.GroupBy(d => d.Description).Select(g => new PurchaseOrderDetailReportDto()
                {
                    Description = g.Key,
                    Quantity = g.Sum(d => d.Quantity),
                    UnitPrice = g.First().UnitPrice,
                    Amount = g.Sum(d => d.Amount)
                }).ToList()
            };
        }
    }
}









