using Application.Contracts;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace Application.Services.Analytics
{
    public class ManagementDashboardService(IUnitOfWork unitOfWork, IExerciseService exerciseService) : IManagementDashboardService
    {
        public async Task<ManagementDashboardResult> GetDashboard()
        {
            var now = DateTime.Now;
            var currentExercise = exerciseService.GetExerciceByDate(now);

            var result = new ManagementDashboardResult();

            if (currentExercise == null)
            {
                // No exercise covers today: nothing to compute for exercise-scoped KPIs.
                return result;
            }

            var currentPeriodStart = currentExercise.StartDate;
            var currentPeriodEnd = now;
            var previousPeriodStart = currentPeriodStart.AddYears(-1);
            var previousPeriodEnd = currentPeriodEnd.AddYears(-1);

            // 1. Revenue: current exercise YTD vs same YTD window last year (base amount, no tax).
            var currentInvoices = await unitOfWork.SalesInvoices.FindAsync(si =>
                !si.Disabled && si.InvoiceDate >= currentPeriodStart && si.InvoiceDate <= currentPeriodEnd);
            var previousInvoices = await unitOfWork.SalesInvoices.FindAsync(si =>
                !si.Disabled && si.InvoiceDate >= previousPeriodStart && si.InvoiceDate <= previousPeriodEnd);

            result.RevenueCurrentPeriod = Math.Round(currentInvoices.Sum(si => si.BaseAmount), 2);
            result.RevenuePreviousYearPeriod = Math.Round(previousInvoices.Sum(si => si.BaseAmount), 2);
            result.RevenueVariationPercent = result.RevenuePreviousYearPeriod == 0
                ? 0
                : Math.Round((result.RevenueCurrentPeriod - result.RevenuePreviousYearPeriod) / result.RevenuePreviousYearPeriod * 100, 1);

            // 2. Pending budgets (all open, not scoped to exercise) + their line-level amount.
            var statusPending = await unitOfWork.Lifecycles.GetStatusByName(StatusConstants.Lifecycles.Budget, StatusConstants.Statuses.PendentAcceptar);
            if (statusPending != null)
            {
                var pendingBudgets = await unitOfWork.Budgets.FindAsyncWithQueryParams(
                    b => !b.Disabled && b.StatusId == statusPending.Id,
                    q => q.Include(b => b.Details));

                result.PendingBudgetsCount = pendingBudgets.Count;
                result.PendingBudgetsAmount = Math.Round(
                    pendingBudgets.Sum(b => b.Details.Where(d => !d.Disabled).Sum(d => d.Amount)), 2);
            }

            // 3. Rejected budgets within the current exercise period.
            var statusRejected = await unitOfWork.Lifecycles.GetStatusByName(StatusConstants.Lifecycles.Budget, StatusConstants.Statuses.Rebutjat);
            if (statusRejected != null)
            {
                var rejectedBudgets = await unitOfWork.Budgets.FindAsync(b =>
                    !b.Disabled && b.StatusId == statusRejected.Id &&
                    b.Date >= currentExercise.StartDate && b.Date <= currentExercise.EndDate);
                result.RejectedBudgetsCount = rejectedBudgets.Count;
            }

            // 4. Order lines without a work order: exclude delivered lines and served/invoiced/disabled orders.
            var statusServida = await unitOfWork.Lifecycles.GetStatusByName(StatusConstants.Lifecycles.SalesOrder, StatusConstants.Statuses.ComandaServida);
            var statusFacturada = await unitOfWork.Lifecycles.GetStatusByName(StatusConstants.Lifecycles.SalesOrder, StatusConstants.Statuses.ComandaFacturada);
            var excludedOrderStatusIds = new[] { statusServida?.Id, statusFacturada?.Id }
                .Where(id => id.HasValue)
                .Select(id => id!.Value)
                .ToHashSet();

            var detailsWithoutWorkOrder = await unitOfWork.SalesOrderDetails.FindAsyncWithQueryParams(
                d => !d.Disabled && d.WorkOrderId == null && !d.IsDelivered,
                q => q.Include(d => d.SalesOrderHeader));

            result.OrderLinesWithoutWorkOrderCount = detailsWithoutWorkOrder.Count(d =>
                d.SalesOrderHeader != null &&
                !d.SalesOrderHeader.Disabled &&
                (d.SalesOrderHeader.StatusId == null || !excludedOrderStatusIds.Contains(d.SalesOrderHeader.StatusId.Value)));

            // 5. New customers created in the last 30 days.
            var newCustomers = await unitOfWork.Customers.FindAsync(c =>
                !c.Disabled && c.CreatedOn >= now.AddDays(-30));
            result.NewCustomersLastMonthCount = newCustomers.Count;

            // 6. Customers invoiced last full exercise year but not invoiced this exercise YTD.
            var previousExercise = exerciseService.GetExerciceByDate(currentPeriodStart.AddYears(-1));
            var previousYearStart = previousExercise?.StartDate ?? currentExercise.StartDate.AddYears(-1);
            var previousYearEnd = previousExercise?.EndDate ?? currentExercise.EndDate.AddYears(-1);

            var previousYearInvoices = await unitOfWork.SalesInvoices.FindAsync(si =>
                !si.Disabled && si.CustomerId != null && si.InvoiceDate >= previousYearStart && si.InvoiceDate <= previousYearEnd);
            var currentYearCustomerIds = currentInvoices
                .Where(si => si.CustomerId != null)
                .Select(si => si.CustomerId!.Value)
                .ToHashSet();

            result.LostCustomersCount = previousYearInvoices
                .Where(si => si.CustomerId != null)
                .Select(si => si.CustomerId!.Value)
                .Distinct()
                .Count(id => !currentYearCustomerIds.Contains(id));

            // 7. Planned machine hours per plant-visible area, next 6 weeks (mirrors MetricsService's
            // cycle-time-vs-block-time estimate). Overdue/undated work bucket into the current week.
            var areas = (await unitOfWork.Areas.FindAsync(a => !a.Disabled && a.IsVisibleInPlant)).ToList();
            if (areas.Count > 0)
            {
                var areaIds = areas.Select(a => a.Id).ToHashSet();
                var workcenters = (await unitOfWork.Workcenters.FindAsync(w => !w.Disabled && areaIds.Contains(w.AreaId))).ToList();
                var machineCountByArea = workcenters.GroupBy(w => w.AreaId).ToDictionary(g => g.Key, g => g.Count());
                var workcenterAreaById = workcenters.ToDictionary(w => w.Id, w => w.AreaId);

                // A WorkcenterType can span more than one area; fall back to the alphabetically-first
                // workcenter of that type as the representative area when no PreferredWorkcenterId is set.
                var typeAreaMap = workcenters
                    .GroupBy(w => w.WorkcenterTypeId)
                    .ToDictionary(g => g.Key, g => g.OrderBy(w => w.Name).First().AreaId);

                var statusTancada = await unitOfWork.Lifecycles.GetStatusByName(StatusConstants.Lifecycles.WorkOrder, StatusConstants.Statuses.Tancada);
                var statusCancellada = await unitOfWork.Lifecycles.GetStatusByName(StatusConstants.Lifecycles.WorkOrder, StatusConstants.Statuses.OFCancellada);
                var excludedWorkOrderStatusIds = new[] { statusTancada?.Id, statusCancellada?.Id }
                    .Where(id => id.HasValue)
                    .Select(id => id!.Value)
                    .ToHashSet();

                var bucketDates = Enumerable.Range(0, 6).Select(i => now.Date.AddDays(7 * i)).ToList();
                var buckets = bucketDates
                    .Select(d => (Year: ISOWeek.GetYear(d), Week: ISOWeek.GetWeekOfYear(d)))
                    .ToList();
                var windowEnd = now.Date.AddDays(7 * 6);

                var openWorkOrders = await unitOfWork.WorkOrders.FindAsyncWithQueryParams(
                    wo => !wo.Disabled && wo.PlannedDate <= windowEnd && !excludedWorkOrderStatusIds.Contains(wo.StatusId),
                    q => q.Include(wo => wo.Phases).ThenInclude(p => p.Details));

                var hoursByAreaWeek = new Dictionary<(Guid AreaId, int BucketIndex), decimal>();

                foreach (var workOrder in openWorkOrders)
                {
                    var (woYear, woWeek) = (ISOWeek.GetYear(workOrder.PlannedDate), ISOWeek.GetWeekOfYear(workOrder.PlannedDate));
                    var bucketIndex = buckets.FindIndex(b => b.Year == woYear && b.Week == woWeek);
                    if (bucketIndex < 0) bucketIndex = workOrder.PlannedDate < now.Date ? 0 : buckets.Count - 1;

                    foreach (var phase in workOrder.Phases.Where(p => !p.Disabled && !p.IsExternalWork))
                    {
                        Guid? areaId = null;
                        if (phase.PreferredWorkcenterId.HasValue && workcenterAreaById.TryGetValue(phase.PreferredWorkcenterId.Value, out var preferredAreaId))
                            areaId = preferredAreaId;
                        else if (phase.WorkcenterTypeId.HasValue && typeAreaMap.TryGetValue(phase.WorkcenterTypeId.Value, out var typeAreaId))
                            areaId = typeAreaId;

                        if (!areaId.HasValue || !areaIds.Contains(areaId.Value)) continue;

                        foreach (var detail in phase.Details.Where(d => !d.Disabled))
                        {
                            var hours = (detail.IsCycleTime ? workOrder.PlannedQuantity * detail.EstimatedTime : detail.EstimatedTime) / 60m;
                            var key = (areaId.Value, bucketIndex);
                            hoursByAreaWeek[key] = hoursByAreaWeek.GetValueOrDefault(key, 0m) + hours;
                        }
                    }
                }

                result.MachineHoursByArea = areas.Select(a => new MachineHoursAreaSeries
                {
                    AreaId = a.Id,
                    AreaName = a.Name,
                    MachineCount = machineCountByArea.GetValueOrDefault(a.Id, 0),
                    Points = buckets.Select((b, i) => new MachineHoursWeekPoint
                    {
                        Year = b.Year,
                        Week = b.Week,
                        Label = $"S{b.Week}",
                        Hours = Math.Round(hoursByAreaWeek.GetValueOrDefault((a.Id, i), 0m), 1),
                    }).ToList(),
                }).ToList();
            }

            // 8. Real production-cost margin vs invoiced amount, for work orders closed within the
            // current exercise. Follows WorkOrder -> SalesOrderDetail -> DeliveryNoteDetail -> SalesInvoiceDetail.
            var statusWorkOrderTancada = await unitOfWork.Lifecycles.GetStatusByName(StatusConstants.Lifecycles.WorkOrder, StatusConstants.Statuses.Tancada);
            if (statusWorkOrderTancada != null)
            {
                var closedWorkOrders = await unitOfWork.WorkOrders.FindAsync(wo =>
                    !wo.Disabled && wo.ExerciseId == currentExercise.Id && wo.StatusId == statusWorkOrderTancada.Id);
                var workOrderIds = closedWorkOrders.Select(wo => wo.Id).ToList();

                if (workOrderIds.Count > 0)
                {
                    var linkedDetails = await unitOfWork.SalesOrderDetails.FindAsync(d =>
                        !d.Disabled && d.WorkOrderId != null && workOrderIds.Contains(d.WorkOrderId.Value));
                    var detailIds = linkedDetails.Select(d => d.Id).ToList();

                    var deliveryDetails = await unitOfWork.DeliveryNotes.Details.FindAsync(dd =>
                        !dd.Disabled && dd.SalesOrderDetailId != null && detailIds.Contains(dd.SalesOrderDetailId.Value));
                    var deliveryDetailIds = deliveryDetails.Select(dd => dd.Id).ToList();

                    var invoiceDetails = await unitOfWork.SalesInvoices.InvoiceDetails.FindAsync(id =>
                        !id.Disabled && id.DeliveryNoteDetailId != null && deliveryDetailIds.Contains(id.DeliveryNoteDetailId.Value));

                    var invoicedByDeliveryDetail = invoiceDetails
                        .GroupBy(id => id.DeliveryNoteDetailId!.Value)
                        .ToDictionary(g => g.Key, g => g.Sum(id => id.Amount));

                    var invoicedBySalesOrderDetail = deliveryDetails
                        .Where(dd => dd.SalesOrderDetailId.HasValue)
                        .GroupBy(dd => dd.SalesOrderDetailId!.Value)
                        .ToDictionary(g => g.Key, g => g.Sum(dd => invoicedByDeliveryDetail.GetValueOrDefault(dd.Id, 0m)));

                    var invoicedByWorkOrder = linkedDetails
                        .Where(d => d.WorkOrderId.HasValue)
                        .GroupBy(d => d.WorkOrderId!.Value)
                        .ToDictionary(g => g.Key, g => g.Sum(d => invoicedBySalesOrderDetail.GetValueOrDefault(d.Id, 0m)));

                    var withInvoicedAmount = closedWorkOrders
                        .Select(wo => new
                        {
                            ProductionCost = wo.MachineCost + wo.OperatorCost + wo.MaterialCost,
                            InvoicedAmount = invoicedByWorkOrder.GetValueOrDefault(wo.Id, 0m),
                        })
                        .Where(x => x.InvoicedAmount > 0)
                        .ToList();

                    result.ClosedWorkOrdersWithMarginCount = withInvoicedAmount.Count;
                    result.ProductionCostAmount = Math.Round(withInvoicedAmount.Sum(x => x.ProductionCost), 2);
                    result.InvoicedAmountForMargin = Math.Round(withInvoicedAmount.Sum(x => x.InvoicedAmount), 2);
                    result.ProductionCostMarginPercent = result.InvoicedAmountForMargin == 0
                        ? 0
                        : Math.Round((result.InvoicedAmountForMargin - result.ProductionCostAmount) / result.InvoicedAmountForMargin * 100, 1);
                }

                // WIP: work orders still open, accumulated real cost so far vs the expected
                // revenue (sales order line Amount) rather than actual invoiced amount, since
                // they haven't been invoiced yet.
                var statusWorkOrderCancellada = await unitOfWork.Lifecycles.GetStatusByName(StatusConstants.Lifecycles.WorkOrder, StatusConstants.Statuses.OFCancellada);
                var excludedFromWip = new[] { statusWorkOrderTancada.Id, statusWorkOrderCancellada?.Id }
                    .Where(id => id.HasValue)
                    .Select(id => id!.Value)
                    .ToHashSet();

                var wipWorkOrders = await unitOfWork.WorkOrders.FindAsync(wo =>
                    !wo.Disabled && wo.ExerciseId == currentExercise.Id && !excludedFromWip.Contains(wo.StatusId));

                if (wipWorkOrders.Count > 0)
                {
                    var wipWorkOrderIds = wipWorkOrders.Select(wo => wo.Id).ToList();
                    var wipLinkedDetails = await unitOfWork.SalesOrderDetails.FindAsync(d =>
                        !d.Disabled && d.WorkOrderId != null && wipWorkOrderIds.Contains(d.WorkOrderId.Value));
                    var expectedRevenueByWorkOrder = wipLinkedDetails
                        .Where(d => d.WorkOrderId.HasValue)
                        .GroupBy(d => d.WorkOrderId!.Value)
                        .ToDictionary(g => g.Key, g => g.Sum(d => d.Amount));

                    result.WipWorkOrdersCount = wipWorkOrders.Count;
                    result.WipProductionCostAmount = Math.Round(wipWorkOrders.Sum(wo => wo.MachineCost + wo.OperatorCost + wo.MaterialCost), 2);
                    result.WipExpectedRevenueAmount = Math.Round(wipWorkOrders.Sum(wo => expectedRevenueByWorkOrder.GetValueOrDefault(wo.Id, 0m)), 2);
                    result.WipMarginPercent = result.WipExpectedRevenueAmount == 0
                        ? 0
                        : Math.Round((result.WipExpectedRevenueAmount - result.WipProductionCostAmount) / result.WipExpectedRevenueAmount * 100, 1);
                }
            }

            // 10. Purchases and expenses: current exercise YTD vs same YTD window last year (mirrors KPI 1).
            var currentPurchaseInvoices = await unitOfWork.PurchaseInvoices.FindAsync(pi =>
                !pi.Disabled && pi.PurchaseInvoiceDate >= currentPeriodStart && pi.PurchaseInvoiceDate <= currentPeriodEnd);
            var previousPurchaseInvoices = await unitOfWork.PurchaseInvoices.FindAsync(pi =>
                !pi.Disabled && pi.PurchaseInvoiceDate >= previousPeriodStart && pi.PurchaseInvoiceDate <= previousPeriodEnd);

            result.PurchasesCurrentPeriod = Math.Round(currentPurchaseInvoices.Sum(pi => pi.BaseAmount), 2);
            result.PurchasesPreviousYearPeriod = Math.Round(previousPurchaseInvoices.Sum(pi => pi.BaseAmount), 2);
            result.PurchasesVariationPercent = result.PurchasesPreviousYearPeriod == 0
                ? 0
                : Math.Round((result.PurchasesCurrentPeriod - result.PurchasesPreviousYearPeriod) / result.PurchasesPreviousYearPeriod * 100, 1);

            var currentExpenses = await unitOfWork.Expenses.FindAsync(e =>
                !e.Disabled && e.PaymentDate >= currentPeriodStart && e.PaymentDate <= currentPeriodEnd);
            var previousExpenses = await unitOfWork.Expenses.FindAsync(e =>
                !e.Disabled && e.PaymentDate >= previousPeriodStart && e.PaymentDate <= previousPeriodEnd);

            result.ExpensesCurrentPeriod = Math.Round(currentExpenses.Sum(e => e.Amount), 2);
            result.ExpensesPreviousYearPeriod = Math.Round(previousExpenses.Sum(e => e.Amount), 2);
            result.ExpensesVariationPercent = result.ExpensesPreviousYearPeriod == 0
                ? 0
                : Math.Round((result.ExpensesCurrentPeriod - result.ExpensesPreviousYearPeriod) / result.ExpensesPreviousYearPeriod * 100, 1);

            return result;
        }
    }
}
