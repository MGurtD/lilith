<template>
  <div class="management-grid">
    <!-- 1. Revenue current vs same period last year -->
    <div class="mgmt-card" style="grid-area: revenue">
      <div class="mgmt-card-label">{{ t("analytics.management.kpi.revenue") }}</div>
      <div class="mgmt-card-value">{{ formatCurrency(result?.revenueCurrentPeriod ?? 0) }}</div>
      <div class="mgmt-card-sub">
        {{ t("analytics.management.kpi.revenuePrevious") }}:
        {{ formatCurrency(result?.revenuePreviousYearPeriod ?? 0) }}
        <span :class="variationClass">
          ({{ (result?.revenueVariationPercent ?? 0) > 0 ? "+" : "" }}{{ result?.revenueVariationPercent ?? 0 }}%)
        </span>
      </div>
    </div>

    <!-- 2. Pending budgets -->
    <div class="mgmt-card" style="grid-area: pending">
      <div class="mgmt-card-label">{{ t("analytics.management.kpi.pendingBudgets") }}</div>
      <div class="mgmt-card-value">{{ result?.pendingBudgetsCount ?? 0 }}</div>
      <div class="mgmt-card-sub">
        {{ t("analytics.management.kpi.pendingBudgetsAmount") }}:
        {{ formatCurrency(result?.pendingBudgetsAmount ?? 0) }}
      </div>
    </div>

    <!-- 3. Rejected budgets -->
    <div class="mgmt-card" style="grid-area: rejected">
      <div class="mgmt-card-label">{{ t("analytics.management.kpi.rejectedBudgets") }}</div>
      <div class="mgmt-card-value text-red-500">{{ result?.rejectedBudgetsCount ?? 0 }}</div>
      <div class="mgmt-card-sub">{{ t("analytics.management.kpi.rejectedBudgetsSub") }}</div>
    </div>

    <!-- 4. Order lines without work order -->
    <div class="mgmt-card" style="grid-area: lines">
      <div class="mgmt-card-label">{{ t("analytics.management.kpi.linesWithoutWorkOrder") }}</div>
      <div class="mgmt-card-value">{{ result?.orderLinesWithoutWorkOrderCount ?? 0 }}</div>
      <div class="mgmt-card-sub">{{ t("analytics.management.kpi.linesWithoutWorkOrderSub") }}</div>
    </div>

    <!-- 5. New customers last month -->
    <div class="mgmt-card" style="grid-area: newcust">
      <div class="mgmt-card-label">{{ t("analytics.management.kpi.newCustomers") }}</div>
      <div class="mgmt-card-value text-green-600">{{ result?.newCustomersLastMonthCount ?? 0 }}</div>
      <div class="mgmt-card-sub">{{ t("analytics.management.kpi.newCustomersSub") }}</div>
    </div>

    <!-- 6. Lost customers -->
    <div class="mgmt-card" style="grid-area: lostcust">
      <div class="mgmt-card-label">{{ t("analytics.management.kpi.lostCustomers") }}</div>
      <div class="mgmt-card-value text-red-500">{{ result?.lostCustomersCount ?? 0 }}</div>
      <div class="mgmt-card-sub">{{ t("analytics.management.kpi.lostCustomersSub") }}</div>
    </div>

    <!-- 7. Planned machine hours per area, next 6 weeks (2 wide x 2 tall) -->
    <div class="mgmt-card mgmt-card--wide" style="grid-area: hours">
      <div class="mgmt-card-label">{{ t("analytics.management.kpi.machineHours") }}</div>
      <div class="mgmt-hours-chart">
        <Chart
          v-if="hasMachineHoursData"
          type="line"
          :data="machineHoursChartData"
          :options="machineHoursChartOptions"
          style="height: 100%"
        />
        <div v-else class="text-center text-gray-500">{{ t("analytics.abc.noData") }}</div>
      </div>
    </div>

    <!-- 8. Purchases current vs same period last year -->
    <div class="mgmt-card" style="grid-area: purchases">
      <div class="mgmt-card-label">{{ t("analytics.management.kpi.purchasesLabel") }}</div>
      <div class="mgmt-card-value">{{ formatCurrency(result?.purchasesCurrentPeriod ?? 0) }}</div>
      <div class="mgmt-card-sub">
        {{ t("analytics.management.kpi.vsLastYear") }}:
        {{ formatCurrency(result?.purchasesPreviousYearPeriod ?? 0) }}
        <span :class="purchasesVariationClass">
          ({{ (result?.purchasesVariationPercent ?? 0) > 0 ? "+" : "" }}{{ result?.purchasesVariationPercent ?? 0 }}%)
        </span>
      </div>
    </div>

    <!-- 8b. Expenses current vs same period last year -->
    <div class="mgmt-card" style="grid-area: expenses">
      <div class="mgmt-card-label">{{ t("analytics.management.kpi.expensesLabel") }}</div>
      <div class="mgmt-card-value">{{ formatCurrency(result?.expensesCurrentPeriod ?? 0) }}</div>
      <div class="mgmt-card-sub">
        {{ t("analytics.management.kpi.vsLastYear") }}:
        {{ formatCurrency(result?.expensesPreviousYearPeriod ?? 0) }}
        <span :class="expensesVariationClass">
          ({{ (result?.expensesVariationPercent ?? 0) > 0 ? "+" : "" }}{{ result?.expensesVariationPercent ?? 0 }}%)
        </span>
      </div>
    </div>

    <!-- 9. Production cost margin vs invoiced amount -->
    <div class="mgmt-card" style="grid-area: reserved2">
      <div class="mgmt-card-label">{{ t("analytics.management.kpi.productionMargin") }}</div>
      <div class="mgmt-card-value-row">
        <div class="mgmt-card-value" :class="productionMarginClass">
          {{ result?.productionCostMarginPercent ?? 0 }}%
        </div>
        <div class="mgmt-card-value-wip" :class="wipMarginClass">
          {{ t("analytics.management.kpi.wipMarginPrefix") }} {{ result?.wipMarginPercent ?? 0 }}%
        </div>
      </div>
      <div class="mgmt-card-sub">
        {{ result?.closedWorkOrdersWithMarginCount ?? 0 }} {{ t("analytics.management.kpi.productionMarginCount") }}
        &middot; {{ formatCurrency(result?.productionCostAmount ?? 0) }} / {{ formatCurrency(result?.invoicedAmountForMargin ?? 0) }}
      </div>
      <div class="mgmt-card-sub">
        {{ result?.wipWorkOrdersCount ?? 0 }} {{ t("analytics.management.kpi.wipCount") }}
        &middot; {{ formatCurrency(result?.wipProductionCostAmount ?? 0) }} / {{ formatCurrency(result?.wipExpectedRevenueAmount ?? 0) }}
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, watch } from "vue";
import { useI18n } from "vue-i18n";
import { PrimeIcons } from "@primevue/core/api";
import { formatCurrency } from "@/utils/functions";
import { useStore } from "@/store";
import { ManagementDashboardService } from "../services/managementDashboard.service";
import type { ManagementDashboardResult } from "../types";

