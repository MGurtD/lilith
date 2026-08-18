using Application.Contracts;

namespace Application.Services.Analytics
{
    public class AbcAnalysisService(IUnitOfWork unitOfWork) : IAbcAnalysisService
    {
        public async Task<AbcAnalysisResult> GetCustomerAbc(DateTime startDate, DateTime endDate)
        {
            var invoices = await unitOfWork.SalesInvoices.FindAsync(si =>
                si.InvoiceDate >= startDate && si.InvoiceDate <= endDate && !si.Disabled);

            var items = invoices
                .Where(si => si.CustomerId.HasValue)
                .GroupBy(si => si.CustomerId!.Value)
                .Select(g => new AbcItem(
                    g.Key,
                    g.First().CustomerCode,
                    g.First().CustomerComercialName,
                    g.Sum(si => si.NetAmount)))
                .ToList();

            return Classify(items);
        }

        public async Task<AbcAnalysisResult> GetSupplierAbc(DateTime startDate, DateTime endDate)
        {
            var invoices = await unitOfWork.PurchaseInvoices.FindAsync(pi =>
                pi.PurchaseInvoiceDate >= startDate && pi.PurchaseInvoiceDate <= endDate && !pi.Disabled);

            var grouped = invoices
                .GroupBy(pi => pi.SupplierId)
                .Select(g => new { SupplierId = g.Key, Number = g.First().SupplierNumber, Value = g.Sum(pi => pi.NetAmount) })
                .ToList();

            var supplierIds = grouped.Select(g => g.SupplierId).ToList();
            var supplierNames = (await unitOfWork.Suppliers.FindAsync(s => supplierIds.Contains(s.Id)))
                .ToDictionary(s => s.Id, s => s.ComercialName);

            var items = grouped
                .Select(g => new AbcItem(
                    g.SupplierId,
                    g.Number,
                    supplierNames.GetValueOrDefault(g.SupplierId, string.Empty),
                    g.Value))
                .ToList();

            return Classify(items);
        }

        private readonly record struct AbcItem(Guid Id, string Code, string Name, decimal Value);

        // Pareto classification: items sorted by value desc, category by cumulative value share
        // (A up to 80%, B up to 95%, C the rest).
        private static AbcAnalysisResult Classify(List<AbcItem> items)
        {
            var ordered = items.Where(i => i.Value > 0).OrderByDescending(i => i.Value).ToList();
            var total = ordered.Sum(i => i.Value);

            var rows = new List<AbcRow>();
            decimal cumulative = 0;
            var rank = 0;

            foreach (var item in ordered)
            {
                rank++;
                cumulative += item.Value;
                var cumulativePercent = total == 0 ? 0 : cumulative / total * 100;
                var category = cumulativePercent <= 80m ? "A" : cumulativePercent <= 95m ? "B" : "C";

                rows.Add(new AbcRow
                {
                    EntityId = item.Id,
                    Code = item.Code,
                    Name = item.Name,
                    Value = Math.Round(item.Value, 2),
                    ValuePercent = total == 0 ? 0 : Math.Round(item.Value / total * 100, 2),
                    CumulativePercent = Math.Round(cumulativePercent, 2),
                    Category = category,
                    Rank = rank,
                });
            }

            var categories = new[] { "A", "B", "C" }.Select(c =>
            {
                var group = rows.Where(r => r.Category == c).ToList();
                var groupValue = group.Sum(r => r.Value);
                return new AbcCategorySummary
                {
                    Category = c,
                    ItemCount = group.Count,
                    ItemPercent = rows.Count == 0 ? 0 : Math.Round((decimal)group.Count / rows.Count * 100, 2),
                    Value = Math.Round(groupValue, 2),
                    ValuePercent = total == 0 ? 0 : Math.Round(groupValue / total * 100, 2),
                };
            }).ToList();

            return new AbcAnalysisResult
            {
                TotalValue = Math.Round(total, 2),
                TotalItems = rows.Count,
                Categories = categories,
                Rows = rows,
            };
        }
    }
}
