<template>
  <StatsDashboard
    :columns="columns"
    :items="result?.rows ?? []"
    :kpis="kpis"
    v-model:filter-values="filter"
    :filter-body-width="filterBodyWidth"
    sort-field="budgetNumber"
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
          :placeholder="t('analytics.budgetConversion.filters.periodPlaceholder')"
          showIcon
          size="small"
          class="w-full"
        />
      </div>
      <div class="table-filter-prepend-field table-filter-prepend-field--md">
        <label class="filter-label table-filter-prepend-label">
          {{ t("common.customer") }}
        </label>
        <DropdownCustomers label="" v-model="filter.customerId" />
      </div>
    </template>

    <!-- Clickable customer -->
    <template #body-customerName="{ data }">
      <a class="stats-cell-link" @click.stop="go(`/customers/${data.customerId}`)">
        {{ data.customerName }}
      </a>
    </template>
    <!-- Clickable budget code -->
    <template #body-budgetNumber="{ data }">
      <a class="stats-cell-link" @click.stop="go(`/budget/${data.budgetId}`)">
        {{ data.budgetNumber }}
      </a>
    </template>
    <!-- Clickable order code -->
    <template #body-orderNumber="{ data }">
      <a
        v-if="data.orderId"
        class="stats-cell-link"
        @click.stop="go(`/salesorder/${data.orderId}`)"
      >
        {{ data.orderNumber }}
      </a>
      <span v-else class="text-gray-400">-</span>
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
import {
  formatDateForQueryParameter,
  formatCurrency,
} from "@/utils/functions";
import { useStore } from "@/store";
import { useLifecyclesStore } from "../../shared/store/lifecycle";
import DropdownCustomers from "../../sales/components/DropdownCustomers.vue";
import { BudgetConversionService } from "../services/budgetConversion.service";
import type { BudgetConversionResult } from "../types";

const router = useRouter();
const { t, locale } = useI18n();
const store = useStore();
const lifecycleStore = useLifecyclesStore();
const service = new BudgetConversionService("/budgetconversion");

const currentYear = new Date().getFullYear();
const filter = ref({
  dates: [new Date(currentYear, 0, 1), new Date(currentYear, 11, 31)] as
    | Array<Date>
    | undefined,
  customerId: undefined as string | undefined,
});
const filterBodyWidth: FilterBodyWidth = { desktop: "28rem", tablet: "32rem" };
const result = ref<BudgetConversionResult>();

const columns = computed<Column[]>(() => [
  { field: "customerName", header: t("common.customer"), sortable: true },
  {
    field: "budgetNumber",
    header: t("analytics.budgetConversion.columns.budgetNumber"),
    sortable: true,
  },
  {
    field: "budgetDate",
    header: t("analytics.budgetConversion.columns.budgetDate"),
    columnType: ColumnType.Date,
  },
  {
    field: "statusId",
    header: t("common.status"),
    columnType: ColumnType.Lookup,
    resolver: lifecycleStore.getStatusNameById,
    sortable: true,
  },
  {
    field: "orderNumber",
    header: t("analytics.budgetConversion.columns.orderNumber"),
    sortable: true,
  },
  {
    field: "orderDate",
    header: t("analytics.budgetConversion.columns.orderDate"),
    columnType: ColumnType.Date,
  },
  {
    field: "daysToConversion",
    header: t("analytics.budgetConversion.columns.daysToConversion"),
    columnType: ColumnType.Number,
  },
  { field: "amount", header: t("common.amount"), columnType: ColumnType.Currency },
]);

const kpis = computed<StatKpi[]>(() => [
  {
    label: t("analytics.budgetConversion.kpi.totalBudgets"),
    value: result.value?.totalBudgets ?? 0,
  },
  {
    label: t("analytics.budgetConversion.kpi.totalOrders"),
    value: result.value?.totalOrders ?? 0,
  },
  {
    label: t("analytics.budgetConversion.kpi.conversionRate"),
    value: `${result.value?.conversionRate ?? 0} %`,
    colorClass: "text-green-500",
  },
  {
    label: t("analytics.budgetConversion.kpi.avgAcceptanceDays"),
    value: result.value?.averageAcceptanceDays ?? 0,
  },
  {
    label: t("analytics.budgetConversion.kpi.totalBudgetAmount"),
    value: formatCurrency(result.value?.totalBudgetAmount ?? 0),
  },
  {
    label: t("analytics.budgetConversion.kpi.totalConvertedAmount"),
    value: formatCurrency(result.value?.totalConvertedAmount ?? 0),
    colorClass: "text-green-600",
  },
]);

const go = (path: string) => router.push({ path });

const setMenuItem = () => {
  store.setMenuItem({
    icon: PrimeIcons.SYNC,
    title: t("analytics.budgetConversion.title"),
  });
};

const load = async () => {
  const dates = filter.value.dates;
  if (!dates || dates.length !== 2 || !dates[1]) return;
  result.value = await service.GetConversion(
    formatDateForQueryParameter(dates[0]),
    formatDateForQueryParameter(dates[1]),
    filter.value.customerId,
  );
};

const clearFilter = () => {
  filter.value.dates = [
    new Date(currentYear, 0, 1),
    new Date(currentYear, 11, 31),
  ];
  filter.value.customerId = undefined;
  load();
};

watch(
  () => [filter.value.dates, filter.value.customerId],
  ([dates]) => {
    if (dates && (dates as Array<Date>).length === 2 && (dates as Array<Date>)[1]) load();
  },
  { deep: true },
);

watch(locale, setMenuItem);

onMounted(async () => {
  setMenuItem();
  await lifecycleStore.fetchOneByName("Budget");
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
