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
    <Column field="quantity" :header="t('purchase.receiptDetail.columns.quantity')" style="width: 7.5%" />
    <Column
      sortable
      :header="t('purchase.receiptDetail.columns.reference')"
      field="reference.code"
      style="width: 20%"
    >
      <template #body="{ data }">
        <LinkReference :id="data.referenceId" :full-name="true" />
      </template>
    </Column>
    <Column field="description" :header="t('purchase.receiptDetail.columns.description')" style="width: 20%"></Column>
    <Column field="width" :header="t('purchase.receiptDetail.columns.width')" style="width: 7.5%"></Column>
    <Column field="height" :header="t('purchase.receiptDetail.columns.height')" style="width: 7.5%"></Column>
    <Column field="lenght" :header="t('purchase.receiptDetail.columns.length')" style="width: 7.5%"></Column>
    <Column field="thickness" :header="t('purchase.receiptDetail.columns.thickness')" style="width: 7.5%"></Column>
    <Column field="diameter" :header="t('purchase.receiptDetail.columns.diameter')" style="width: 7.5%"></Column>
    <Column field="totalWeight" :header="t('purchase.receiptDetail.columns.weight')" style="width: 7.5%">
      <template #body="slotProps">
        {{ slotProps.data.totalWeight }} KG</template
      >
    </Column>
    <Column field="amount" :header="t('purchase.receiptDetail.columns.price')" style="width: 7.5%">
      <template #body="slotProps">
        {{ formatCurrency(slotProps.data.amount) }}
      </template>
    </Column>
    <Column style="width: 5%">
      <template #body="slotProps">
        <i
          v-if="slotProps.data.stockMovementId === null"
          :class="PrimeIcons.TIMES"
          :aria-label="t('purchase.receipt.actions.deleteLine')"
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
import { useI18n } from "vue-i18n";
import { ReceiptDetail } from "../types";
import { formatCurrency } from "../../../utils/functions";
import LinkReference from "../../shared/components/LinkReference.vue";

const { t } = useI18n();

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
