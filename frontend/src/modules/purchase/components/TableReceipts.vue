<template>
  <Table
    :items="receipts ?? []"
    :columns="columns"
    :filter-config="[]"
    :filter-values="noFilters"
    :show-filter-actions="false"
    :card-layout="cardLayout"
    phone-layout="cards"
    preset="read-only"
    tableStyle="min-width: 100%"
    show-delete-column
    @create="emit('add')"
    @delete="(receipt: Receipt) => emit('delete', receipt)"
    @row-click="(event: DataTableRowClickEvent) => emit('edit', event.data)"
  >
    <template #prepend>
      <span class="text-900 font-bold">
        {{ t("purchase.receipts.associated") }}
      </span>
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
import type { DataTableRowClickEvent } from "primevue/datatable";
import { computed } from "vue";
import { useI18n } from "vue-i18n";
import type { Receipt } from "../types";
import { receiptDate } from "./receipt-date";

defineProps<{
  receipts: Array<Receipt> | undefined;
}>();

const emit = defineEmits<{
  (e: "add"): void;
  (e: "edit", receipt: Receipt): void;
  (e: "delete", receipt: Receipt): void;
}>();

const { t } = useI18n();
const noFilters = {};

const columns = computed<Column[]>(() => [
  {
    field: "number",
    header: t("purchase.receipts.columns.supplierNumber"),
    style: "width: 30%",
  },
  {
    field: "supplierNumber",
    header: t("purchase.receipts.columns.providerNumber"),
    style: "width: 35%",
  },
  {
    field: "date",
    header: t("purchase.receipts.columns.date"),
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
