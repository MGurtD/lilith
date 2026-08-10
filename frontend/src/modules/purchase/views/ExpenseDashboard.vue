<template>
  <div class="dashboard-filter">
    <div class="dashboard-filter-left">
      <TableFilter
        :config="filterConfig"
        v-model="filter"
        :show-title="false"
        :show-filter-action="false"
        :show-create="false"
        :show-action-labels="false"
        :body-width="filterBodyWidth"
        embedded
        @clear="clearFilter"
      >
        <template #prepend>
          <div class="table-filter-prepend-field table-filter-prepend-field--md">
            <label class="filter-label table-filter-prepend-label">{{ t("purchase.fields.period") }}</label>
            <DatePicker
              v-model="filter.dates"
              selectionMode="range"
              dateFormat="dd/mm/yy"
              :placeholder="t('purchase.placeholders.selectPeriod')"
              showIcon
              size="small"
              class="w-full"
            />
          </div>
        </template>
      </TableFilter>
    </div>
    <div class="dashboard-kpis">
      <div class="kpi-card">
        <div class="kpi-label">{{ t("purchase.dashboard.totalExpense") }}</div>
        <div class="kpi-value text-primary">
          {{ formatCurrency(totalAmount) }}
        </div>
      </div>
    </div>
  </div>

  <Tabs v-model:value="selectedTabIndex">
    <TabList>
      <Tab value="0">
        <i :class="PrimeIcons.CHART_BAR" class="mr-2"></i>
        <span>{{ t("purchase.dashboard.charts") }}</span>
      </Tab>
      <Tab value="1">
        <i :class="PrimeIcons.LIST" class="mr-2"></i>
        <span>{{ t("purchase.dashboard.list") }}</span>
      </Tab>
    </TabList>
    <TabPanels>
      <TabPanel value="0">
        <div class="dashboard-container">
          <section class="dashboard-item">
            <header class="dashboard-item-header">
              {{ t("purchase.dashboard.monthlyExpenses") }}
            </header>
            <div class="dashboard-item-chart">
              <Chart
                v-if="chartData"
                type="bar"
                :data="chartData"
                :options="chartOptions"
              />
            </div>
          </section>
          <section class="dashboard-item">
            <header class="dashboard-item-header">
              {{ t("purchase.dashboard.expensesByType") }}
            </header>
            <div class="dashboard-item-chart">
              <Chart
                v-if="pieChartData"
                type="pie"
                :data="pieChartData"
                :options="pieChartOptions"
              />
            </div>
          </section>
        </div>
      </TabPanel>
      <TabPanel value="1">
        <TableConsolidatedExpenses
          scrollable
          scrollHeight="flex"
          :expenses="consolidatedExpenses"
        />
      </TabPanel>
    </TabPanels>
  </Tabs>
</template>
<script setup lang="ts">
import { computed, onMounted, ref, watch } from "vue";
import { useI18n } from "vue-i18n";
import Chart from "primevue/chart";
import { PrimeIcons } from "@primevue/core/api";
import { useStore } from "../../../store";
import { useSharedDataStore } from "../../shared/store/masterData";
import TableFilter, {
  type FilterBodyWidth,
  type FilterConfig,
} from "../../../components/tables/TableFilter.vue";
import _ from "lodash";
import {
  formatCurrency,
  formatDateForQueryParameter,
} from "../../../utils/functions";
import ExpenseServices from "../services";
import { ConsolidatedExpense, ExpenseType } from "../types";
import TableConsolidatedExpenses from "../components/TableConsolidatedExpenses.vue";
import { ChartOptions } from "../../../types/component";

const store = useStore();
const sharedDataStore = useSharedDataStore();
const { t, locale } = useI18n();
const selectedTabIndex = ref("0");

const currentYear = new Date().getFullYear();
const filter = ref({
  dates: [new Date(currentYear, 0, 1), new Date(currentYear, 11, 31)] as
    | Array<Date>
    | undefined,
  type: "" as string,
  typeDetail: "" as string,
});

const filterConfig = computed<Array<FilterConfig>>(() => [
  {
    key: "type",
    label: t("purchase.fields.type"),
    type: "select",
    options: [
      { label: t("purchase.dashboard.purchase"), value: "Compra" },
      { label: t("purchase.dashboard.expense"), value: "Despesa" },
    ],
    placeholder: t("purchase.placeholders.selectType"),
    size: "md",
  },
  {
    key: "typeDetail",
    label: t("purchase.dashboard.detail"),
    type: "select",
    options: (pieChartData.value?.labels ?? []).map((label) => ({
      label,
      value: label,
    })),
    placeholder: t("purchase.placeholders.selectDetail"),
    size: "md",
  },
]);

const filterBodyWidth: FilterBodyWidth = {
  desktop: "75%",
  tablet: "100%",
};

const expenseTypes = ref(undefined as Array<ExpenseType> | undefined);
const consolidatedExpenses = ref([] as Array<ConsolidatedExpense>);

const clearFilter = () => {
  filter.value.dates = undefined;
  filter.value.type = "";
  filter.value.typeDetail = "";
};

watch(
  () => filter.value.dates,
  (dates) => {
    if (dates && dates.length === 2 && dates[1]) {
      filterDashboard(false);
    }
  },
  { deep: true },
);

watch(
  () => filter.value.type,
  (newValue, oldValue) => {
    if (newValue === oldValue) return;
    filterDashboard(true);
  },
);

watch(
  () => filter.value.typeDetail,
  (newValue, oldValue) => {
    if (newValue === oldValue) return;
    filterDashboard(false);
  },
);

