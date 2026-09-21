<template>
  <StatsDashboard
    :columns="columns"
    :items="result?.rows ?? []"
    :kpis="kpis"
    v-model:filter-values="filter"
    :filter-body-width="filterBodyWidth"
    sort-field="workOrderCode"
    :sort-order="1"
    @filter="load"
    @clear="clearFilter"
  >
    <template #filter>
      <div class="table-filter-prepend-field table-filter-prepend-field--md">
        <label class="filter-label table-filter-prepend-label">
          {{ t("common.period") }}
        </label>
        <DatePicker
          v-model="filter.dates"
          selectionMode="range"
          dateFormat="dd/mm/yy"
          :placeholder="t('analytics.productionDeviation.filters.periodPlaceholder')"
          showIcon
          size="small"
          class="w-full"
        />
      </div>
    </template>

    <!-- Clickable work order -->
    <template #body-workOrderCode="{ data }">
      <a class="stats-cell-link" @click.stop="go(`/workorder/${data.workOrderId}`)">
        {{ data.workOrderCode }}
      </a>
    </template>
    <!-- Colored deviations -->
    <template #body-machineDeviation="{ data }">
      <span :class="data.machineDeviation > 0 ? 'text-red-500' : 'text-green-600'">
        {{ data.machineDeviation }}
      </span>
    </template>
    <template #body-operatorDeviation="{ data }">
      <span :class="data.operatorDeviation > 0 ? 'text-red-500' : 'text-green-600'">
        {{ data.operatorDeviation }}
      </span>
    </template>
  </StatsDashboard>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, watch } from "vue";
import { useRouter } from "vue-router";
import { useI18n } from "vue-i18n";
import { PrimeIcons } from "@primevue/core/api";
import StatsDashboard, { type StatKpi } from "@/components/StatsDashboard.vue";
import { ColumnType, type Column } from "@/components/tables/types";
import type { FilterBodyWidth } from "@/components/tables/TableFilter.vue";
import { formatDateForQueryParameter } from "@/utils/functions";
import { useStore } from "@/store";
import { ProductionTimeDeviationService } from "../services/productionTimeDeviation.service";
import type { ProductionTimeDeviationResult } from "../types";

const router = useRouter();
const { t, locale } = useI18n();
const store = useStore();
const service = new ProductionTimeDeviationService("/productiontimedeviation");

const currentYear = new Date().getFullYear();
const filter = ref({
  dates: [new Date(currentYear, 0, 1), new Date(currentYear, 11, 31)] as
    | Array<Date>
    | undefined,
});
const filterBodyWidth: FilterBodyWidth = { desktop: "28rem", tablet: "32rem" };
const result = ref<ProductionTimeDeviationResult>();

const min = (value?: number) => `${value ?? 0} min`;

const columns = computed<Column[]>(() => [
  {
    field: "workOrderCode",
    header: t("analytics.productionDeviation.columns.workOrder"),
    sortable: true,
  },
  {
    field: "phaseName",
    header: t("analytics.productionDeviation.columns.phase"),
    sortable: true,
  },
  {
    field: "statusName",
    header: t("analytics.productionDeviation.columns.status"),
    sortable: true,
  },
  {
    field: "quantity",
    header: t("analytics.productionDeviation.columns.quantity"),
    columnType: ColumnType.Number,
  },
  {
    field: "theoreticalMachineTime",
    header: t("analytics.productionDeviation.columns.theoreticalMachine"),
    columnType: ColumnType.Number,
  },
  {
    field: "realMachineTime",
    header: t("analytics.productionDeviation.columns.realMachine"),
    columnType: ColumnType.Number,
  },
  {
    field: "machineDeviation",
    header: t("analytics.productionDeviation.columns.machineDeviation"),
    columnType: ColumnType.Number,
  },
  {
    field: "theoreticalOperatorTime",
    header: t("analytics.productionDeviation.columns.theoreticalOperator"),
    columnType: ColumnType.Number,
  },
  {
    field: "realOperatorTime",
    header: t("analytics.productionDeviation.columns.realOperator"),
    columnType: ColumnType.Number,
  },
  {
    field: "operatorDeviation",
    header: t("analytics.productionDeviation.columns.operatorDeviation"),
    columnType: ColumnType.Number,
  },
]);

const kpis = computed<StatKpi[]>(() => [
  {
    label: t("analytics.productionDeviation.kpi.theoreticalMachine"),
    value: min(result.value?.theoreticalMachineTime),
  },
  {
    label: t("analytics.productionDeviation.kpi.realMachine"),
    value: min(result.value?.realMachineTime),
  },
  {
    label: t("analytics.productionDeviation.kpi.machineDeviation"),
    value: `${result.value?.machineDeviationPercent ?? 0} %`,
    colorClass:
      (result.value?.machineDeviationPercent ?? 0) > 0
        ? "text-red-500"
        : "text-green-600",
  },
  {
    label: t("analytics.productionDeviation.kpi.theoreticalOperator"),
    value: min(result.value?.theoreticalOperatorTime),
  },
  {
    label: t("analytics.productionDeviation.kpi.realOperator"),
    value: min(result.value?.realOperatorTime),
  },
  {
    label: t("analytics.productionDeviation.kpi.operatorDeviation"),
    value: `${result.value?.operatorDeviationPercent ?? 0} %`,
    colorClass:
      (result.value?.operatorDeviationPercent ?? 0) > 0
        ? "text-red-500"
        : "text-green-600",
  },
]);

const go = (path: string) => router.push({ path });

const setMenuItem = () => {
  store.setMenuItem({
    icon: PrimeIcons.CLOCK,
    title: t("analytics.productionDeviation.title"),
  });
};

const load = async () => {
  const dates = filter.value.dates;
  if (!dates || dates.length !== 2 || !dates[1]) return;
  result.value = await service.GetDeviation(
    formatDateForQueryParameter(dates[0]),
    formatDateForQueryParameter(dates[1]),
  );
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

watch(locale, setMenuItem);

onMounted(async () => {
  setMenuItem();
  await load();
});
</script>

<style scoped>
.stats-cell-link {
  color: var(--p-primary-color);
  cursor: pointer;
  text-decoration: underline;
}
</style>
