export interface CustomerSalesRanking {
  customerId: string;
  customerCode: string;
  customerName: string;
  year: number;
  month: number;
  quarter: number;
  totalSales: number;
  invoiceCount: number;
  monthKey: string;
  quarterKey: string;
}

export interface BudgetConversionRow {
  customerId: string;
  customerName: string;
  budgetId: string;
  budgetNumber: string;
  budgetDate: string;
  statusId?: string;
  amount: number;
  orderId?: string;
  orderNumber?: string;
  orderDate?: string;
  orderAmount?: number;
  daysToConversion?: number;
}

export interface BudgetConversionResult {
  totalBudgets: number;
  totalOrders: number;
  conversionRate: number;
  averageAcceptanceDays: number;
  totalBudgetAmount: number;
  totalConvertedAmount: number;
  rows: BudgetConversionRow[];
}

export interface ProductionTimeDeviationRow {
  workOrderId: string;
  workOrderCode: string;
  phaseId: string;
  phaseName: string;
  machineStatusId?: string;
  statusName: string;
  isCycleTime: boolean;
  quantity: number;
  theoreticalMachineTime: number;
  realMachineTime: number;
  machineDeviation: number;
  theoreticalOperatorTime: number;
  realOperatorTime: number;
  operatorDeviation: number;
}

export interface ProductionTimeDeviationResult {
  theoreticalMachineTime: number;
  realMachineTime: number;
  machineDeviation: number;
  machineDeviationPercent: number;
  theoreticalOperatorTime: number;
  realOperatorTime: number;
  operatorDeviation: number;
  operatorDeviationPercent: number;
  stepCount: number;
  deviatedStepCount: number;
  rows: ProductionTimeDeviationRow[];
}

export interface AbcRow {
  entityId: string;
  code: string;
  name: string;
  value: number;
  valuePercent: number;
  cumulativePercent: number;
  category: string;
  rank: number;
}

export interface AbcCategorySummary {
  category: string;
  itemCount: number;
  itemPercent: number;
  value: number;
  valuePercent: number;
}

export interface AbcAnalysisResult {
  totalValue: number;
  totalItems: number;
  categories: AbcCategorySummary[];
  rows: AbcRow[];
}

export interface MachineHoursWeekPoint {
  year: number;
  week: number;
  label: string;
  hours: number;
}

export interface MachineHoursAreaSeries {
  areaId: string;
  areaName: string;
  machineCount: number;
  points: MachineHoursWeekPoint[];
}

export interface ManagementDashboardResult {
  revenueCurrentPeriod: number;
  revenuePreviousYearPeriod: number;
  revenueVariationPercent: number;
  pendingBudgetsCount: number;
  pendingBudgetsAmount: number;
  rejectedBudgetsCount: number;
  orderLinesWithoutWorkOrderCount: number;
  newCustomersLastMonthCount: number;
  lostCustomersCount: number;
  machineHoursByArea: MachineHoursAreaSeries[];
  closedWorkOrdersWithMarginCount: number;
  productionCostAmount: number;
  invoicedAmountForMargin: number;
  productionCostMarginPercent: number;
  wipWorkOrdersCount: number;
  wipProductionCostAmount: number;
  wipExpectedRevenueAmount: number;
  wipMarginPercent: number;
  purchasesCurrentPeriod: number;
  purchasesPreviousYearPeriod: number;
  purchasesVariationPercent: number;
  expensesCurrentPeriod: number;
  expensesPreviousYearPeriod: number;
  expensesVariationPercent: number;
}