const { t, locale } = useI18n();
const store = useStore();
const service = new ManagementDashboardService("/managementdashboard");

const result = ref<ManagementDashboardResult>();

const variationClass = computed(() =>
  (result.value?.revenueVariationPercent ?? 0) >= 0 ? "text-green-600" : "text-red-500",
);

const productionMarginClass = computed(() =>
  (result.value?.productionCostMarginPercent ?? 0) >= 0 ? "text-green-600" : "text-red-500",
);

const purchasesVariationClass = computed(() =>
  (result.value?.purchasesVariationPercent ?? 0) <= 0 ? "text-green-600" : "text-red-500",
);

const expensesVariationClass = computed(() =>
  (result.value?.expensesVariationPercent ?? 0) <= 0 ? "text-green-600" : "text-red-500",
);

const wipMarginClass = computed(() =>
  (result.value?.wipMarginPercent ?? 0) >= 0 ? "text-green-600" : "text-red-500",
);

// Distinct color per area line; index-based so it stays stable across reloads.
const AREA_COLORS = ["#42A5F5", "#66BB6A", "#FFA726", "#EC407A", "#AB47BC", "#26A69A", "#FFCA28", "#8D6E63"];

const hasMachineHoursData = computed(() => (result.value?.machineHoursByArea?.length ?? 0) > 0);