const setMenuTitle = () => {
  store.setMenuItem({
    icon: PrimeIcons.MONEY_BILL,
    title: t("purchase.dashboard.title"),
  });
};

onMounted(async () => {
  setMenuTitle();

  await sharedDataStore.fetchMasterData();
  expenseTypes.value = await ExpenseServices.ExpenseType.getAll();
  await filterDashboard(false);
});

const pieChartData = ref(undefined as undefined | ChartOptions);
const pieChartOptions = ref({
  plugins: {
    legend: {
      labels: {
        usePointStyle: true,
      },
    },
  },
});

const chartData = ref(undefined as undefined | ChartOptions);
const chartOptions = ref({
  scales: {
    y: {
      beginAtZero: true,
    },
  },
});

const totalAmount = computed((): number => {
  let amount = 0;
  if (consolidatedExpenses.value.length > 0) {
    consolidatedExpenses.value.forEach((e) => (amount += e.amount));
  }
  return amount;
});

const filterDashboard = async (clearDetail: boolean) => {
  if (
    filter.value.dates &&
    filter.value.dates.length === 2 &&
    filter.value.dates[1]
  ) {
    const startTime = formatDateForQueryParameter(filter.value.dates[0]);
    const endTime = formatDateForQueryParameter(filter.value.dates[1]);

    if (clearDetail) filter.value.typeDetail = "";

    const dataResponse = await ExpenseServices.Expense.getConsolidated(
      startTime,
      endTime,
      filter.value.type,
      filter.value.typeDetail,
    );
    if (dataResponse) consolidatedExpenses.value = dataResponse;

    chartData.value = transformConsolidatedExpensesToChartOptions(
      consolidatedExpenses.value,
      "monthPaymentKey",
    );
    pieChartData.value = transformConsolidatedExpensesToChartOptions(
      consolidatedExpenses.value,
      "typeDetail",
    );
  }
};

const transformConsolidatedExpensesToChartOptions = (
  expenses: Array<ConsolidatedExpense>,
  fieldToGroup: string,
): ChartOptions => {
  const options = {} as ChartOptions;

  const groupedData = _.groupBy(expenses, fieldToGroup);
  const sortedKeys = Object.keys(groupedData).sort((a, b) =>
    a.localeCompare(b, locale.value, { sensitivity: "base" }),
  );
  options.labels = sortedKeys;

  const chartColors = getChartColors(options.labels.length);
  options.datasets = [
    {
      label: t("purchase.dashboard.expenses"),
      data: [],
      backgroundColor: chartColors,
      borderColor: chartColors,
      borderWidth: 1,
    },
  ];

  sortedKeys.forEach((key) => {
    let totalAmount = 0;
    groupedData[key].forEach((mov) => {
      totalAmount += mov.amount;
    });

    options.datasets![0].data.push(totalAmount);
  });

  return options;
};

watch(locale, () => {
  setMenuTitle();
  chartData.value = transformConsolidatedExpensesToChartOptions(
    consolidatedExpenses.value,
    "monthPaymentKey",
  );
  pieChartData.value = transformConsolidatedExpensesToChartOptions(
    consolidatedExpenses.value,
    "typeDetail",
  );
});

const getChartColors = (numberOfColors: number): Array<string> => {
  const documentStyle = getComputedStyle(document.body);

  const colors = [
    documentStyle.getPropertyValue("--p-blue-400"),
    documentStyle.getPropertyValue("--p-green-400"),
    documentStyle.getPropertyValue("--p-yellow-400"),
    documentStyle.getPropertyValue("--p-cyan-400"),
    documentStyle.getPropertyValue("--p-pink-400"),
    documentStyle.getPropertyValue("--p-indigo-400"),
    documentStyle.getPropertyValue("--p-orange-400"),
    documentStyle.getPropertyValue("--p-purple-400"),
    documentStyle.getPropertyValue("--p-red-400"),
  ];

  const colorsToReturn = [];
  while (colorsToReturn.length < numberOfColors) {
    for (let i = 0; i < colors.length; i++) {
      colorsToReturn.push(colors[i]);
    }
  }
  return colorsToReturn;
};
</script>
<style scoped>
.dashboard-filter {
  display: flex;
  justify-content: space-between;
  align-items: flex-end;
  gap: 0.75rem;
  flex-wrap: wrap;
}

.dashboard-filter-left {
  display: flex;
  flex: 1 1 0;
  min-width: 22rem;
  align-self: center;
}

.dashboard-filter-left :deep(.table-filter) {
  width: 100%;
}

.dashboard-filter-left :deep(.table-filter__body--constrained) {
  width: var(--filter-body-max-desktop);
}

.dashboard-kpis {
  display: flex;
  flex-wrap: wrap;
  gap: 0.75rem;
  justify-content: flex-end;
}

.kpi-card {
  border: 1px solid var(--p-content-border-color);
  border-radius: 8px;
  padding: 0.75rem 1rem;
  background: var(--p-content-background);
}

.kpi-label {
  font-size: 0.85rem;
  color: var(--p-text-muted-color);
}

.kpi-value {
  font-size: 1.4rem;
  font-weight: 700;
}

.dashboard-container {
  display: grid;
  grid-template-columns: repeat(2, 1fr);
  gap: 2rem;
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
    display: grid;
    grid-template-columns: repeat(1, 1fr);
    grid-template-rows: repeat(2, 1fr);
    gap: 2rem;
    height: 70vh;
    width: 100%;
  }

  .dashboard-filter {
    align-items: stretch;
  }

  .dashboard-filter-left {
    min-width: 100%;
  }

  .dashboard-kpis {
    width: 100%;
    justify-content: flex-start;
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
</style>
