<template>
  <Table
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
  type Column,
} from "../../../components/tables/types";
import { getNewUuid } from "../../../utils/functions";
import { PrimeIcons } from "@primevue/core/api";
import { useToast } from "primevue/usetoast";
import { useConfirm } from "primevue/useconfirm";
import { computed, onMounted } from "vue";
import { useRouter } from "vue-router";
import { useI18n } from "vue-i18n";
import { DataTableRowClickEvent } from "primevue/datatable";
import { InvoiceSerie } from "../types";
import { useStore } from "../../../store";
import { usePurchaseInvoiceSeries } from "../store/purchaseInvoiceSeries";

const toast = useToast();
const confirm = useConfirm();
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

const deletePurchaseInvoiceSerie = (purchaseInvoiceSerie: InvoiceSerie) => {
  confirm.require({
    message: t("purchase.invoiceSeries.messages.confirmDelete", {
      name: purchaseInvoiceSerie.name,
    }),
    icon: "pi pi-question-circle",
    acceptIcon: "pi pi-check",
    rejectIcon: "pi pi-times",
    accept: async () => {
      const deleted = await purchaseStore.deletePurchaseInvoiceSerie(
        purchaseInvoiceSerie.id
      );

      if (deleted) {
        toast.add({
          severity: "success",
          summary: t("purchase.messages.deleted"),
          life: 3000,
        });
        await purchaseStore.fetchPurchaseInvoiceSeries();
      }
    },
  });
};
</script>