const machineHoursChartData = computed(() => {
  const series = result.value?.machineHoursByArea ?? [];
  const labels = series[0]?.points.map((p) => p.label) ?? [];
  return {
    labels,
    datasets: series.map((s, i) => ({
      label: `${s.areaName} (${s.machineCount})`,
      data: s.points.map((p) => p.hours),
      borderColor: AREA_COLORS[i % AREA_COLORS.length],
      backgroundColor: AREA_COLORS[i % AREA_COLORS.length],
      tension: 0.25,
      pointRadius: 3,
      fill: false,
    })),
  };
});

const machineHoursChartOptions = computed(() => ({
  maintainAspectRatio: false,
  plugins: { legend: { display: true, position: "bottom" } },
  scales: {
    y: {
      beginAtZero: true,
      title: { display: true, text: t("analytics.management.kpi.machineHoursAxis") },
    },
  },
}));

const setMenuItem = () => {
  store.setMenuItem({
    icon: PrimeIcons.BRIEFCASE,
    title: t("analytics.management.title"),
  });
};

watch(locale, setMenuItem);

onMounted(async () => {
  setMenuItem();
  result.value = await service.GetDashboard();
});
</script>

<style scoped>
.management-grid {
  display: grid;
  /* 20 columns = lcm(5,4): row 1 spans 5 equal cards (4 cols each), rows 2-3 span 4 wider cards (5 cols each). */
  grid-template-columns: repeat(20, minmax(2rem, 1fr));
  grid-template-rows: repeat(3, minmax(8rem, auto));
  grid-template-areas:
    "revenue revenue revenue revenue purchases purchases purchases purchases expenses expenses expenses expenses pending pending pending pending rejected rejected rejected rejected"
    "newcust newcust newcust newcust newcust lostcust lostcust lostcust lostcust lostcust hours hours hours hours hours hours hours hours hours hours"
    "lines lines lines lines lines reserved2 reserved2 reserved2 reserved2 reserved2 hours hours hours hours hours hours hours hours hours hours";
  gap: 1rem;
}

.mgmt-card {
  border: 1px solid var(--p-content-border-color);
  border-radius: 8px;
  padding: 1rem 1.25rem;
  background: var(--p-content-background, var(--p-surface-card, #fff));
  min-height: 8rem;
  display: flex;
  flex-direction: column;
  justify-content: center;
  gap: 0.35rem;
}

.mgmt-card--wide {
  align-items: stretch;
  justify-content: flex-start;
}

.mgmt-hours-chart {
  flex: 1;
  min-height: 14rem;
}

.mgmt-card--reserved {
  border-style: dashed;
  align-items: center;
  justify-content: center;
  color: var(--p-text-muted-color);
}

.mgmt-card-label {
  font-size: 0.85rem;
  color: var(--p-text-muted-color);
}

.mgmt-card-value {
  font-size: 1.8rem;
  font-weight: 700;
}

.mgmt-card-value-row {
  display: flex;
  align-items: baseline;
  gap: 0.5rem;
}

.mgmt-card-value-wip {
  font-size: 1rem;
  font-weight: 600;
}

.mgmt-card-sub {
  font-size: 0.78rem;
  color: var(--p-text-muted-color);
}

.mgmt-card-line-label {
  font-size: 0.7rem;
  font-weight: 700;
  text-transform: uppercase;
  letter-spacing: 0.03em;
  color: var(--p-text-muted-color);
}

@media only screen and (max-width: 1200px) {
  .management-grid {
    grid-template-columns: repeat(2, minmax(14rem, 1fr));
    grid-template-areas:
      "revenue purchases"
      "expenses pending"
      "rejected newcust"
      "lostcust ."
      "hours hours"
      "lines reserved2";
  }
}
@media only screen and (max-width: 640px) {
  .management-grid {
    grid-template-columns: 1fr;
    grid-template-areas:
      "revenue"
      "purchases"
      "expenses"
      "pending"
      "rejected"
      "newcust"
      "lostcust"
      "hours"
      "lines"
      "reserved2";
  }
}
</style>
