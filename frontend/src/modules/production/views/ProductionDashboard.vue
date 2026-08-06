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
      <div
        class="flex flex-wrap align-items-center justify-content-between gap-2"
      >
        <label class="block text-900 text-xl font-semibold"
          >Seguiment de marges i temps de producció</label
        >
        <IconField iconPosition="left">
          <InputIcon class="pi pi-search" />
          <InputText
            v-model="filters['global'].value"
            placeholder="Cercar OF o referència"
          />
        </IconField>
      </div>
    </template>
    <template #empty>Sense ordres de fabricació en producció.</template>

    <Column field="code" header="OF" sortable />
    <Column header="Referència" sortable field="referenceCode">
      <template #body="{ data }">
        <div class="flex flex-column">
          <span class="font-medium">{{ data.referenceCode }}</span>
          <span class="text-color-secondary text-sm">{{
            data.referenceDescription
          }}</span>
        </div>
      </template>
    </Column>
    <Column field="plannedQuantity" header="Quantitat" sortable />
    <ProgressColumn
      header="Avanç fases"
      field="phaseProgressPercentage"
      style="min-width: 13rem"
    />
    <ProgressColumn
      header="Avanç temps"
      field="timeProgressPercentage"
      :tooltip="timeBreakdown"
      style="min-width: 13rem"
    />
    <Column field="orderPrice" header="Preu comanda" sortable>
      <template #body="{ data }">{{ formatCurrency(data.orderPrice) }}</template>
    </Column>
    <Column field="theoreticalCost" header="Cost teòric" sortable>
      <template #body="{ data }">{{
        formatCurrency(data.theoreticalCost)
      }}</template>
    </Column>
    <Column field="accumulatedTotalCost" header="Cost acumulat" sortable>
      <template #body="{ data }">
        <span
          v-tooltip.top="costBreakdown(data)"
          class="cursor-help"
          >{{ formatCurrency(data.accumulatedTotalCost) }}</span
        >
      </template>
    </Column>
    <Column field="margin" header="Marge" sortable>
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
import { ref, onMounted } from "vue";
import { useRouter } from "vue-router";
import { useToast } from "primevue/usetoast";
import { PrimeIcons } from "@primevue/core/api";
import { FilterMatchMode } from "@primevue/core/api";
import { DataTableRowClickEvent } from "primevue/datatable";

import ProgressColumn from "@/components/ProgressColumn.vue";
import { useStore } from "@/store";
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
      summary: "Error al carregar el dashboard de producció",
      life: 5000,
    });
  } finally {
    loading.value = false;
  }
};

onMounted(async () => {
  store.setMenuItem({
    icon: PrimeIcons.CHART_LINE,
    title: "Dashboard de producció",
  });
  await loadData();
});
</script>

<style scoped>
:deep(.time-overrun .p-progressbar-value) {
  background: var(--red-500);
}
</style>
