<template>
  <Table
    phone-layout="cards"
    :card-layout="cardLayout"
    preset="crud-list"
    :columns="columns"
    :items="purchaseStore.purchaseInvoiceSeries ?? []"
    :filter-config="[]"
    :show-filter-actions="false"
    delete-column-width="5%"
    show-delete-column
    tableStyle="min-width: 100%"
    @row-click="editPurchaseInvoiceSerie"
    @create="createButtonClick"
    @delete="deletePurchaseInvoiceSerie"
  >
    <template #prepend>
      <span class="text-900 font-bold">{{ t("purchase.invoiceSeries.title") }}</span>
    </template>
  </Table>
</template>
<script setup lang="ts">
import Table from "../../../components/tables/Table.vue";
import {
  ColumnType,
  type CardLayout,
  type Column,
} from "../../../components/tables/types";
import { getNewUuid } from "../../../utils/functions";
import { PrimeIcons } from "@primevue/core/api";
import { useConfirmDelete } from "@/composables/useConfirmDelete";
import { computed, onMounted } from "vue";
import { useRouter } from "vue-router";
import { useI18n } from "vue-i18n";
import { DataTableRowClickEvent } from "primevue/datatable";
import { InvoiceSerie } from "../types";
import { useStore } from "../../../store";
import { usePurchaseInvoiceSeries } from "../store/purchaseInvoiceSeries";

const confirmDelete = useConfirmDelete();
const router = useRouter();
const store = useStore();
const purchaseStore = usePurchaseInvoiceSeries();
const { t } = useI18n();

const columns = computed<Column[]>(() => [
  {
    field: "name",
    header: t("purchase.invoiceSeries.fields.name"),
    style: "width: 20%",
  },
  {
    field: "description",
    header: t("purchase.invoiceSeries.fields.description"),
    style: "width: 50%",
  },
  {
    field: "disabled",
    header: t("purchase.invoiceSeries.fields.disabled"),
    columnType: ColumnType.Boolean,
    style: "width: 20%",
  },
]);

const cardLayout: CardLayout = {
  title: "name",
  subtitle: "description",
  meta: ["disabled"],
};

onMounted(async () => {
  await purchaseStore.fetchPurchaseInvoiceSeries();
  store.setMenuItem({
    icon: PrimeIcons.SERVER,
    title: t("purchase.invoiceSeries.title"),
  });
});
const createButtonClick = () => {
  router.push({ path: `/purchaseinvoiceserie/${getNewUuid()}` });
};

const editPurchaseInvoiceSerie = (row: DataTableRowClickEvent) => {
  router.push({ path: `/purchaseinvoiceserie/${row.data.id}` });
};

const deletePurchaseInvoiceSerie = (purchaseInvoiceSerie: InvoiceSerie) =>
  confirmDelete({
    name: purchaseInvoiceSerie.name,
    remove: () =>
      purchaseStore.deletePurchaseInvoiceSerie(purchaseInvoiceSerie.id),
    onDeleted: () => purchaseStore.fetchPurchaseInvoiceSeries(),
  });
</script>
