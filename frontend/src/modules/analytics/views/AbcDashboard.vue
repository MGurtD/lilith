<template>
  <div class="dashboard-filter">
    <div class="dashboard-filter-left">
      <TableFilter
        :config="[]"
        v-model="filter"
        :show-title="false"
        :show-filter-action="true"
        :show-create="false"
        :show-action-labels="true"
        :body-width="filterBodyWidth"
        embedded
        @filter="load"
        @clear="clearFilter"
      >
        <template #prepend>
          <div class="table-filter-prepend-field table-filter-prepend-field--md">
            <label class="filter-label table-filter-prepend-label">
              {{ t("common.period") }}
            </label>
            <DatePicker
              v-model="filter.dates"
              selectionMode="range"
              dateFormat="dd/mm/yy"
              :placeholder="t('analytics.abc.filters.periodPlaceholder')"
              showIcon
              size="small"
              class="w-full"
            />
          </div>
        </template>
      </TableFilter>
    </div>
  </div>

  <Tabs value="0">
    <TabList>
      <Tab value="0">{{ t("analytics.abc.tabs.chart") }}</Tab>
      <Tab value="1">{{ t("analytics.abc.tabs.data") }}</Tab>
    </TabList>
    <TabPanels>
      <TabPanel value="0">
        <div class="abc-legend">
          <span
            v-for="cat in result?.categories ?? []"
            :key="cat.category"
            class="abc-legend-item"
          >
            <span class="abc-dot" :style="{ background: categoryColor(cat.category) }"></span>
            {{ cat.category }} · {{ cat.itemCount }} ({{ cat.itemPercent }}%) ·
            {{ formatCurrency(cat.value) }} ({{ cat.valuePercent }}%)
          </span>
        </div>
        <div class="abc-chart">
          <Chart
            v-if="(result?.rows?.length ?? 0) > 0"
            type="bar"
            :data="chartData"
            :options="chartOptions"
            class="w-full"
            style="height: 100%"
          />
          <div v-else class="text-center text-gray-500">
            {{ t("analytics.abc.noData") }}
          </div>
        </div>
      </TabPanel>

      <TabPanel value="1">
        <Table
          preset="read-only"
          :columns="columns"
          :items="result?.rows ?? []"
          :show-filters="false"
          :show-create="false"
          :paginator="true"
          :rows="25"
          scrollable
          scroll-height="flex"
          sort-field="rank"
          :sort-order="1"
          class="small-datatable"
          tableStyle="min-width: 100%"
        >
          <template #body-name="{ data }">
            <a class="stats-cell-link" @click.stop="go(`${detailRoute}/${data.entityId}`)">
              {{ data.name }}
            </a>
          </template>
          <template #body-valuePercent="{ data }">{{ data.valuePercent }} %</template>
          <template #body-cumulativePercent="{ data }">
            {{ data.cumulativePercent }} %
          </template>
          <template #body-category="{ data }">
            <span
              class="abc-badge"
              :style="{ background: categoryColor(data.category) }"
            >
              {{ data.category }}
            </span>
          </template>
        </Table>
      </TabPanel>
    </TabPanels>
  </Tabs>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, watch } from "vue";
import { useRouter } from "vue-router";
import { useI18n } from "vue-i18n";
import { PrimeIcons } from "@primevue/core/api";
import TableFilter, {
  type FilterBodyWidth,
} from "@/components/tables/TableFilter.vue";
import Table from "@/components/tables/Table.vue";
import { ColumnType, type Column } from "@/components/tables/types";
import { formatCurrency, formatDateForQueryParameter } from "@/utils/functions";
import { useStore } from "@/store";
import { AbcAnalysisService } from "../services/abcAnalysis.service";
import type { AbcAnalysisResult } from "../types";

const props = defineProps<{ mode: "customer" | "supplier" }>();

const router = useRouter();
const { t, locale } = useI18n();
const store = useStore();
const service = new AbcAnalysisService("/abcanalysis");

const currentYear = new Date().getFullYear();
const filter = ref({
  dates: [new Date(currentYear, 0, 1), new Date(currentYear, 11, 31)] as
    | Array<Date>
    | undefined,
});
const filterBodyWidth: FilterBodyWidth = { desktop: "28rem", tablet: "32rem" };
const result = ref<AbcAnalysisResult>();

const isCustomer = computed(() => props.mode === "customer");
const detailRoute = computed(() => (isCustomer.value ? "/customers" : "/suppliers"));
const nameHeader = computed(() =>
  isCustomer.value
    ? t("analytics.abc.headers.customer")
    : t("analytics.abc.headers.supplier"),
);

