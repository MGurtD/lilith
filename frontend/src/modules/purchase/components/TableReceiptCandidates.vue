<template>
  <Table
    :items="candidates"
    :columns="columns"
    :filter-config="[]"
    :filter-values="noFilters"
    :show-filter-actions="false"
    :show-create="false"
    :card-layout="cardLayout"
    :selection="selection"
    data-key="id"
    phone-layout="cards"
    preset="detail-lines"
    tableStyle="min-width: 100%"
    show-selection-column
    @update:selection="(value: ReceiptCandidate[]) => emit('update:selection', value)"
  >
    <template #prepend>
      <span class="text-900 font-bold">
        {{ t("purchase.invoiceImport.receipts.title") }}
      </span>
    </template>
    <template #body-matchReason="{ data }">
      <Tag
        v-if="data.matchReason"
        severity="info"
        :value="matchLabel(data.matchReason)"
      />
    </template>
    <template #empty>
      {{ t("purchase.invoiceImport.receipts.empty") }}
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
import { computed } from "vue";
import { useI18n } from "vue-i18n";
import type { ReceiptCandidate } from "../types";
import { receiptDate } from "./receipt-date";

defineProps<{
  candidates: ReceiptCandidate[];
  selection: ReceiptCandidate[];
}>();

const emit = defineEmits<{
  (e: "update:selection", value: ReceiptCandidate[]): void;
}>();

const { t } = useI18n();
const noFilters = {};

const matchLabel = (reason: string): string =>
  reason === "DeliveryNoteNumber"
    ? t("purchase.invoiceImport.receipts.match.deliveryNoteNumber")
    : t("purchase.invoiceImport.receipts.match.amount");

const columns = computed<Column[]>(() => [
  {
    field: "supplierNumber",
    header: t("purchase.invoiceImport.receipts.columns.supplierNumber"),
    style: "width: 25%",
  },
  {
    field: "number",
    header: t("purchase.invoiceImport.receipts.columns.number"),
    style: "width: 18%",
  },
  {
    field: "date",
    header: t("purchase.invoiceImport.receipts.columns.date"),
    columnType: ColumnType.Date,
    resolver: receiptDate,
    style: "width: 17%",
  },
  {
    field: "amount",
    header: t("purchase.invoiceImport.receipts.columns.amount"),
    columnType: ColumnType.Currency,
    style: "width: 18%; text-align: right",
  },
  {
    field: "matchReason",
    header: t("purchase.invoiceImport.receipts.columns.match"),
    style: "width: 22%",
  },
]);

const cardLayout: CardLayout = {
  title: "supplierNumber",
  subtitle: "date",
  trailing: "amount",
  meta: ["number", "matchReason"],
};
</script>
