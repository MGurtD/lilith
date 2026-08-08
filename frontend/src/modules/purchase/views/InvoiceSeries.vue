<template>
  <DataTable
    :value="purchaseStore.purchaseInvoiceSeries"
    tableStyle="min-width: 100%"
    @row-click="editPurchaseInvoiceSerie"
  >
    <template #header>
      <div
        class="flex flex-wrap align-items-center justify-content-between gap-2"
      >
        <span class="text-900 font-bold">{{ t("purchase.invoiceSeries.title") }}</span>
        <Button
          :icon="PrimeIcons.PLUS"
          rounded
          raised
          @click="createButtonClick"
        />
      </div>
    </template>
    <Column field="name" :header="t('purchase.invoiceSeries.fields.name')" style="width: 20%"></Column>
    <Column field="description" :header="t('purchase.invoiceSeries.fields.description')" style="width: 50%"></Column>
    <Column :header="t('purchase.invoiceSeries.fields.disabled')" style="width: 20%">
      <template #body="slotProps">
        <BooleanColumn :value="slotProps.data.disabled" />
      </template>
    </Column>
    <Column>
      <template #body="slotProps">
        <i
          :class="PrimeIcons.TIMES"
          class="grid_delete_column_button"
          @click="deletePurchaseInvoiceSerie($event, slotProps.data)"
        />
      </template>
    </Column>
  </DataTable>
</template>
<script setup lang="ts">
import { getNewUuid } from "../../../utils/functions";
import { PrimeIcons } from "@primevue/core/api";
import { useToast } from "primevue/usetoast";
import { useConfirm } from "primevue/useconfirm";
import { onMounted, ref } from "vue";
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
  if (
    !(row.originalEvent.target as any).className.includes(
      "grid_delete_column_button"
    )
  ) {
    router.push({ path: `/purchaseinvoiceserie/${row.data.id}` });
  }
};

const deletePurchaseInvoiceSerie = (
  event: any,
  purchaseInvoiceSerie: InvoiceSerie
) => {
  confirm.require({
    target: event.currentTarget,
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
