<template>
  <Table
    :items="workorders"
    :columns="columns"
    :filter-config="[]"
    :show-filter-actions="false"
    :show-create="false"
    page="WorkorderPlanning"
    show-row-reorder-column
    tableStyle="min-width: 100%"
    class="p-datatable-sm small-datatable"
    sortField="order"
    :sortOrder="1"
    scrollable
    scrollHeight="flex"
    @row-reorder="onRowReorder"
  >
    <template #append>
      <Button :icon="PrimeIcons.SAVE" rounded raised @click="updateOrder" />
    </template>
    <template #body-code="{ data }">
      <LinkWorkorder :id="data.id" :code="data.code" />
    </template>
  </Table>
</template>
<script setup lang="ts">
import Table from "@/components/tables/Table.vue";
import { ColumnType, type Column } from "@/components/tables/types";
import { useI18n } from "vue-i18n";
const { t } = useI18n();
const pt = (key: string): string => t(`production.ui.${key}`);
import LinkWorkorder from "../components/LinkWorkorder.vue";
import { computed, onMounted } from "vue";
import { useWorkOrderStore } from "../store/workorder";
import { PrimeIcons } from "@primevue/core/api";
import { WorkOrder, WorkOrderOrder } from "../types";
import { useStore } from "@/store";
import { useToast } from "primevue/usetoast";

const store = useStore();
const toast = useToast();
const workorderStore = useWorkOrderStore();

const columns = computed<Column[]>(() => [
  { field: "code", header: pt("Codi") },
  { field: "status.name", header: pt("Estat") },
  {
    field: "reference.customer.comercialName",
    header: pt("Client"),
  },
  {
    field: "reference.code",
    header: pt("Referència"),
    resolver: (_value, data) => {
      const workorder = data as WorkOrder;
      return workorder.reference
        ? `${workorder.reference.code} - ${workorder.reference.description}`
        : "";
    },
  },
  {
    field: "plannedDate",
    header: pt("Data Prevista"),
    columnType: ColumnType.Date,
    style: "width: 12%",
  },
  {
    field: "order",
    header: pt("Prioritat"),
    columnType: ColumnType.Number,
  },
  {
    field: "plannedQuantity",
    header: pt("Quantitat"),
    columnType: ColumnType.Number,
  },
]);

onMounted(async () => {
  store.setMenuItem({
    icon: PrimeIcons.BUILDING,
    backButtonVisible: false,
    title: pt("Prioritzar ordres de fabricació"),
  });

  await workorderStore.fetchPlannable();
});

const workorders = computed(() => {
  return Array.isArray(workorderStore.workorders)
    ? workorderStore.workorders
    : [];
});

const onRowReorder = (event: { value: WorkOrder[] }) => {
  workorderStore.workorders = event.value.map((item, index) => ({
    ...item,
    order: index + 1,
  }));
};

const updateOrder = async () => {
  const payload: Array<WorkOrderOrder> = workorders.value.map((item) => {
    return {
      id: item.id,
      order: item.order,
    };
  });
  const response = await workorderStore.priorize(payload);
  if (response.result) {
    toast.add({
      severity: "success",
      summary: pt("Ordres de fabricació actualitzades"),
      detail: t("production.messages.updatedWorkorderPlanning"),
      life: 3000,
    });
  } else {
    toast.add({
      severity: "error",
      summary: pt("Error actualitzant les ordres de fabricació"),
      detail:
        response.errors?.join(", ") || "S'ha produït un error desconegut.",
      life: 5000,
    });
  }
};
</script>
