<template>
  <div class="dashboard-filter">
    <div class="dashboard-filter-left">
      <div class="dashboard-filter-field">
        <ExerciseDatePicker
          :exercises="exercicesStore.exercises"
          @range-selected="onRangeSelected"
        />
      </div>
      <Button
        class="grid_add_row_button"
        :icon="PrimeIcons.FILTER_SLASH"
        @click="clearFilter"
      />
    </div>
    <div class="dashboard-kpis">
      <div class="kpi-card">
        <div class="kpi-label">
          {{ t("analytics.cashflow.kpi.incomes") || "Ingressos" }}
        </div>
        <div class="kpi-value text-green-500">
          {{ formatCurrency(totalIncomes) }}
        </div>
      </div>
      <div class="kpi-card">
        <div class="kpi-label">
          {{ t("analytics.cashflow.kpi.expenses") || "Despeses" }}
        </div>
        <div class="kpi-value text-pink-500">
          {{ formatCurrency(totalExpenses) }}
        </div>
      </div>
      <div class="kpi-card">
        <div class="kpi-label">
          {{ t("analytics.cashflow.kpi.net") || "Nete" }}
        </div>
        <div
          :class="[
            'kpi-value',
            netTotal >= 0 ? 'text-green-600' : 'text-red-500',
          ]"
        >
          {{ formatCurrency(netTotal) }}
        </div>
      </div>
      <div class="kpi-card">
        <div class="kpi-label">
          {{
            t("analytics.cashflow.kpi.avgMonthlyNet") || "Mitjana mensual neta"
          }}
        </div>
        <div
          :class="[
            'kpi-value',
            avgMonthlyNet >= 0 ? 'text-green-600' : 'text-red-500',
          ]"
        >
          {{ formatCurrency(avgMonthlyNet) }}
        </div>
      </div>
    </div>
  </div>
  <Tabs value="0">
    <TabList>
      <Tab value="0">{{ t("analytics.cashflow.tabs.chart") || "Gràfics" }}</Tab>
      <Tab value="1">{{ t("analytics.cashflow.tabs.data") || "Dades" }}</Tab>
    </TabList>
    <TabPanels>
      <TabPanel value="0">
        <div class="dashboard-container">
          <div class="chart-area">
            <Chart
              type="line"
              :data="chartData"
              :options="chartOptions"
              class="w-full"
              style="height: 100%"
            />
          </div>
        </div>
      </TabPanel>
      <TabPanel value="1">
        <DataTable
          :value="totals"
          class="small-datatable"
          tableStyle="min-width: 100%"
          scrollable
          scrollHeight="flex"
          paginator
          sortField="date"
          :sortOrder="1"
          :rows="20"
        >
          <Column
            field="date"
            :header="t('common.date') || 'Data'"
            style="width: 15%"
            sortable
          >
            <template #body="slotProps">
              {{ formatDate(slotProps.data.date) }}
            </template>
          </Column>
          <Column :header="t('common.type') || 'Tipus'" field="type" />
          <Column :header="t('common.detail') || 'Detall'" field="detail" />
          <Column
            :header="t('common.description') || 'Descripció'"
            field="description"
          />
          <Column :header="t('common.total') || 'Total'" field="total">
            <template #body="slotProps">
              <span
                :class="
                  slotProps.data.source === 'income'
                    ? 'text-green-600 font-semibold'
                    : 'text-red-500 font-semibold'
                "
              >
                {{ formatCurrency(slotProps.data.total) }}
              </span>
            </template>
          </Column>
        </DataTable>
      </TabPanel>
    </TabPanels>
  </Tabs>
</template>
<script setup lang="ts">
import { ref, onMounted, computed, watch } from "vue";
import { useStore } from "../../../store";
import { useI18n } from "vue-i18n";
import { useToast } from "primevue/usetoast";

import {
  formatDateForQueryParameter,
  formatDate,
  formatCurrency,
} from "../../../utils/functions";
import { useExerciseStore } from "../../shared/store/exercise";

import { PrimeIcons } from "@primevue/core/api";
import ExerciseDatePicker from "../../../components/ExerciseDatePicker.vue";

import { ConsolidatedIncomes } from "../../sales/types";
import { IncomeService } from "../../sales/services/income.service";

import ExpenseServices from "../../purchase/services";
import { ConsolidatedExpense, ExpenseType } from "../../purchase/types";

