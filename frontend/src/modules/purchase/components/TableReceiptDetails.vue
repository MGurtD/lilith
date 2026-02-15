<template>
  <DataTable
    @row-click="onEditRow"
    :value="props.details"
    tableStyle="min-width: 100%"
    class="p-datatable-sm"
    sort-mode="single"
    sort-field="reference.code"
    :sort-order="1"
  >
    <template #header>
      <slot name="header"></slot>
    </template>
    <Column field="quantity" header="Quantitat" style="width: 7.5%" />
    <Column
      sortable
      header="Referència"
      field="reference.code"
      style="width: 20%"
    >
      <template #body="{ data }">
        <LinkReference :id="data.referenceId" :full-name="true" />
      </template>
    </Column>
    <Column field="description" header="Descripció" style="width: 20%"></Column>
    <Column field="width" header="Amplada" style="width: 7.5%"></Column>
    <Column field="height" header="Alçada" style="width: 7.5%"></Column>
    <Column field="lenght" header="Longitud" style="width: 7.5%"></Column>
    <Column field="thickness" header="Gruix" style="width: 7.5%"></Column>
    <Column field="diameter" header="Diàmetre" style="width: 7.5%"></Column>
    <Column field="totalWeight" header="Pes" style="width: 7.5%">
      <template #body="slotProps">
        {{ slotProps.data.totalWeight }} KG</template
      >
    </Column>
    <Column field="amount" header="Preu" style="width: 7.5%">
      <template #body="slotProps">
        {{ formatCurrency(slotProps.data.amount) }}
      </template>
    </Column>
    <Column style="width: 5%">
      <template #body="slotProps">
        <i
          v-if="slotProps.data.stockMovementId === null"
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
import { ReceiptDetail } from "../types";
import { formatCurrency } from "../../../utils/functions";
import LinkReference from "../../shared/components/LinkReference.vue";

const props = defineProps<{
  details: Array<ReceiptDetail> | undefined;
}>();

const emit = defineEmits<{
  (e: "edit", detail: ReceiptDetail): void;
  (e: "delete", detail: ReceiptDetail): void;
}>();

const onEditRow = (row: DataTableRowClickEvent) => {
  if (
    !(row.originalEvent.target as any).className.includes(
      "grid_delete_column_button"
    )
  ) {
    emit("edit", row.data);
  }
};

const onDeleteRow = (event: any, detail: ReceiptDetail) => {
  emit("delete", detail);
};
</script>
