<template>
  <DataTable
    :value="workorders"
    tableStyle="min-width: 100%"
    class="clickable-rows"
    scrollable
    scrollHeight="flex"
    sort-mode="multiple"
    @row-click="onEditRow"
    paginator
    :rows="20"
    stripedRows
    :rowHover="true"
  >
    <template #header>
      <slot name="header"></slot>
    </template>
    <Column field="code" header="Codi" style="width: 15%"></Column>
    <Column header="Referencia" style="width: 40%">
      <template #body="slotProps">
        {{ referenceStore.getFullNameById(slotProps.data.referenceId) }}
      </template>
    </Column>
    <Column header="Client" style="width: 15%">
      <template #body="slotProps">
        {{
          customersStore.getCustomerNameById(
            slotProps.data.reference.customerId,
          )
        }}
      </template>
    </Column>
    <Column field="statusId" header="Estat" style="width: 10%">
      <template #body="slotProps">
        {{ lifecycleStore.getStatusName(slotProps.data.statusId) }}
      </template>
    </Column>
    <Column
      field="plannedDate"
      header="Data Prevista"
      sortable
      style="width: 12%"
    >
      <template #body="slotProps">
        {{ formatDate(slotProps.data.plannedDate) }}
      </template>
    </Column>
    <Column field="order" header="Prioritat" style="width: 10%"></Column>
    <Column
      field="plannedQuantity"
      header="Quantitat"
      style="width: 10%"
    ></Column>
    <Column>
      <template #body="slotProps">
        <i
          :class="PrimeIcons.TIMES"
          class="grid_delete_column_button"
          @click="onDeleteRow($event, slotProps.data)"
        />
      </template>
    </Column>
  </DataTable>
</template>

<script setup lang="ts">
import { PrimeIcons } from "@primevue/core/api";
import { DataTableRowClickEvent } from "primevue/datatable";
import { WorkOrder } from "../types";
import { formatDate } from "../../../utils/functions";
import { useReferenceStore } from "../../shared/store/reference";
import { useCustomersStore } from "../../sales/store/customers";
import { useLifecyclesStore } from "../../shared/store/lifecycle";

const props = defineProps<{
  workorders: Array<WorkOrder> | undefined;
}>();

const emit = defineEmits<{
  (e: "edit", workorder: WorkOrder): void;
  (e: "delete", workorder: WorkOrder): void;
}>();

const referenceStore = useReferenceStore();
const customersStore = useCustomersStore();
const lifecycleStore = useLifecyclesStore();

const onEditRow = (row: DataTableRowClickEvent) => {
  if (
    !(row.originalEvent.target as any).className.includes(
      "grid_delete_column_button",
    )
  ) {
    emit("edit", row.data);
  }
};

const onDeleteRow = (event: Event, workorder: WorkOrder) => {
  event.stopPropagation();
  emit("delete", workorder);
};
</script>