const store = useStore();
const exercicesStore = useExerciseStore();
const incomeService = new IncomeService("/income");
const { t } = useI18n();
const toast = useToast();

const filter = ref({
  dates: undefined as Array<Date> | undefined,
  consolidatedBy: undefined as string | undefined,
});

const clearFilter = () => {
  store.cleanExercisePicker();
  filter.value.consolidatedBy = "";
  filterDashboard();
};

const incomes = ref([] as Array<ConsolidatedIncomes>);
const expenses = ref([] as Array<ConsolidatedExpense>);

onMounted(async () => {
  store.setMenuItem({
    icon: PrimeIcons.MONEY_BILL,
    title: "Dashboard comparatiu flux de caixa",
  });

  await exercicesStore.fetchActive();

  if (!store.exercisePicker.exercise) store.setCurrentYear();
  filterDashboard();
});

let debounceTimer: number | undefined;
const onRangeSelected = () => {
  if (debounceTimer) window.clearTimeout(debounceTimer);
  debounceTimer = window.setTimeout(() => {
    filterDashboard();
  }, 300);
};

const filterDashboard = async () => {
  totals.value = [];
  if (store.exercisePicker.dates) {
    const startTime = formatDateForQueryParameter(
      store.exercisePicker.dates[0],
    );
    const endTime = formatDateForQueryParameter(store.exercisePicker.dates[1]);

    let incomeResponse: Array<ConsolidatedIncomes> | undefined;
    let expenseResponse: Array<ConsolidatedExpense> | undefined;

    try {
      const [incRes, expRes] = await Promise.all([
        incomeService.GetBetweenDates(startTime, endTime),
        ExpenseServices.Expense.getConsolidated(startTime, endTime, "", ""),
      ]);
      incomeResponse = incRes;
      expenseResponse = expRes;
    } catch (err) {
      console.error("Error loading dashboard data:", err);
      toast.add({
        severity: "error",
        summary: t("common.error"),
        detail:
          t("analytics.cashflow.messages.loadError") ||
          "Error loading dashboard data",
        life: 5000,
      });
      return;
    }

    if (incomeResponse) incomes.value = incomeResponse;
    if (expenseResponse) expenses.value = expenseResponse;

    chartData.value = setChartData();
    chartOptions.value = setChartOptions();
  }
};

const chartData = ref();
const chartOptions = ref();

type Total = {
  date: string;
  type: string;
  detail: string;
  description: string;
  total: number;
  source: "income" | "expense";
};

const totals = ref<Total[]>([]);

