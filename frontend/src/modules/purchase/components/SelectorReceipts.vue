<template>
  <Table
    v-model:filter-values="filter"
    v-model:selection="selectedReceipts"
    :items="filteredReceipts"
    :columns="columns"
    :filter-config="filterConfig"
    :show-filter-actions="false"
    :show-create="false"
    :card-layout="cardLayout"
    data-key="id"
    phone-layout="cards"
    preset="selector"
    selection-mode="multiple"
    tableStyle="min-width: 100%"
    show-selection-column
  />
  <div class="receipt-selector__actions">
    <Button
      icon="pi pi-check"
      :label="t('purchase.receiptSelector.actions.select')"
      :disabled="selectedReceipts.length === 0"
      @click="emit('selected', selectedReceipts)"
    />
  </div>
</template>

<script setup lang="ts">
import Table from "@/components/tables/Table.vue";
import type { FilterConfig } from "@/components/tables/TableFilter.vue";
import {
  ColumnType,
  type CardLayout,
  type Column,
} from "@/components/tables/types";
import { computed, ref } from "vue";
import { useI18n } from "vue-i18n";
import type { Receipt } from "../types";
import { receiptDate } from "./receipt-date";

const props = defineProps<{
  receipts: Array<Receipt> | undefined;
}>();

const emit = defineEmits<{
  (e: "selected", receipts: Array<Receipt>): void;
}>();

const { t } = useI18n();
const selectedReceipts = ref<Receipt[]>([]);
const filter = ref({ search: "" });

const filterConfig = computed<FilterConfig[]>(() => [
  {
    key: "search",
    label: t("purchase.receiptSelector.search"),
    type: "text",
    placeholder: t("purchase.receiptSelector.search"),
    size: "md",
  },
]);

const filteredReceipts = computed(() => {
  const search = (filter.value.search ?? "").trim().toLocaleLowerCase();
  return (props.receipts ?? []).filter(
    (receipt) =>
      !search ||
      receipt.number.toString().toLocaleLowerCase().includes(search) ||
      receipt.supplierNumber.toLocaleLowerCase().includes(search),
  );
});

const columns = computed<Column[]>(() => [
  {
    field: "number",
    header: t("purchase.receiptSelector.columns.number"),
    style: "width: 32%",
  },
  {
    field: "supplierNumber",
    header: t("purchase.receiptSelector.columns.supplierNumber"),
    style: "width: 32%",
  },
  {
    field: "date",
    header: t("purchase.receiptSelector.columns.date"),
    columnType: ColumnType.Date,
    resolver: receiptDate,
    style: "width: 32%",
  },
]);

const cardLayout: CardLayout = {
  title: "number",
  subtitle: "supplierNumber",
  trailing: "date",
};
</script>

<style scoped>
.receipt-selector__actions {
  display: flex;
  justify-content: flex-end;
  margin-top: 1rem;
}
</style>
