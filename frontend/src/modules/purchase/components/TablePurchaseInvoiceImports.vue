<template>
  <DataTable
    @row-click="onEditRow"
    :value="props.purchaseInvoiceImports"
    tableStyle="min-width: 100%"
  >
    <Button
      @click="onAdd"
      rounded
      :icon="PrimeIcons.PLUS"
      class="grid_add_row_button"
      style="margin-right: 1.5rem"
    />

    <Column field="baseAmount" header="Base" style="width: 25%">
      <template #body="slotProps"> {{ slotProps.data.baseAmount }} € </template>
    </Column>
    <Column field="taxId" header="% IVA" style="width: 25%">
      <template #body="slotProps">
        {{ getTaxNameById(slotProps.data.taxId) }}
      </template>
    </Column>
    <Column field="taxAmount" header="Cuota IVA" style="width: 25%">
      <template #body="slotProps"> {{ slotProps.data.taxAmount }} € </template>
    </Column>
    <Column field="netAmount" header="Total" style="width: 25%">
      <template #body="slotProps"> {{ slotProps.data.netAmount }} € </template>
    </Column>
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
import { PurchaseInvoiceImport } from "../types";
import { getNewUuid } from "../../../utils/functions";
import { usePurchaseMasterDataStore } from "../store/purchase";

const props = defineProps<{
  purchaseInvoiceImports: Array<PurchaseInvoiceImport> | undefined;
}>();

const emit = defineEmits<{
  (e: "add", invoiceImport: PurchaseInvoiceImport): void;
  (e: "edit", invoiceImport: PurchaseInvoiceImport): void;
  (e: "delete", invoiceImport: PurchaseInvoiceImport): void;
}>();

const purchaseMasterData = usePurchaseMasterDataStore();

const getTaxNameById = (taxId: string) => {
  const tax = purchaseMasterData.masterData.taxes?.find((t) => t.id === taxId);
  if (tax) return tax.percentatge;
};

const onAdd = () => {
  const tax = purchaseMasterData.masterData.taxes?.find((t) =>
    t.name.includes("21")
  );

  const defaultImport = {
    id: getNewUuid(),
    baseAmount: null,
    taxId: tax ? tax.id : "",
    taxAmount: 0,
    netAmount: 0,
    purchaseInvoiceId: "",
  } as PurchaseInvoiceImport;
  emit("add", defaultImport);
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

const onDeleteRow = (event: any, invoiceImport: PurchaseInvoiceImport) => {
  emit("delete", invoiceImport);
};
</script>