const categoryColor = (category: string) =>
  category === "A" ? "#ef4444" : category === "B" ? "#f59e0b" : "#22c55e";

const columns = computed<Column[]>(() => [
  { field: "rank", header: t("analytics.abc.columns.rank"), columnType: ColumnType.Number },
  { field: "code", header: t("analytics.abc.columns.code") },
  { field: "name", header: nameHeader.value, sortable: true },
  { field: "value", header: t("analytics.abc.columns.value"), columnType: ColumnType.Currency, sortable: true },
  { field: "valuePercent", header: t("analytics.abc.columns.valuePercent"), columnType: ColumnType.Number },
  { field: "cumulativePercent", header: t("analytics.abc.columns.cumulativePercent"), columnType: ColumnType.Number },
  { field: "category", header: t("analytics.abc.columns.category"), sortable: true },
]);

const chartData = computed(() => {
  const rows = result.value?.rows ?? [];
  return {
    labels: rows.map((r) => r.name || r.code),
    datasets: [
      {
        type: "bar",
        label: t("analytics.abc.chart.value"),
        yAxisID: "y",
        data: rows.map((r) => r.value),
        backgroundColor: rows.map((r) => categoryColor(r.category)),
        order: 1,
      },
      {
        type: "line",
        label: t("analytics.abc.chart.cumulative"),
        yAxisID: "y1",
        data: rows.map((r) => r.cumulativePercent),
        borderColor: "#3b82f6",
        backgroundColor: "#3b82f6",
        tension: 0.2,
        pointRadius: 0,
        order: 0,
      },
    ],
  };
});

const chartOptions = computed(() => ({
  maintainAspectRatio: false,
  interaction: { mode: "index", intersect: false },
  scales: {
    y: {
      position: "left",
      title: { display: true, text: t("analytics.abc.chart.value") },
    },
    y1: {
      position: "right",
      min: 0,
      max: 100,
      grid: { drawOnChartArea: false },
      title: { display: true, text: t("analytics.abc.chart.cumulative") },
    },
    x: {
      ticks: { display: (result.value?.rows?.length ?? 0) <= 40 },
    },
  },
  plugins: { legend: { display: true } },
}));

const go = (path: string) => router.push({ path });

const setMenuItem = () => {
  store.setMenuItem({
    icon: PrimeIcons.SORT_AMOUNT_DOWN,
    title: isCustomer.value
      ? t("analytics.abc.customersTitle")
      : t("analytics.abc.suppliersTitle"),
  });
};

const load = async () => {
  const dates = filter.value.dates;
  if (!dates || dates.length !== 2 || !dates[1]) return;
  const start = formatDateForQueryParameter(dates[0]);
  const end = formatDateForQueryParameter(dates[1]);
  result.value = isCustomer.value
    ? await service.GetCustomers(start, end)
    : await service.GetSuppliers(start, end);
};

const clearFilter = () => {
  filter.value.dates = [
    new Date(currentYear, 0, 1),
    new Date(currentYear, 11, 31),
  ];
  load();
};

watch(
  () => filter.value.dates,
  (dates) => {
    if (dates && dates.length === 2 && dates[1]) load();
  },
  { deep: true },
);

watch([locale, () => props.mode], setMenuItem);

onMounted(async () => {
  setMenuItem();
  await load();
});
</script>

<style scoped>
.dashboard-filter {
  display: flex;
  gap: 0.75rem;
  align-items: flex-end;
  justify-content: flex-start;
  flex-wrap: wrap;
  margin-bottom: 1rem;
}
.dashboard-filter-left {
  flex: 0 1 auto;
  min-width: 20rem;
}
.abc-chart {
  height: calc(100vh - var(--top-panel-height) - 12rem);
  min-height: 22rem;
}
.abc-legend {
  display: flex;
  flex-wrap: wrap;
  gap: 1rem;
  margin: 0.5rem 0 0.75rem;
  font-size: 0.85rem;
}
.abc-legend-item {
  display: inline-flex;
  align-items: center;
  gap: 0.4rem;
}
.abc-dot {
  width: 0.8rem;
  height: 0.8rem;
  border-radius: 50%;
  display: inline-block;
}
.abc-badge {
  display: inline-block;
  min-width: 1.4rem;
  padding: 0.1rem 0.45rem;
  border-radius: 4px;
  color: #fff;
  font-weight: 700;
  text-align: center;
}
.stats-cell-link {
  color: var(--p-primary-color);
  cursor: pointer;
  text-decoration: underline;
}
</style>
