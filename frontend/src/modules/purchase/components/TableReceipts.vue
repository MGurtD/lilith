<template>
  <DataTable
    @row-click="onEditRow"
    :value="props.receipts"
    tableStyle="min-width: 100%"
    class="p-datatable-sm"
  >
    <template #header>
      <slot name="header">
        <div
          class="flex flex-wrap align-items-center justify-content-between gap-2"
        >
          <span class="text-l text-900 font-bold">Albarans associats</span>
          <div>
            <Button
              :size="'small'"
              :icon="PrimeIcons.PLUS"
              rounded
              @click="onAddRow"
            />
          </div>
        </div>
      </slot>
    </template>
    <Column field="number" header="Número Albarà" style="width: 15%" />
    <Column
      field="supplierNumber"
      header="Número Proveïdor"
      style="width: 15%"
    />
    <Column field="date" header="Data" style="width: 15%">
      <template #body="slotProps">
        {{ formatDate(slotProps.data.date) }}
      </template>
    </Column>
    <!-- <Column field="statusId" header="Estat" style="width: 15%">
      <template #body="slotProps">
        {{
          lifecycleStore.getStatusNameById("Receipts", slotProps.data.statusId)
        }}
      </template>
    </Column> -->
    <Column style="width: 10%">
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
import { Receipt } from "../types";
import { formatDate } from "../../../utils/functions";

const props = defineProps<{
  receipts: Array<Receipt> | undefined;
}>();

const emit = defineEmits<{
  (e: "add"): void;
  (e: "edit", receipt: Receipt): void;
  (e: "delete", receipt: Receipt): void;
}>();

const onAddRow = () => {
  emit("add");
};

const onEditRow = (row: DataTableRowClickEvent) => {
  if (
    !(row.originalEvent.target as any).className.includes(
      "grid_delete_column_button"
    )
  ) {
    emit("edit", row.data);
  }
};

const onDeleteRow = (event: any, receipt: Receipt) => {
  emit("delete", receipt);
};
</script>
