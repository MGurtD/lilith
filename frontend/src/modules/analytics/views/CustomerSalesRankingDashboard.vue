<template>
  <div class="dashboard-filter">
    <div class="dashboard-filter-field">
      <label>{{ t("analytics.customerRanking.filters.year") }}</label>
      <Select
        v-model="selectedYear"
        :options="availableYears"
        :placeholder="t('analytics.customerRanking.filters.yearPlaceholder')"
        @change="loadRankingData"
      />
    </div>
    <div class="dashboard-filter-field">
      <label>{{ t("analytics.customerRanking.filters.aggregation") }}</label>
      <Select
        v-model="aggregationType"
        :options="aggregationOptions"
        optionLabel="label"
        optionValue="value"
        :placeholder="
          t('analytics.customerRanking.filters.aggregationPlaceholder')
        "
        @change="prepareData"
      />
    </div>
    <Button
      class="grid_add_row_button"
      :icon="PrimeIcons.FILTER_SLASH"
      @click="clearFilter"
      severity="secondary"
    />
    <div class="kpi-card">
      <div class="kpi-label">
        {{ t("analytics.customerRanking.kpi.totalInvoices") }}
      </div>
      <div class="kpi-value text-purple-500">
        {{ totalInvoices }}
      </div>
    </div>
    <div class="kpi-card">
      <div class="kpi-label">
        {{ t("analytics.customerRanking.kpi.totalSales") }}
      </div>
      <div class="kpi-value text-green-500">
        {{ formatCurrency(totalSales) }}
      </div>
    </div>
  </div>

  <Tabs value="0">
    <TabList>
      <Tab value="0">{{ t("analytics.customerRanking.tabs.chart") }}</Tab>
      <Tab value="1">{{ t("analytics.customerRanking.tabs.data") }}</Tab>
    </TabList>
    <TabPanels>
      <TabPanel value="0">
        <div class="dashboard-container">
          <div
            class="chart-area"
            style="
              display: flex;
              justify-content: center;
              align-items: center;
              min-height: 400px;
            "
          >
            <Chart
              v-if="pieChartData && pieChartData.labels.length > 0"
              type="pie"
              :data="pieChartData"
              :options="pieChartOptions"
              style="width: 60%; max-width: 600px"
            />
            <div v-else class="text-center text-gray-500">
              {{ t("analytics.customerRanking.table.noData") }}
            </div>
          </div>
        </div>
      </TabPanel>

      <TabPanel value="1">
        <DataTable
          :value="tableData"
          class="small-datatable"
          tableStyle="min-width: 100%"
          scrollable
          scrollHeight="flex"
          sortField="totalSales"
          :sortOrder="-1"
        >
          <Column
            field="customerName"
            :header="t('analytics.customerRanking.table.customer')"
            style="width: 25%"
            sortable
            frozen
          />
          <Column
            v-for="period in dynamicPeriods"
            :key="period.key"
            :field="period.key"
            :header="period.label"
            style="width: auto"
            sortable
          >
            <template #body="slotProps">
              <span v-if="slotProps.data[period.key]">
                {{ formatCurrency(slotProps.data[period.key]) }}
              </span>
              <span v-else class="text-gray-400">-</span>
            </template>
          </Column>
          <Column
            field="totalSales"
            :header="t('analytics.customerRanking.table.total')"
            style="width: 15%"
            sortable
          >
            <template #body="slotProps">
              <span class="font-semibold text-green-600">
                {{ formatCurrency(slotProps.data.totalSales) }}
              </span>
            </template>
          </Column>
        </DataTable>
      </TabPanel>
    </TabPanels>
  </Tabs>
</template>

<script setup lang="ts">
import { ref, onMounted, computed } from "vue";
import { useStore } from "../../../store";
import { useToast } from "primevue/usetoast";
import { PrimeIcons } from "@primevue/core/api";
import { formatCurrency } from "../../../utils/functions";
import { useI18n } from "vue-i18n";

import { CustomerSalesRanking } from "../types";
import { CustomerRankingService } from "../services/customerRanking.service";

const store = useStore();
const toast = useToast();
const { t } = useI18n();
const customerRankingService = new CustomerRankingService("/customerranking");

const selectedYear = ref(new Date().getFullYear());
const aggregationType = ref<"monthly" | "quarterly">("monthly");
const rawRankings = ref<Array<CustomerSalesRanking>>([]);

const aggregationOptions = [
  { label: t("analytics.customerRanking.filters.monthly"), value: "monthly" },
  {
    label: t("analytics.customerRanking.filters.quarterly"),
    value: "quarterly",
  },
];

const availableYears = computed(() => {
  const currentYear = new Date().getFullYear();
  const years = [];
  for (let i = currentYear; i >= currentYear - 10; i--) {
    years.push(i);
  }
  return years;
});

onMounted(async () => {
  store.setMenuItem({
    icon: PrimeIcons.CHART_PIE,
    title: t("analytics.customerRanking.title"),
  });

  await loadRankingData();
});

const loadRankingData = async () => {
  if (!selectedYear.value) {
    toast.add({
      severity: "warn",
      summary: t("analytics.customerRanking.messages.selectYear"),
      life: 4000,
    });
    return;
  }

  try {
    const response = await customerRankingService.GetAnnualRanking(
      selectedYear.value,
    );
    if (response) {
      rawRankings.value = response;
      prepareData();
    } else {
      rawRankings.value = [];
      toast.add({
        severity: "info",
        summary: t("analytics.customerRanking.messages.noDataFound", {
          year: selectedYear.value,
        }),
        life: 4000,
      });
    }
  } catch (error) {
    console.error("Error loading customer ranking:", error);
    toast.add({
      severity: "error",
      summary: t("analytics.customerRanking.messages.loadError"),
      life: 5000,
    });
  }
};

