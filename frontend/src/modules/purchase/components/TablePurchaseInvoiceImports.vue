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
      :aria-label="t('purchase.purchaseInvoiceImport.actions.add')"
      :title="t('purchase.purchaseInvoiceImport.actions.add')"
      class="grid_add_row_button"
      style="margin-right: 1.5rem"
    />

    <Column field="baseAmount" :header="t('purchase.purchaseInvoiceImport.columns.base')" style="width: 25%">
      <template #body="slotProps"> {{ slotProps.data.baseAmount }} € </template>
    </Column>
    <Column field="taxId" :header="t('purchase.purchaseInvoiceImport.columns.tax')" style="width: 25%">
      <template #body="slotProps">
        {{ getTaxNameById(slotProps.data.taxId) }}
      </template>
    </Column>
    <Column field="taxAmount" :header="t('purchase.purchaseInvoiceImport.columns.taxAmount')" style="width: 25%">
      <template #body="slotProps"> {{ slotProps.data.taxAmount }} € </template>
    </Column>
    <Column field="netAmount" :header="t('common.total')" style="width: 25%">
      <template #body="slotProps"> {{ slotProps.data.netAmount }} € </template>
    </Column>
    <Column style="width: 10%">
      <template #body="slotProps">
        <i
          :class="PrimeIcons.TIMES"
          class="grid_delete_column_button"
          :aria-label="t('purchase.purchaseInvoiceImport.actions.delete')"
          :title="t('purchase.purchaseInvoiceImport.actions.delete')"
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
import { useI18n } from "vue-i18n";

const props = defineProps<{
  purchaseInvoiceImports: Array<PurchaseInvoiceImport> | undefined;
}>();

const emit = defineEmits<{
  (e: "add", invoiceImport: PurchaseInvoiceImport): void;
  (e: "edit", invoiceImport: PurchaseInvoiceImport): void;
  (e: "delete", invoiceImport: PurchaseInvoiceImport): void;
}>();

const purchaseMasterData = usePurchaseMasterDataStore();
const { t } = useI18n();

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
