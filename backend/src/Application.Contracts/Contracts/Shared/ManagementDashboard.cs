namespace Application.Contracts
{
    public class MachineHoursWeekPoint : Contract
    {
        public int Year { get; set; }
        public int Week { get; set; }
        public string Label { get; set; } = string.Empty;
        public decimal Hours { get; set; }
    }

    public class MachineHoursAreaSeries : Contract
    {
        public Guid AreaId { get; set; }
        public string AreaName { get; set; } = string.Empty;
        public int MachineCount { get; set; }
        public IEnumerable<MachineHoursWeekPoint> Points { get; set; } = new List<MachineHoursWeekPoint>();
    }

    public class ManagementDashboardResult : Contract
    {
        public decimal RevenueCurrentPeriod { get; set; }
        public decimal RevenuePreviousYearPeriod { get; set; }
        public decimal RevenueVariationPercent { get; set; }

        public int PendingBudgetsCount { get; set; }
        public decimal PendingBudgetsAmount { get; set; }

        public int RejectedBudgetsCount { get; set; }

        public int OrderLinesWithoutWorkOrderCount { get; set; }

        public int NewCustomersLastMonthCount { get; set; }

        public int LostCustomersCount { get; set; }

        public IEnumerable<MachineHoursAreaSeries> MachineHoursByArea { get; set; } = new List<MachineHoursAreaSeries>();

        public int ClosedWorkOrdersWithMarginCount { get; set; }
        public decimal ProductionCostAmount { get; set; }
        public decimal InvoicedAmountForMargin { get; set; }
        public decimal ProductionCostMarginPercent { get; set; }

        public int WipWorkOrdersCount { get; set; }
        public decimal WipProductionCostAmount { get; set; }
        public decimal WipExpectedRevenueAmount { get; set; }
        public decimal WipMarginPercent { get; set; }

        public decimal PurchasesCurrentPeriod { get; set; }
        public decimal PurchasesPreviousYearPeriod { get; set; }
        public decimal PurchasesVariationPercent { get; set; }

        public decimal ExpensesCurrentPeriod { get; set; }
        public decimal ExpensesPreviousYearPeriod { get; set; }
        public decimal ExpensesVariationPercent { get; set; }
    }
}