const clearFilter = () => {
  selectedYear.value = new Date().getFullYear();
  aggregationType.value = "monthly";
  loadRankingData();
};

// KPIs
const totalSales = computed(() => {
  return rawRankings.value.reduce((sum, r) => sum + r.totalSales, 0);
});

const uniqueCustomersCount = computed(() => {
  const uniqueIds = new Set(rawRankings.value.map((r) => r.customerId));
  return uniqueIds.size;
});

const totalInvoices = computed(() => {
  return rawRankings.value.reduce((sum, r) => sum + r.invoiceCount, 0);
});

const averageSalePerCustomer = computed(() => {
  return uniqueCustomersCount.value > 0
    ? totalSales.value / uniqueCustomersCount.value
    : 0;
});

// Dynamic periods
interface Period {
  key: string;
  label: string;
}

const dynamicPeriods = ref<Period[]>([]);

// Pie Chart
const pieChartData = ref<any>(null);
const pieChartOptions = ref({
  plugins: {
    legend: {
      position: "right",
      labels: {
        usePointStyle: true,
        padding: 15,
        font: {
          size: 11,
        },
      },
    },
    tooltip: {
      callbacks: {
        label: function (context: any) {
          const label = context.label || "";
          const value = context.parsed || 0;
          const formatted = new Intl.NumberFormat("de-DE", {
            style: "currency",
            currency: "EUR",
          }).format(value);
          return `${label}: ${formatted}`;
        },
      },
    },
  },
  maintainAspectRatio: true,
  responsive: true,
});

// Table Data
interface TableRow {
  customerId: string;
  customerName: string;
  totalSales: number;
  [key: string]: any; // Dynamic period columns
}

const tableData = ref<TableRow[]>([]);

const prepareData = () => {
  if (!rawRankings.value || rawRankings.value.length === 0) {
    pieChartData.value = null;
    tableData.value = [];
    dynamicPeriods.value = [];
    return;
  }

  // Group by customer
  const customerMap = new Map<string, TableRow>();

  rawRankings.value.forEach((ranking) => {
    const key = ranking.customerId;

    if (!customerMap.has(key)) {
      customerMap.set(key, {
        customerId: ranking.customerId,
        customerName: ranking.customerName,
        totalSales: 0,
      });
    }

    const row = customerMap.get(key)!;
    row.totalSales += ranking.totalSales;

    // Add period column
    if (aggregationType.value === "monthly") {
      const monthKey = `month-${ranking.month}`;
      row[monthKey] = (row[monthKey] || 0) + ranking.totalSales;
    } else {
      const quarterKey = `quarter-${ranking.quarter}`;
      row[quarterKey] = (row[quarterKey] || 0) + ranking.totalSales;
    }
  });

  // Generate dynamic periods
  if (aggregationType.value === "monthly") {
    const monthKeys = [
      "january",
      "february",
      "march",
      "april",
      "may",
      "june",
      "july",
      "august",
      "september",
      "october",
      "november",
      "december",
    ];
    dynamicPeriods.value = monthKeys.map((key, index) => ({
      key: `month-${index + 1}`,
      label: t(`analytics.customerRanking.periods.${key}`),
    }));
  } else {
    dynamicPeriods.value = [
      {
        key: "quarter-1",
        label: t("analytics.customerRanking.periods.quarter1"),
      },
      {
        key: "quarter-2",
        label: t("analytics.customerRanking.periods.quarter2"),
      },
      {
        key: "quarter-3",
        label: t("analytics.customerRanking.periods.quarter3"),
      },
      {
        key: "quarter-4",
        label: t("analytics.customerRanking.periods.quarter4"),
      },
    ];
  }

  // Sort by total sales descending
  tableData.value = Array.from(customerMap.values()).sort(
    (a, b) => b.totalSales - a.totalSales,
  );

  // Prepare pie chart (top 10 customers)
  const top10 = tableData.value.slice(0, 10);
  const othersTotal = tableData.value
    .slice(10)
    .reduce((sum, row) => sum + row.totalSales, 0);

  const labels = top10.map((row) => row.customerName);
  const data = top10.map((row) => row.totalSales);

  if (othersTotal > 0) {
    labels.push(t("analytics.customerRanking.periods.others"));
    data.push(othersTotal);
  }

  pieChartData.value = {
    labels: labels,
    datasets: [
      {
        data: data,
        backgroundColor: [
          "#42A5F5",
          "#66BB6A",
          "#FFA726",
          "#EC407A",
          "#AB47BC",
          "#26A69A",
          "#FFCA28",
          "#FF7043",
          "#8D6E63",
          "#78909C",
          "#BDBDBD",
        ],
      },
    ],
  };
};
</script>

<style scoped>
.dashboard-filter {
  display: flex;
  align-items: flex-end;
  gap: 1rem;
  margin-bottom: 1rem;
  background: var(--p-content-background);
  border: 1px solid var(--p-content-border-color);
  border-radius: 8px;
}

.dashboard-filter-field {
  display: flex;
  flex-direction: column;
  gap: 0.25rem;
}

.dashboard-filter-field label {
  font-size: 0.875rem;
  font-weight: 500;
  color: var(--p-text-color);
}

.kpi-card {
  border: 1px solid var(--p-content-border-color);
  border-radius: 8px;
  padding: 0.75rem 1rem;
  background: var(--p-content-background);
}

.kpi-label {
  font-size: 0.875rem;
  color: var(--p-text-secondary-color);
  margin-bottom: 0.5rem;
}

.kpi-value {
  font-size: 1.5rem;
  font-weight: 700;
}

.dashboard-container {
  padding: 1rem;
  background: var(--p-content-background);
}

.chart-area {
  width: 100%;
  height: 500px;
}
</style>
