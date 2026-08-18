using Application.Contracts;
using Microsoft.EntityFrameworkCore;

namespace Application.Services.Sales
{
    public class BudgetConversionService(IUnitOfWork unitOfWork) : IBudgetConversionService
    {
        public async Task<BudgetConversionResult> GetConversion(
            DateTime startDate, DateTime endDate, Guid? customerId)
        {
            // Budget header Amount is not maintained; the real total lives in the detail lines.
            var budgets = (await unitOfWork.Budgets.FindAsyncWithQueryParams(
                    b => b.Date >= startDate && b.Date <= endDate && !b.Disabled &&
                         (customerId == null || b.CustomerId == customerId),
                    q => q.Include(b => b.Details)))
                .ToList();

            var budgetIds = budgets.Select(b => b.Id).ToList();
            var amountByBudget = budgets.ToDictionary(
                b => b.Id,
                b => b.Details.Where(d => !d.Disabled).Sum(d => d.Amount));

            var orders = (await unitOfWork.SalesOrderHeaders.FindAsync(o =>
                    o.BudgetId != null && budgetIds.Contains(o.BudgetId!.Value) && !o.Disabled))
                .GroupBy(o => o.BudgetId!.Value)
                .ToDictionary(g => g.Key, g => g.OrderBy(o => o.Date).First());

            var orderIds = orders.Values.Select(o => o.Id).ToList();
            var amountByOrder = (await unitOfWork.SalesOrderDetails.FindAsync(d =>
                    orderIds.Contains(d.SalesOrderHeaderId) && !d.Disabled))
                .GroupBy(d => d.SalesOrderHeaderId)
                .ToDictionary(g => g.Key, g => g.Sum(d => d.Amount));

            var customerIds = budgets.Select(b => b.CustomerId).Distinct().ToList();
            var customers = (await unitOfWork.Customers.FindAsync(c => customerIds.Contains(c.Id)))
                .ToDictionary(c => c.Id, c => c.ComercialName);

            var rows = budgets.Select(b =>
            {
                orders.TryGetValue(b.Id, out var order);
                return new BudgetConversionRow
                {
                    CustomerId = b.CustomerId,
                    CustomerName = customers.GetValueOrDefault(b.CustomerId, string.Empty),
                    BudgetId = b.Id,
                    BudgetNumber = b.Number,
                    BudgetDate = b.Date,
                    StatusId = b.StatusId,
                    Amount = amountByBudget.GetValueOrDefault(b.Id, 0),
                    OrderId = order?.Id,
                    OrderNumber = order?.Number,
                    OrderDate = order?.Date,
                    OrderAmount = order == null ? null : amountByOrder.GetValueOrDefault(order.Id, 0),
                    DaysToConversion = order == null ? null : (order.Date.Date - b.Date.Date).Days,
                };
            }).ToList();

            var converted = rows.Where(r => r.OrderId != null).ToList();

            return new BudgetConversionResult
            {
                TotalBudgets = budgets.Count,
                TotalOrders = converted.Count,
                ConversionRate = budgets.Count == 0
                    ? 0
                    : Math.Round((decimal)converted.Count / budgets.Count * 100, 2),
                AverageAcceptanceDays = converted.Count == 0
                    ? 0
                    : Math.Round(converted.Average(r => r.DaysToConversion!.Value), 1),
                TotalBudgetAmount = amountByBudget.Values.Sum(),
                TotalConvertedAmount = converted.Sum(r => r.OrderAmount ?? 0),
                Rows = rows,
            };
        }
    }
}