const setChartData = () => {
  const incomesGroupedByYearMonth = {} as { [key: string]: number };
  const expensesGroupedByYearMonth = {} as { [key: string]: number };
  incomes.value.forEach((income) => {
    const yearMonth = `${income.year}-${String(income.month).padStart(2, "0")}`;
    incomesGroupedByYearMonth[yearMonth] =
      (incomesGroupedByYearMonth[yearMonth] || 0) + income.amount;
    totals.value.push({
      date: income.date,
      type: income.type,
      detail: income.typeDetail,
      description: income.description,
      total: income.amount,
      source: "income",
    });
  });

  expenses.value.forEach((expense) => {
    const yearMonth = `${expense.yearPaymentDate}-${String(
      expense.monthPaymentDate,
    ).padStart(2, "0")}`;
    expensesGroupedByYearMonth[yearMonth] =
      (expensesGroupedByYearMonth[yearMonth] || 0) + expense.amount;

    totals.value.push({
      date: expense.paymentDate,
      type: expense.type,
      detail: expense.typeDetail,
      description: expense.description,
      total: expense.amount,
      source: "expense",
    });
  });

  const labels = Object.keys(incomesGroupedByYearMonth)
    .concat(Object.keys(expensesGroupedByYearMonth))
    .filter((value, index, self) => self.indexOf(value) === index)
    .sort();

  const incomeDataset = {
    type: "line",
    label: t("analytics.cashflow.series.incomes") || "Ingressos",
    borderColor: "#B3FFBA",
    backgroundColor: "#B3FFBA",
    data: labels.map((month) => incomesGroupedByYearMonth[month] || 0),
    fill: false,
    tension: 0.3,
    pointRadius: 3,
  };
  const expenseDataset = {
    type: "line",
    label: t("analytics.cashflow.series.expenses") || "Despeses",
    borderColor: "#ff336f",
    backgroundColor: "#ff336f",
    data: labels.map((month) => expensesGroupedByYearMonth[month] || 0),
    fill: false,
    tension: 0.3,
    pointRadius: 3,
  };
  const balanceData: number[] = [];
  let running = 0;
  for (const m of labels) {
    const net =
      (incomesGroupedByYearMonth[m] || 0) -
      (expensesGroupedByYearMonth[m] || 0);
    running += net;
    balanceData.push(running);
  }
  const balanceDataset = {
    type: "line",
    label: t("analytics.cashflow.series.balance") || "Saldo acumulat",
    borderColor: "#8a8a8a",
    backgroundColor: "rgba(138, 138, 138, 0.15)",
    data: balanceData,
    fill: true,
    borderDash: [],
    tension: 0.25,
    pointRadius: 0,
    order: 0,
  };
  //expenseDataset,
  // Ensure balance area is rendered behind line series
  const datasets: any[] = [balanceDataset, expenseDataset, incomeDataset];
  return {
    labels,
    datasets,
  };
};
const setChartOptions = () => {
  const documentStyle = getComputedStyle(document.documentElement);
  const textColor = documentStyle.getPropertyValue("--p-text-color");
  const textColorSecondary = documentStyle.getPropertyValue(
    "--p-text-muted-color",
  );
  const surfaceBorder = documentStyle.getPropertyValue(
    "--p-content-border-color",
  );

  return {
    maintainAspectRatio: false,
    aspectRatio: 0.8,
    plugins: {
      datalabels: {},
      // Chart.js v2 compatibility
      tooltips: {
        mode: "index",
        intersect: false,
        callbacks: {
          label: function (tooltipItem: any, data: any) {
            const value = tooltipItem.yLabel || 0;
            const label = data.datasets[tooltipItem.datasetIndex].label || "";
            return `${label}: ${formatCurrency(Number(value))}`;
          },
        },
      },
      // Chart.js v3+
      tooltip: {
        mode: "index" as const,
        intersect: false,
        callbacks: {
          label: (ctx: any) => {
            const label = ctx.dataset?.label || "";
            const value = ctx.parsed?.y ?? ctx.raw ?? 0;
            return `${label}: ${formatCurrency(Number(value))}`;
          },
        },
      },
      legend: {
        labels: {
          color: textColor,
        },
      },
    },
    scales: {
      x: {
        //stacked: true,
        ticks: {
          color: textColorSecondary,
        },
        grid: {
          color: surfaceBorder,
        },
      },
      y: {
        //stacked: true,
        ticks: {
          color: textColorSecondary,
          callback: (value: any) => formatCurrency(Number(value)),
        },
        grid: {
          color: surfaceBorder,
        },
      },
    },
  };
};

// Derived metrics for KPIs
const totalIncomes = computed(() =>
  incomes.value.reduce((acc, i) => acc + (i?.amount || 0), 0),
);
const totalExpenses = computed(() =>
  expenses.value.reduce((acc, e) => acc + (e?.amount || 0), 0),
);
const netTotal = computed(() => totalIncomes.value - totalExpenses.value);
const allMonths = computed(() => {
  const set = new Set<string>();
  incomes.value.forEach((i) =>
    set.add(`${i.year}-${String(i.month).padStart(2, "0")}`),
  );
  expenses.value.forEach((e) =>
    set.add(
      `${e.yearPaymentDate}-${String(e.monthPaymentDate).padStart(2, "0")}`,
    ),
  );
  return Array.from(set).sort();
});
const avgMonthlyNet = computed(() => {
  const months = allMonths.value;
  if (!months.length) return 0;
  // Map by month
  const inc: Record<string, number> = {};
  const exp: Record<string, number> = {};
  incomes.value.forEach((i) => {
    const m = `${i.year}-${String(i.month).padStart(2, "0")}`;
    inc[m] = (inc[m] || 0) + i.amount;
  });
  expenses.value.forEach((e) => {
    const m = `${e.yearPaymentDate}-${String(e.monthPaymentDate).padStart(
      2,
      "0",
    )}`;
    exp[m] = (exp[m] || 0) + e.amount;
  });
  const sumNet = months.reduce(
    (acc, m) => acc + ((inc[m] || 0) - (exp[m] || 0)),
    0,
  );
  return sumNet / months.length;
});

