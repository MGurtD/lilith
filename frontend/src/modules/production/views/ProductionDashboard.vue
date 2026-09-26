<template>
  <DataTable
    :value="items"
    tableStyle="min-width: 100%"
    scrollable
    scrollHeight="flex"
    :loading="loading"
    paginator
    :rows="15"
    dataKey="id"
    v-model:filters="filters"
    :globalFilterFields="['code', 'referenceCode', 'referenceDescription']"
    @row-click="openWorkOrder"
    rowHover
  >
    <template #header>
      <TableFilter
        v-model="filterValues"
        :config="filterConfig"
        embedded
        :show-title="false"
        :show-action-labels="false"
        :show-create="false"
        :show-filter-action="false"
        :show-clear-action="false"
      >
        <template #prepend>
          <label class="block text-900 text-xl font-semibold"
            >{{ pt("Seguiment de marges i temps de producció") }}</label
          >
        </template>
      </TableFilter>
    </template>
    <template #empty>{{ t("production.detail.noWorkordersInProduction") }}</template>

    <Column field="code" header="OF" sortable />
    <Column :header="pt('Referència')" sortable field="referenceCode">
      <template #body="{ data }">
        <div class="flex flex-column">
          <span class="font-medium">{{ data.referenceCode }}</span>
          <span class="text-color-secondary text-sm">{{
            data.referenceDescription
          }}</span>
        </div>
      </template>
    </Column>
    <Column field="plannedQuantity" :header="pt('Quantitat')" sortable />
    <Column
      :header="pt('Avanç fases')"
      field="phaseProgressPercentage"
      sortable
      style="min-width: 13rem"
    >
      <template #body="{ data }">
        <div class="flex align-items-center gap-2">
          <ProgressBar
            :value="Math.min(data.phaseProgressPercentage, 100)"
            :showValue="false"
            :class="{ 'phase-overrun': data.phaseProgressPercentage > 100 }"
            style="height: 0.75rem; flex: 1"
          />
          <span class="text-sm white-space-nowrap">
            {{ data.phaseProgressPercentage }}%
          </span>
        </div>
      </template>
    </Column>
    <Column
      :header="pt('Avanç temps')"
      field="timeProgressPercentage"
      sortable
      style="min-width: 13rem"
    >
      <template #body="{ data }">
        <div class="flex align-items-center gap-2">
          <ProgressBar
            :value="Math.min(data.timeProgressPercentage, 100)"
            :showValue="false"
            :class="{ 'time-overrun': data.timeProgressPercentage > 100 }"
            style="height: 0.75rem; flex: 1"
          />
          <span
            v-tooltip.top="timeBreakdown(data)"
            class="text-sm white-space-nowrap cursor-help"
          >
            {{ data.timeProgressPercentage }}%
          </span>
        </div>
      </template>
    </Column>
    <Column field="orderPrice" :header="pt('Preu comanda')" sortable>
      <template #body="{ data }">{{ formatCurrency(data.orderPrice) }}</template>
    </Column>
    <Column field="theoreticalCost" :header="pt('Cost teòric')" sortable>
      <template #body="{ data }">{{
        formatCurrency(data.theoreticalCost)
      }}</template>
    </Column>
    <Column field="accumulatedTotalCost" :header="pt('Cost acumulat')" sortable>
      <template #body="{ data }">
        <span
          v-tooltip.top="costBreakdown(data)"
          class="cursor-help"
          >{{ formatCurrency(data.accumulatedTotalCost) }}</span
        >
      </template>
    </Column>
    <Column field="margin" :header="pt('Marge')" sortable>
      <template #body="{ data }">
        <Tag
          :value="formatCurrency(data.margin)"
          :severity="marginSeverity(data)"
        />
      </template>
    </Column>
  </DataTable>
</template>

<script setup lang="ts">
import { useI18n } from "vue-i18n";
const { t } = useI18n();
const pt = (key: string): string => t(`production.ui.${key}`);
import { computed, ref, onMounted } from "vue";
import { useRouter } from "vue-router";
import { useToast } from "primevue/usetoast";
import { PrimeIcons } from "@primevue/core/api";
import { FilterMatchMode } from "@primevue/core/api";
import { DataTableRowClickEvent } from "primevue/datatable";

import { useStore } from "@/store";
import TableFilter from "@/components/tables/TableFilter.vue";
import type { FilterConfig } from "@/components/tables/TableFilter.vue";
import { formatCurrency } from "../../../utils/functions";
import { WorkOrderDashboardItem } from "../types";
import { WorkOrderService } from "../services/workorder.service";

const router = useRouter();
const toast = useToast();
const store = useStore();

const workOrderService = new WorkOrderService("/WorkOrder");

const items = ref<Array<WorkOrderDashboardItem>>([]);
const loading = ref(false);

const filters = ref({
  global: { value: null as string | null, matchMode: FilterMatchMode.CONTAINS },
});

const filterConfig = computed<FilterConfig[]>(() => [
  {
    key: "global",
    label: t("common.search"),
    type: "text",
    placeholder: pt("Cercar OF o referència"),
    size: "lg",
  },
]);

// The filter bar edits the DataTable's global filter directly, so the table
// keeps filtering as the user types.
const filterValues = computed({
  get: () => ({ global: filters.value.global.value }),
  set: (values: { global?: string | null }) => {
    filters.value.global.value = values.global ?? null;
  },
});

const costBreakdown = (data: WorkOrderDashboardItem) =>
  `Material: ${formatCurrency(data.accumulatedMaterialCost)}\n` +
  `Màquina: ${formatCurrency(data.accumulatedMachineCost)}\n` +
  `Operari: ${formatCurrency(data.accumulatedOperatorCost)}\n` +
  `Serveis externs: ${formatCurrency(data.accumulatedExternalCost)}`;

const timeBreakdown = (data: WorkOrderDashboardItem) =>
  `Temps real: ${data.actualTimeMinutes.toFixed(1)} min\n` +
  `Temps teòric: ${data.theoreticalTimeMinutes.toFixed(1)} min`;

const marginSeverity = (data: WorkOrderDashboardItem) => {
  if (data.margin < 0) return "danger";
  if (data.margin === 0) return "warn";
  return "success";
};

const openWorkOrder = (event: DataTableRowClickEvent) => {
  const item = event.data as WorkOrderDashboardItem;
  router.push({ path: `/workorder/${item.id}` });
};

const loadData = async () => {
  loading.value = true;
  try {
    items.value = (await workOrderService.GetDashboardData()) ?? [];
  } catch (error) {
    console.error("Error loading production dashboard:", error);
    toast.add({
      severity: "error",
      summary: pt("Error al carregar el dashboard de producció"),
      life: 5000,
    });
  } finally {
    loading.value = false;
  }
};

onMounted(async () => {
  store.setMenuItem({
    icon: PrimeIcons.CHART_LINE,
    title: pt("Dashboard de producció"),
  });
  await loadData();
});
</script>

<style scoped>
:deep(.time-overrun .p-progressbar-value) {
  background: var(--red-500);
}

:deep(.phase-overrun .p-progressbar-value) {
  background: var(--red-500);
}
</style>
