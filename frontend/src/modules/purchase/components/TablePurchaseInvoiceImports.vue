<template>
  <Table
    :items="purchaseInvoiceImports ?? []"
    :columns="columns"
    :filter-config="[]"
    :filter-values="noFilters"
    :show-filter-actions="false"
    :card-layout="cardLayout"
    :row-class="rowClass"
    phone-layout="cards"
    preset="read-only"
    tableStyle="min-width: 100%"
    show-delete-column
    @create="onAdd"
    @delete="(row: PurchaseInvoiceImport) => emit('delete', row)"
    @row-click="(event: DataTableRowClickEvent) => emit('edit', event.data)"
  >
    <template #prepend>
      <span class="text-900 font-bold">
        {{ t("purchase.purchaseInvoiceImport.title") }}
      </span>
    </template>
    <template #body-baseAmount="{ data }">
      <span class="import-row-base">
        <i
          v-if="rowWarnings?.[data.id]?.length"
          v-tooltip.top="rowWarnings[data.id].join(' · ')"
          class="pi pi-exclamation-triangle import-row-warning-icon"
          :aria-label="rowWarnings[data.id].join('. ')"
        />
        {{ formatCurrency(data.baseAmount ?? 0) }}
      </span>
    </template>
    <template #body-taxId="{ data }">
      <span
        v-if="getTaxRate(data.taxId) == null && pendingTaxRates?.[data.id] != null"
        v-tooltip.top="t('purchase.purchaseInvoiceImport.pendingTax')"
        class="import-row-pending-tax"
      >
        {{ pendingTaxRates[data.id] }}
        <i class="pi pi-exclamation-triangle" aria-hidden="true" />
      </span>
      <template v-else>{{ getTaxRate(data.taxId) ?? "" }}</template>
    </template>
  </Table>
</template>

<script setup lang="ts">
import Table from "@/components/tables/Table.vue";
import {
  ColumnType,
  type CardLayout,
  type Column,
} from "@/components/tables/types";
import { formatCurrency, getNewUuid } from "@/utils/functions";
import type { DataTableRowClickEvent } from "primevue/datatable";
import { computed } from "vue";
import { useI18n } from "vue-i18n";
import { usePurchaseMasterDataStore } from "../store/purchase";
import type { PurchaseInvoiceImport } from "../types";

const props = defineProps<{
  purchaseInvoiceImports: Array<PurchaseInvoiceImport> | undefined;
  /** Review hints per import id, e.g. from a PDF import. */
  rowWarnings?: Record<string, string[]>;
  /** VAT rate read from a document for lines still without a tax, per import id. */
  pendingTaxRates?: Record<string, number>;
}>();

const emit = defineEmits<{
  (e: "add", invoiceImport: PurchaseInvoiceImport): void;
  (e: "edit", invoiceImport: PurchaseInvoiceImport): void;
  (e: "delete", invoiceImport: PurchaseInvoiceImport): void;
}>();

const purchaseMasterData = usePurchaseMasterDataStore();
const { t } = useI18n();
const noFilters = {};

const getTaxRate = (taxId: unknown): number | undefined =>
  purchaseMasterData.masterData.taxes?.find((tax) => tax.id === taxId)
    ?.percentatge;

const columns = computed<Column[]>(() => [
  {
    field: "baseAmount",
    header: t("purchase.purchaseInvoiceImport.columns.base"),
    columnType: ColumnType.Currency,
    style: "width: 25%; text-align: right",
  },
  {
    field: "taxId",
    header: t("purchase.purchaseInvoiceImport.columns.tax"),
    columnType: ColumnType.Lookup,
    resolver: (value) => getTaxRate(value) ?? "",
    style: "width: 25%",
  },
  {
    field: "taxAmount",
    header: t("purchase.purchaseInvoiceImport.columns.taxAmount"),
    columnType: ColumnType.Currency,
    style: "width: 25%; text-align: right",
  },
  {
    field: "netAmount",
    header: t("common.total"),
    columnType: ColumnType.Currency,
    style: "width: 22%; text-align: right",
  },
]);

const cardLayout: CardLayout = {
  title: "netAmount",
  subtitle: "taxId",
  meta: ["baseAmount", "taxAmount"],
};

const rowClass = (row: PurchaseInvoiceImport) =>
  props.rowWarnings?.[row.id]?.length ? "import-row-warning" : undefined;

const onAdd = () => {
  const tax = purchaseMasterData.masterData.taxes?.find((t) =>
    t.name.includes("21"),
  );

  emit("add", {
    id: getNewUuid(),
    baseAmount: null,
    taxId: tax ? tax.id : "",
    taxAmount: 0,
    netAmount: 0,
    purchaseInvoiceId: "",
  });
};
</script>

<style>
/* Unscoped: the rows are rendered by Table.vue, outside this component's scope. */
.p-datatable-tbody > tr.import-row-warning > td {
  background: color-mix(in srgb, var(--p-yellow-500) 12%, transparent);
}
</style>

<style scoped>
.import-row-base {
  display: inline-flex;
  align-items: center;
  gap: 0.35rem;
}

.import-row-pending-tax {
  display: inline-flex;
  align-items: center;
  gap: 0.35rem;
  color: var(--p-yellow-700);
}

.import-row-warning-icon {
  color: var(--p-yellow-600);
}
</style>