// Recompute chart when data changes
watch([incomes, expenses], () => {
  chartData.value = setChartData();
});
</script>
<style scoped>
:root {
  --p-color-1: #ff5733;
  --p-color-2: #33ff57;
  --p-color-3: #3357ff;
  --p-color-4: #ff33a1;
  --p-color-5: #33ffa1;
  --p-color-6: #a133ff;
  --p-color-7: #ffd133;
  --p-color-8: #33d1ff;
  --p-color-9: #ff6f33;
  --p-color-10: #ff33d1;
  --p-color-11: #57ff33;
  --p-color-12: #5733ff;
  --p-color-13: #a1ff33;
  --p-color-14: #ff3333;
  --p-color-15: #33ffd1;
  --p-color-16: #d1ff33;
  --p-color-17: #33a1ff;
  --p-color-18: #ff33a1;
  --p-color-19: #a1ff57;
  --p-color-20: #ffa133;
  --p-color-21: #ff5733;
  --p-color-22: #33ff6f;
  --p-color-23: #6f33ff;
  --p-color-24: #ff33ff;
  --p-color-25: #33fff7;
  --p-color-26: #ff6fff;
  --p-color-27: #d133ff;
  --p-color-28: #ffd133;
  --p-color-29: #a1a1ff;
  --p-color-30: #ff336f;
  --p-color-31: #57ffa1;
  --p-color-32: #5733d1;
  --p-color-33: #a1336f;
  --p-color-34: #6fff33;
  --p-color-35: #ffd1a1;
  --p-color-36: #ffa157;
  --p-color-37: #33ff33;
  --p-color-38: #336fff;
  --p-color-39: #33ffa1;
  --p-color-40: #6f57ff;
  --p-color-41: #ff33a1;
  --p-color-42: #ff3357;
  --p-color-43: #a157ff;
  --p-color-44: #33ffd1;
  --p-color-45: #ff6f33;
  --p-color-46: #5733ff;
  --p-color-47: #33a1ff;
  --p-color-48: #ffd157;
  --p-color-49: #a1ff57;
  --p-color-50: #ff5733;
}
.dashboard-filter {
  display: flex;
  gap: 1rem;
  align-items: stretch;
  justify-content: space-between;
  color: black;
  flex-wrap: wrap;
}
.dashboard-filter-left {
  display: flex;
  gap: 1rem;
  align-items: center;
}
.dashboard-filter-field {
  display: contents;
}

.dashboard-container {
  display: flex;
  flex-direction: column;
  height: calc(100vh - var(--top-panel-height) - 10rem);
}
.chart-area {
  flex: 1;
  min-height: 24rem;
}

.dashboard-item {
  display: grid;
  grid-template-rows: 0.1fr 0.9fr;
  gap: 1rem;
  align-items: center;
  justify-items: center;
  align-content: center;
  justify-content: center;
}

.dashboard-item-header {
  text-align: center;
}

.dashboard-item-chart {
  position: relative;
  height: 60vh;
  width: 30vw;
  align-self: center;
  justify-self: center;
}

/* phone */
@media only screen and (max-width: 767px) {
  .dashboard-container {
    display: flex;
    flex-direction: column;
    gap: 1rem;
    height: calc(100vh - var(--top-panel-height) - 10rem);
  }

  .dashboard-filter {
    display: block;
  }

  .dashboard-filter-field {
    padding-bottom: 1rem;
  }

  .dashboard-item {
    display: grid;
    grid-template-rows: 0.1fr 0.9fr;
    gap: 1rem;
  }

  .dashboard-item-chart {
    display: block;
    height: 50%;
    width: 90%;
  }
}

/* KPI styles */
.dashboard-kpis {
  display: grid;
  grid-template-columns: repeat(4, minmax(10rem, 1fr));
  gap: 0.75rem;
  margin: 0;
  align-items: stretch;
  flex: 1;
}
.kpi-card {
  border: 1px solid var(--p-content-border-color);
  border-radius: 8px;
  padding: 0.75rem 1rem;
  background: var(--p-content-background, var(--p-surface-card, #fff));
}
.kpi-label {
  font-size: 0.85rem;
  color: var(--p-text-muted-color);
}
.kpi-value {
  font-size: 1.4rem;
  font-weight: 700;
}

@media only screen and (max-width: 1200px) {
  .dashboard-kpis {
    grid-template-columns: repeat(2, minmax(10rem, 1fr));
  }
}
@media only screen and (max-width: 640px) {
  .dashboard-kpis {
    grid-template-columns: repeat(1, minmax(10rem, 1fr));
  }
}
</style>
