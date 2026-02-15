using Application.Contracts;

namespace Application.Services.Sales
{
    public class CustomerRankingService(IUnitOfWork unitOfWork) : ICustomerRankingService
    {
        public async Task<IEnumerable<CustomerSalesRanking>> GetAnnualRanking(int year)
        {
            var startDate = new DateTime(year, 1, 1);
            var endDate = new DateTime(year, 12, 31, 23, 59, 59);

            // Get all invoices for the year using the repository
            var invoices = await unitOfWork.SalesInvoices
                .FindAsync(si => si.InvoiceDate >= startDate && 
                                 si.InvoiceDate <= endDate && 
                                 !si.Disabled);

            // Group and aggregate in memory
            var rankings = invoices
                .GroupBy(si => new
                {
                    si.CustomerId,
                    si.CustomerCode,
                    si.CustomerComercialName,
                    si.InvoiceDate.Year,
                    si.InvoiceDate.Month,
                    Quarter = (si.InvoiceDate.Month - 1) / 3 + 1
                })
                .Select(g => new CustomerSalesRanking
                {
                    CustomerId = g.Key.CustomerId ?? Guid.Empty,
                    CustomerCode = g.Key.CustomerCode,
                    CustomerName = g.Key.CustomerComercialName,
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    Quarter = g.Key.Quarter,
                    TotalSales = g.Sum(si => si.NetAmount),
                    InvoiceCount = g.Count()
                })
                .OrderByDescending(r => r.TotalSales)
                .ToList();

            return rankings;
        }
    }
}






